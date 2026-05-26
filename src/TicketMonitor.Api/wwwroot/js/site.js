// ============================================================
// TicketMonitor - Core JS: Auth, JWT, Notifications, Helpers
// ============================================================

const API = {
    async request(url, options = {}) {
        const token = Auth.getToken();
        const headers = { 'Content-Type': 'application/json', ...options.headers };
        if (token) headers['Authorization'] = `Bearer ${token}`;

        const res = await fetch(url, { ...options, headers });

        if (res.status === 401) {
            Auth.handleExpired();
            throw new Error('Unauthorized');
        }

        return res;
    },

    get: (url) => API.request(url),
    post: (url, body) => API.request(url, { method: 'POST', body: JSON.stringify(body) }),
    put: (url, body) => API.request(url, { method: 'PUT', body: JSON.stringify(body) }),
    delete: (url) => API.request(url, { method: 'DELETE' }),
};

const Auth = {
    getToken() { return localStorage.getItem('tm_token'); },
    getUser() {
        try { return JSON.parse(localStorage.getItem('tm_user') || 'null'); }
        catch { return null; }
    },
    setSession(token, user) {
        localStorage.setItem('tm_token', token);
        localStorage.setItem('tm_user', JSON.stringify(user));
    },
    clearSession() {
        localStorage.removeItem('tm_token');
        localStorage.removeItem('tm_user');
    },
    isLoggedIn() {
        const token = this.getToken();
        if (!token) return false;
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            if (payload.exp * 1000 < Date.now()) {
                this.handleExpired();
                return false;
            }
            return true;
        } catch { return false; }
    },
    getExpiresIn() {
        const token = this.getToken();
        if (!token) return 0;
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            return Math.max(0, payload.exp * 1000 - Date.now());
        } catch { return 0; }
    },
    hasRole(role) {
        const user = this.getUser();
        if (!user || !user.roles) return false;
        return user.roles.includes(role);
    },
    hasAnyRole(...roles) {
        return roles.some(r => this.hasRole(r));
    },
    handleExpired() {
        this.clearSession();
        Notifications.warning('Session expired. Please log in again.');
        setTimeout(() => { window.location.href = '/AuthPage/Login'; }, 1500);
    },
    async logout() {
        try { await API.post('/api/auth/logout', {}); } catch { }
        this.clearSession();
        window.location.href = '/AuthPage/Login';
    },
    scheduleExpiryCheck() {
        const ms = this.getExpiresIn();
        if (ms <= 0) return;
        // Warn 2 minutes before expiry
        const warnMs = ms - 2 * 60 * 1000;
        if (warnMs > 0) {
            setTimeout(() => {
                Notifications.warning('Your session expires in 2 minutes. Please save your work.');
            }, warnMs);
        }
        setTimeout(() => this.handleExpired(), ms);
    }
};

const Notifications = {
    container: null,
    init() {
        if (this.container) return;
        this.container = document.createElement('div');
        this.container.id = 'tm-notifications';
        this.container.style.cssText = `
            position: fixed; top: 20px; right: 20px; z-index: 9999;
            display: flex; flex-direction: column; gap: 8px; max-width: 360px;
        `;
        document.body.appendChild(this.container);
    },
    show(msg, type = 'info', duration = 4000) {
        this.init();
        const colors = {
            success: '#10b981', error: '#ef4444',
            warning: '#f59e0b', info: '#6366f1'
        };
        const icons = { success: '✓', error: '✕', warning: '⚠', info: 'ℹ' };
        const el = document.createElement('div');
        el.style.cssText = `
            background: #1e293b; border-left: 4px solid ${colors[type]};
            color: #e2e8f0; padding: 14px 18px; border-radius: 8px;
            box-shadow: 0 8px 32px rgba(0,0,0,0.4);
            font-family: 'DM Sans', sans-serif; font-size: 14px; font-weight: 500;
            display: flex; align-items: center; gap: 10px;
            animation: slideIn 0.3s ease; cursor: pointer;
        `;
        el.innerHTML = `<span style="color:${colors[type]};font-size:16px">${icons[type]}</span>${msg}`;
        el.onclick = () => el.remove();
        this.container.appendChild(el);
        if (duration > 0) setTimeout(() => el.remove(), duration);
        return el;
    },
    success: (msg) => Notifications.show(msg, 'success'),
    error: (msg) => Notifications.show(msg, 'error'),
    warning: (msg) => Notifications.show(msg, 'warning'),
    info: (msg) => Notifications.show(msg, 'info'),
};

// Add CSS for notification animation
const style = document.createElement('style');
style.textContent = `
@keyframes slideIn { from { opacity: 0; transform: translateX(40px); } to { opacity: 1; transform: translateX(0); } }
`;
document.head.appendChild(style);

// SignalR manager
const TM_SignalR = {
    connection: null,
    async connect() {
        if (!Auth.isLoggedIn()) return;
        if (typeof signalR === 'undefined') return;

        const token = Auth.getToken();
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/tickets', { accessTokenFactory: () => token })
            .withAutomaticReconnect()
            .build();

        this.connection.on('TicketCreated', (ticket) => {
            Notifications.info(`New ticket: "${ticket.title}"`);
            document.dispatchEvent(new CustomEvent('tm:ticketCreated', { detail: ticket }));
        });

        this.connection.on('TicketUpdated', (data) => {
            document.dispatchEvent(new CustomEvent('tm:ticketUpdated', { detail: data }));
        });

        this.connection.on('TicketDeleted', (data) => {
            Notifications.warning(`Ticket #${data.ticketId} was deleted`);
            document.dispatchEvent(new CustomEvent('tm:ticketDeleted', { detail: data }));
        });

        this.connection.on('StatusChanged', (data) => {
            Notifications.info(`Ticket #${data.ticketId} status: ${data.oldStatus} → ${data.newStatus}`);
            document.dispatchEvent(new CustomEvent('tm:statusChanged', { detail: data }));
        });

        this.connection.on('CommentAdded', (comment) => {
            document.dispatchEvent(new CustomEvent('tm:commentAdded', { detail: comment }));
        });

        this.connection.on('TicketAssigned', (data) => {
            document.dispatchEvent(new CustomEvent('tm:ticketAssigned', { detail: data }));
        });

        try {
            await this.connection.start();
            console.log('SignalR connected');
        } catch (e) {
            console.warn('SignalR connection failed:', e);
        }
    },

    async joinTicket(ticketId) {
        if (this.connection?.state === 'Connected')
            await this.connection.invoke('JoinTicketGroup', String(ticketId));
    },

    async leaveTicket(ticketId) {
        if (this.connection?.state === 'Connected')
            await this.connection.invoke('LeaveTicketGroup', String(ticketId));
    }
};

// Navbar: update based on auth state
function updateNavbar() {
    const user = Auth.getUser();
    const isLoggedIn = Auth.isLoggedIn();

    const loginLink = document.getElementById('nav-login');
    const logoutBtn = document.getElementById('nav-logout');
    const usernameEl = document.getElementById('nav-username');
    const adminLink = document.getElementById('nav-admin');
    const dashboardLink = document.getElementById('nav-dashboard');

    if (loginLink) loginLink.style.display = isLoggedIn ? 'none' : '';
    if (logoutBtn) logoutBtn.style.display = isLoggedIn ? '' : 'none';
    if (usernameEl) {
        if (isLoggedIn && user) {
            usernameEl.textContent = user.username;
            usernameEl.style.display = '';
        } else {
            usernameEl.style.display = 'none';
        }
    }
    if (adminLink) adminLink.style.display = (isLoggedIn && Auth.hasAnyRole('Administrator')) ? '' : 'none';
    if (dashboardLink) dashboardLink.style.display = (isLoggedIn && Auth.hasAnyRole('Administrator', 'Manager')) ? '' : 'none';
}

// Route protection
function requireAuth(roles) {
    if (!Auth.isLoggedIn()) {
        window.location.href = '/AuthPage/Login';
        return false;
    }
    if (roles && roles.length > 0 && !Auth.hasAnyRole(...roles)) {
        window.location.href = '/HomePage/Index?error=forbidden';
        return false;
    }
    return true;
}

// Helpers
const Helpers = {
    statusBadge(status) {
        const map = {
            'Open': 'badge-open',
            'InProgress': 'badge-inprogress',
            'Resolved': 'badge-resolved',
            'Closed': 'badge-closed'
        };
        const labels = { 'Open': 'Open', 'InProgress': 'In Progress', 'Resolved': 'Resolved', 'Closed': 'Closed' };
        return `<span class="tm-badge ${map[status] || ''}">${labels[status] || status}</span>`;
    },
    priorityBadge(priority) {
        const map = {
            'Low': 'priority-low',
            'Medium': 'priority-medium',
            'High': 'priority-high',
            'Critical': 'priority-critical'
        };
        return `<span class="tm-badge ${map[priority] || ''}">${priority}</span>`;
    },
    formatDate(d) {
        if (!d) return '—';
        return new Date(d).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
    },
    formatDateTime(d) {
        if (!d) return '—';
        return new Date(d).toLocaleString('en-US', { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
    },
    timeAgo(d) {
        if (!d) return '';
        const diff = Date.now() - new Date(d).getTime();
        const m = Math.floor(diff / 60000);
        if (m < 1) return 'just now';
        if (m < 60) return `${m}m ago`;
        const h = Math.floor(m / 60);
        if (h < 24) return `${h}h ago`;
        return `${Math.floor(h / 24)}d ago`;
    },
    escapeHtml(str) {
        const d = document.createElement('div');
        d.appendChild(document.createTextNode(str));
        return d.innerHTML;
    }
};

// On DOM ready
document.addEventListener('DOMContentLoaded', () => {
    updateNavbar();

    const logoutBtn = document.getElementById('nav-logout');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', (e) => {
            e.preventDefault();
            Auth.logout();
        });
    }

    if (Auth.isLoggedIn()) {
        Auth.scheduleExpiryCheck();
        TM_SignalR.connect();
    }
});