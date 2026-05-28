// ============================================================
// TicketMonitor — site.js
// Зависит от i18n.js (подключается раньше в _Layout.cshtml)
// ============================================================

var API = {
    request: function (url, options) {
        options = options || {};
        var token = Auth.getToken();
        var headers = Object.assign({ 'Content-Type': 'application/json' }, options.headers || {});
        if (token) headers['Authorization'] = 'Bearer ' + token;
        return fetch(url, Object.assign({}, options, { headers: headers }))
            .then(function (res) {
                if (res.status === 401) {
                    Auth.handleExpired();
                    throw new Error('Unauthorized');
                }
                return res;
            });
    },
    get: function (url) { return API.request(url); },
    post: function (url, body) { return API.request(url, { method: 'POST', body: JSON.stringify(body) }); },
    put: function (url, body) { return API.request(url, { method: 'PUT', body: JSON.stringify(body) }); },
    delete: function (url) { return API.request(url, { method: 'DELETE' }); }
};

var Auth = {
    getToken: function () { return localStorage.getItem('tm_token'); },
    getUser: function () {
        try { return JSON.parse(localStorage.getItem('tm_user') || 'null'); } catch (e) { return null; }
    },
    setSession: function (token, user) {
        localStorage.setItem('tm_token', token);
        localStorage.setItem('tm_user', JSON.stringify(user));
    },
    clearSession: function () {
        localStorage.removeItem('tm_token');
        localStorage.removeItem('tm_user');
    },
    isLoggedIn: function () {
        var token = this.getToken();
        if (!token) return false;
        try {
            var payload = JSON.parse(atob(token.split('.')[1]));
            if (payload.exp * 1000 < Date.now()) { this.handleExpired(); return false; }
            return true;
        } catch (e) { return false; }
    },
    getExpiresIn: function () {
        var token = this.getToken();
        if (!token) return 0;
        try {
            var payload = JSON.parse(atob(token.split('.')[1]));
            return Math.max(0, payload.exp * 1000 - Date.now());
        } catch (e) { return 0; }
    },
    hasRole: function (role) {
        var user = this.getUser();
        return !!(user && user.roles && user.roles.indexOf(role) !== -1);
    },
    hasAnyRole: function () {
        var self = this;
        for (var i = 0; i < arguments.length; i++) {
            if (self.hasRole(arguments[i])) return true;
        }
        return false;
    },
    handleExpired: function () {
        this.clearSession();
        Notifications.warning(window.t ? t('notify.session_expired') : 'Session expired');
        setTimeout(function () { window.location.href = '/AuthPage/Login'; }, 1500);
    },
    logout: function () {
        var self = this;
        return API.post('/api/auth/logout', {})
            .catch(function () { })
            .finally(function () {
                self.clearSession();
                window.location.href = '/AuthPage/Login';
            });
    },
    scheduleExpiryCheck: function () {
        var self = this;
        var ms = this.getExpiresIn();
        if (ms <= 0) return;
        var warnMs = ms - 2 * 60 * 1000;
        if (warnMs > 0) {
            setTimeout(function () {
                Notifications.warning(t('notify.session_expiring'));
            }, warnMs);
        }
        setTimeout(function () { self.handleExpired(); }, ms);
    }
};

var Notifications = {
    _container: null,
    _init: function () {
        if (this._container) return;
        this._container = document.createElement('div');
        this._container.style.cssText =
            'position:fixed;top:20px;right:20px;z-index:9999;' +
            'display:flex;flex-direction:column;gap:8px;max-width:360px;pointer-events:none';
        document.body.appendChild(this._container);
    },
    show: function (msg, type, duration) {
        type = type || 'info';
        if (duration === undefined) duration = 4000;
        this._init();
        var colors = { success: '#10b981', error: '#ef4444', warning: '#f59e0b', info: '#6366f1' };
        var icons = { success: '✓', error: '✕', warning: '⚠', info: 'ℹ' };
        var el = document.createElement('div');
        el.style.cssText =
            'background:#1e293b;border-left:4px solid ' + colors[type] + ';' +
            'color:#e2e8f0;padding:14px 18px;border-radius:8px;' +
            'box-shadow:0 8px 32px rgba(0,0,0,0.4);' +
            'font-family:"DM Sans",sans-serif;font-size:14px;font-weight:500;' +
            'display:flex;align-items:center;gap:10px;' +
            'animation:tmSlideIn 0.3s ease;cursor:pointer;pointer-events:auto';
        el.innerHTML = '<span style="color:' + colors[type] + ';font-size:16px">' +
            icons[type] + '</span>' + msg;
        el.addEventListener('click', function () { el.remove(); });
        this._container.appendChild(el);
        if (duration > 0) { setTimeout(function () { if (el.parentNode) el.remove(); }, duration); }
        return el;
    },
    success: function (msg) { return Notifications.show(msg, 'success'); },
    error: function (msg) { return Notifications.show(msg, 'error'); },
    warning: function (msg) { return Notifications.show(msg, 'warning'); },
    info: function (msg) { return Notifications.show(msg, 'info'); }
};

// Добавляем CSS-анимацию тостов
(function () {
    var s = document.createElement('style');
    s.textContent =
        '@keyframes tmSlideIn{from{opacity:0;transform:translateX(40px)}to{opacity:1;transform:translateX(0)}}' +
        '.lang-btn{background:none;border:none;cursor:pointer;color:var(--text-muted);' +
        'font-size:0.75rem;font-weight:600;font-family:var(--font-mono);padding:2px 4px;' +
        'border-radius:4px;transition:color 0.15s;letter-spacing:0.05em}' +
        '.lang-btn:hover{color:var(--text-primary)}' +
        '.lang-btn.active{color:var(--accent)}' +
        '#lang-switcher{display:flex;align-items:center;gap:4px}';
    document.head.appendChild(s);
}());

// SignalR
var TM_SignalR = {
    connection: null,
    connect: function () {
        if (!Auth.isLoggedIn() || typeof signalR === 'undefined') return;
        var token = Auth.getToken();
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/tickets', { accessTokenFactory: function () { return token; } })
            .withAutomaticReconnect()
            .build();

        this.connection.on('TicketCreated', function (ticket) {
            Notifications.info(t('notify.ticket_created', { title: ticket.title }));
            document.dispatchEvent(new CustomEvent('tm:ticketCreated', { detail: ticket }));
        });
        this.connection.on('TicketUpdated', function (data) {
            document.dispatchEvent(new CustomEvent('tm:ticketUpdated', { detail: data }));
        });
        this.connection.on('TicketDeleted', function (data) {
            Notifications.warning(t('notify.ticket_deleted', { id: data.ticketId }));
            document.dispatchEvent(new CustomEvent('tm:ticketDeleted', { detail: data }));
        });
        this.connection.on('StatusChanged', function (data) {
            Notifications.info(t('notify.status_changed', {
                id: data.ticketId,
                old: t('status.' + data.oldStatus),
                new: t('status.' + data.newStatus)
            }));
            document.dispatchEvent(new CustomEvent('tm:statusChanged', { detail: data }));
        });
        this.connection.on('CommentAdded', function (comment) {
            document.dispatchEvent(new CustomEvent('tm:commentAdded', { detail: comment }));
        });
        this.connection.on('TicketAssigned', function (data) {
            document.dispatchEvent(new CustomEvent('tm:ticketAssigned', { detail: data }));
        });

        this.connection.start().catch(function (e) { console.warn('SignalR:', e); });
    },
    joinTicket: function (ticketId) {
        if (this.connection && this.connection.state === 'Connected')
            this.connection.invoke('JoinTicketGroup', String(ticketId));
    },
    leaveTicket: function (ticketId) {
        if (this.connection && this.connection.state === 'Connected')
            this.connection.invoke('LeaveTicketGroup', String(ticketId));
    }
};

function updateNavbar() {
    var user = Auth.getUser();
    var loggedIn = Auth.isLoggedIn();
    var el;

    el = document.getElementById('nav-login');
    if (el) el.style.display = loggedIn ? 'none' : '';

    el = document.getElementById('nav-logout');
    if (el) el.style.display = loggedIn ? '' : 'none';

    el = document.getElementById('nav-username');
    if (el) {
        el.style.display = (loggedIn && user) ? '' : 'none';
        if (loggedIn && user) el.textContent = user.username;
    }

    el = document.getElementById('nav-admin');
    if (el) el.style.display = (loggedIn && Auth.hasAnyRole('Administrator')) ? '' : 'none';

    el = document.getElementById('nav-dashboard');
    if (el) el.style.display = (loggedIn && Auth.hasAnyRole('Administrator', 'Manager')) ? '' : 'none';
}

function requireAuth(roles) {
    if (!Auth.isLoggedIn()) { window.location.href = '/AuthPage/Login'; return false; }
    if (roles && roles.length > 0 && !Auth.hasAnyRole.apply(Auth, roles)) {
        window.location.href = '/'; return false;
    }
    return true;
}

var Helpers = {
    statusBadge: function (status) {
        var map = { Open: 'badge-open', InProgress: 'badge-inprogress', Resolved: 'badge-resolved', Closed: 'badge-closed' };
        return '<span class="tm-badge ' + (map[status] || '') + '">' + t('status.' + status) + '</span>';
    },
    priorityBadge: function (priority) {
        var map = { Low: 'priority-low', Medium: 'priority-medium', High: 'priority-high', Critical: 'priority-critical' };
        return '<span class="tm-badge ' + (map[priority] || '') + '">' + t('priority.' + priority) + '</span>';
    },
    formatDate: function (d) {
        if (!d) return '—';
        return new Date(d).toLocaleDateString(I18n.getLang() === 'ru' ? 'ru-RU' : 'en-US',
            { year: 'numeric', month: 'short', day: 'numeric' });
    },
    formatDateTime: function (d) {
        if (!d) return '—';
        return new Date(d).toLocaleString(I18n.getLang() === 'ru' ? 'ru-RU' : 'en-US',
            { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
    },
    timeAgo: function (d) {
        if (!d) return '';
        var diff = Date.now() - new Date(d).getTime();
        var m = Math.floor(diff / 60000);
        var ru = I18n.getLang() === 'ru';
        if (m < 1) return ru ? 'только что' : 'just now';
        if (m < 60) return ru ? m + ' мин. назад' : m + 'm ago';
        var h = Math.floor(m / 60);
        if (h < 24) return ru ? h + ' ч. назад' : h + 'h ago';
        return ru ? Math.floor(h / 24) + ' дн. назад' : Math.floor(h / 24) + 'd ago';
    },
    escapeHtml: function (str) {
        var d = document.createElement('div');
        d.appendChild(document.createTextNode(str == null ? '' : String(str)));
        return d.innerHTML;
    }
};

document.addEventListener('DOMContentLoaded', function () {
    I18n.initSwitcher();
    I18n.applyToPage();
    updateNavbar();

    var logoutBtn = document.getElementById('nav-logout');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', function (e) {
            e.preventDefault();
            Auth.logout();
        });
    }

    if (Auth.isLoggedIn()) {
        Auth.scheduleExpiryCheck();
        TM_SignalR.connect();
    }

    document.addEventListener('tm:langChanged', function () {
        updateNavbar();
    });
});