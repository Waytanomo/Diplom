// ============================================================
// TicketMonitor — i18n.js  (RU / EN)
// Подключать ДО site.js
// Экспортирует глобально: window.I18n, window.t()
// ВАЖНО: использует IIFE + window.t = function(...) {}
// чтобы избежать конфликта с const/let t в других файлах
// ============================================================

(function (global) {
    'use strict';

    var STORAGE_KEY = 'tm_lang';

    var translations = {
        en: {
            'nav.tickets': 'Tickets', 'nav.dashboard': 'Dashboard', 'nav.admin': 'Admin',
            'nav.login': 'Login', 'nav.logout': 'Logout',
            'tickets.title': 'Tickets', 'tickets.new': '+ New Ticket',
            'tickets.search': 'Search tickets…', 'tickets.all_statuses': 'All Statuses',
            'tickets.all_priorities': 'All Priorities', 'tickets.per_page': '/ page',
            'tickets.unassigned': 'Unassigned', 'tickets.empty': 'No tickets found',
            'tickets.loading': 'Loading…', 'tickets.count_one': 'ticket',
            'tickets.count_many': 'tickets',
            'tickets.my_notice': '📋 Showing tickets assigned to you or created by you',
            'col.id': '#', 'col.title': 'Title', 'col.status': 'Status',
            'col.priority': 'Priority', 'col.assigned': 'Assigned To', 'col.created': 'Created',
            'status.Open': 'Open', 'status.InProgress': 'In Progress',
            'status.Resolved': 'Resolved', 'status.Closed': 'Closed',
            'priority.Low': 'Low', 'priority.Medium': 'Medium',
            'priority.High': 'High', 'priority.Critical': 'Critical',
            'ticket.back': '← Back', 'ticket.description': 'Description',
            'ticket.details': 'Details', 'ticket.created_by': 'Created by',
            'ticket.assigned_to': 'Assigned to', 'ticket.created_at': 'Created',
            'ticket.closed_at': 'Closed', 'ticket.change_status': 'Change Status',
            'ticket.update_status': 'Update Status', 'ticket.assign': 'Assign Ticket',
            'ticket.assign_btn': 'Assign', 'ticket.history': 'Status History',
            'ticket.no_history': 'No history yet', 'ticket.delete': '🗑 Delete',
            'ticket.delete_confirm': 'Delete Ticket?',
            'ticket.delete_warning': 'This action cannot be undone. The ticket will be soft-deleted.',
            'ticket.cancel': 'Cancel',
            'comments.title': 'Comments', 'comments.empty': 'No comments yet',
            'comments.placeholder': 'Write a comment…', 'comments.post': 'Post Comment',
            'comments.posting': 'Posting…', 'comments.hint': 'Ctrl+Enter to submit',
            'create.title': 'New Ticket', 'create.back': '← Back to Tickets',
            'create.field_title': 'Title', 'create.field_desc': 'Description',
            'create.field_prio': 'Priority', 'create.submit': 'Create Ticket',
            'create.cancel': 'Cancel', 'create.creating': 'Creating…',
            'create.placeholder_title': 'Brief description of the issue',
            'create.placeholder_desc': 'Provide full details about the issue…',
            'create.err_title': 'Title is required', 'create.err_desc': 'Description is required',
            'login.title': 'Welcome back',
            'login.subtitle': 'Sign in to your TicketMonitor account',
            'login.username': 'Username', 'login.password': 'Password',
            'login.submit': 'Sign In', 'login.signing': 'Signing in…',
            'login.error_empty': 'Please enter username and password',
            'dashboard.title': 'Dashboard',
            'dashboard.subtitle': 'Real-time ticket analytics',
            'dashboard.refresh': '↻ Refresh', 'dashboard.total': 'Total Tickets',
            'dashboard.open': 'Open', 'dashboard.inprogress': 'In Progress',
            'dashboard.resolved': 'Resolved', 'dashboard.avg': 'Avg Resolution',
            'dashboard.hours': 'hours', 'dashboard.by_status': 'By Status',
            'dashboard.by_priority': 'By Priority', 'dashboard.by_assignee': 'By Assignee',
            'dashboard.no_data': 'No assigned tickets',
            'home.title': 'TicketMonitor',
            'home.subtitle': 'Streamlined ticket management for your team.',
            'home.view_tickets': 'View Tickets', 'home.sign_in': 'Sign In',
            'home.open': 'Open Tickets', 'home.inprogress': 'In Progress',
            'admin.title': 'User Management', 'admin.subtitle': 'Manage users and roles',
            'admin.add_user': '+ Add User', 'admin.col_username': 'Username',
            'admin.col_email': 'Email', 'admin.col_role': 'Role',
            'admin.col_actions': 'Actions', 'admin.you': 'you', 'admin.delete': 'Delete',
            'admin.delete_confirm': 'Delete user "{name}"? This cannot be undone.',
            'admin.no_users': 'No users found',
            'user.create_title': 'Create User', 'user.field_username': 'Username',
            'user.field_password': 'Password', 'user.field_role': 'Role',
            'user.field_email': 'Email (optional)',
            'user.email_hint': 'If empty: username@local.tier',
            'user.submit': 'Create User', 'user.creating': 'Creating…', 'user.cancel': 'Cancel',
            'user.err_username': 'Username is required',
            'user.err_password': 'Password must be at least 6 characters',
            'notify.ticket_created': 'New ticket: "{title}"',
            'notify.ticket_deleted': 'Ticket #{id} was deleted',
            'notify.status_changed': 'Ticket #{id}: {old} → {new}',
            'notify.session_expiring': 'Your session expires in 2 minutes. Please save your work.',
            'notify.session_expired': 'Session expired. Please log in again.',
            'common.loading': 'Loading…', 'common.error': 'Something went wrong',
            'common.of': 'of', 'common.unassigned': 'Unassigned', 'common.auto_email': '(auto)',
        },
        ru: {
            'nav.tickets': 'Тикеты', 'nav.dashboard': 'Дашборд',
            'nav.admin': 'Администрирование', 'nav.login': 'Войти', 'nav.logout': 'Выйти',
            'tickets.title': 'Тикеты', 'tickets.new': '+ Новый тикет',
            'tickets.search': 'Поиск тикетов…', 'tickets.all_statuses': 'Все статусы',
            'tickets.all_priorities': 'Все приоритеты', 'tickets.per_page': '/ стр.',
            'tickets.unassigned': 'Не назначен', 'tickets.empty': 'Тикеты не найдены',
            'tickets.loading': 'Загрузка…', 'tickets.count_one': 'тикет',
            'tickets.count_many': 'тикетов',
            'tickets.my_notice': '📋 Показаны тикеты, назначенные вам или созданные вами',
            'col.id': '№', 'col.title': 'Название', 'col.status': 'Статус',
            'col.priority': 'Приоритет', 'col.assigned': 'Исполнитель', 'col.created': 'Создан',
            'status.Open': 'Открыт', 'status.InProgress': 'В работе',
            'status.Resolved': 'Решён', 'status.Closed': 'Закрыт',
            'priority.Low': 'Низкий', 'priority.Medium': 'Средний',
            'priority.High': 'Высокий', 'priority.Critical': 'Критический',
            'ticket.back': '← Назад', 'ticket.description': 'Описание',
            'ticket.details': 'Детали', 'ticket.created_by': 'Создал',
            'ticket.assigned_to': 'Исполнитель', 'ticket.created_at': 'Создан',
            'ticket.closed_at': 'Закрыт', 'ticket.change_status': 'Изменить статус',
            'ticket.update_status': 'Обновить статус', 'ticket.assign': 'Назначить тикет',
            'ticket.assign_btn': 'Назначить', 'ticket.history': 'История статусов',
            'ticket.no_history': 'История пуста', 'ticket.delete': '🗑 Удалить',
            'ticket.delete_confirm': 'Удалить тикет?',
            'ticket.delete_warning': 'Это действие нельзя отменить. Тикет будет помечен как удалённый.',
            'ticket.cancel': 'Отмена',
            'comments.title': 'Комментарии', 'comments.empty': 'Комментариев пока нет',
            'comments.placeholder': 'Написать комментарий…', 'comments.post': 'Отправить',
            'comments.posting': 'Отправка…', 'comments.hint': 'Ctrl+Enter для отправки',
            'create.title': 'Новый тикет', 'create.back': '← Вернуться к тикетам',
            'create.field_title': 'Название', 'create.field_desc': 'Описание',
            'create.field_prio': 'Приоритет', 'create.submit': 'Создать тикет',
            'create.cancel': 'Отмена', 'create.creating': 'Создание…',
            'create.placeholder_title': 'Краткое описание проблемы',
            'create.placeholder_desc': 'Подробно опишите суть проблемы…',
            'create.err_title': 'Укажите название', 'create.err_desc': 'Укажите описание',
            'login.title': 'С возвращением',
            'login.subtitle': 'Войдите в аккаунт TicketMonitor',
            'login.username': 'Имя пользователя', 'login.password': 'Пароль',
            'login.submit': 'Войти', 'login.signing': 'Вход…',
            'login.error_empty': 'Введите имя пользователя и пароль',
            'dashboard.title': 'Дашборд',
            'dashboard.subtitle': 'Аналитика тикетов в реальном времени',
            'dashboard.refresh': '↻ Обновить', 'dashboard.total': 'Всего тикетов',
            'dashboard.open': 'Открытые', 'dashboard.inprogress': 'В работе',
            'dashboard.resolved': 'Решённые', 'dashboard.avg': 'Среднее время',
            'dashboard.hours': 'часов', 'dashboard.by_status': 'По статусу',
            'dashboard.by_priority': 'По приоритету', 'dashboard.by_assignee': 'По исполнителю',
            'dashboard.no_data': 'Нет назначенных тикетов',
            'home.title': 'TicketMonitor',
            'home.subtitle': 'Удобное управление тикетами для вашей команды.',
            'home.view_tickets': 'Перейти к тикетам', 'home.sign_in': 'Войти',
            'home.open': 'Открытые тикеты', 'home.inprogress': 'В работе',
            'admin.title': 'Управление пользователями',
            'admin.subtitle': 'Управление аккаунтами и ролями',
            'admin.add_user': '+ Добавить пользователя', 'admin.col_username': 'Пользователь',
            'admin.col_email': 'Email', 'admin.col_role': 'Роль',
            'admin.col_actions': 'Действия', 'admin.you': 'вы', 'admin.delete': 'Удалить',
            'admin.delete_confirm': 'Удалить пользователя "{name}"? Это нельзя отменить.',
            'admin.no_users': 'Пользователи не найдены',
            'user.create_title': 'Создать пользователя',
            'user.field_username': 'Имя пользователя', 'user.field_password': 'Пароль',
            'user.field_role': 'Роль', 'user.field_email': 'Email (необязательно)',
            'user.email_hint': 'Если не указан: username@local.tier',
            'user.submit': 'Создать пользователя', 'user.creating': 'Создание…',
            'user.cancel': 'Отмена',
            'user.err_username': 'Имя пользователя обязательно',
            'user.err_password': 'Пароль должен содержать минимум 6 символов',
            'notify.ticket_created': 'Новый тикет: "{title}"',
            'notify.ticket_deleted': 'Тикет #{id} удалён',
            'notify.status_changed': 'Тикет #{id}: {old} → {new}',
            'notify.session_expiring': 'Сессия истекает через 2 минуты. Сохраните работу.',
            'notify.session_expired': 'Сессия истекла. Пожалуйста, войдите снова.',
            'common.loading': 'Загрузка…', 'common.error': 'Что-то пошло не так',
            'common.of': 'из', 'common.unassigned': 'Не назначен', 'common.auto_email': '(авто)',
        }
    };

    function detectLang() {
        var saved = localStorage.getItem(STORAGE_KEY);
        if (saved && translations[saved]) return saved;
        var bl = ((navigator.language || '') + '').toLowerCase().slice(0, 2);
        return translations[bl] ? bl : 'ru';
    }

    var currentLang = detectLang();

    function translate(key, vars) {
        var dict = translations[currentLang] || translations['ru'];
        var str = dict[key];
        if (str === undefined) str = (translations['en'] || {})[key];
        if (str === undefined) str = key;
        if (!vars) return str;
        return str.replace(/\{(\w+)\}/g, function (_, k) {
            return vars[k] !== undefined ? String(vars[k]) : '{' + k + '}';
        });
    }

    function setLang(lang) {
        if (!translations[lang]) return;
        currentLang = lang;
        localStorage.setItem(STORAGE_KEY, lang);
        applyToPage();
        updateLangSwitcher();
        document.dispatchEvent(new CustomEvent('tm:langChanged', { detail: { lang: lang } }));
    }

    function applyToPage() {
        document.querySelectorAll('[data-i18n]').forEach(function (el) {
            el.textContent = translate(el.dataset.i18n);
        });
        document.querySelectorAll('[data-i18n-placeholder]').forEach(function (el) {
            el.placeholder = translate(el.dataset.i18nPlaceholder);
        });
    }

    function updateLangSwitcher() {
        document.querySelectorAll('.lang-btn').forEach(function (btn) {
            btn.classList.toggle('active', btn.dataset.lang === currentLang);
        });
    }

    function initSwitcher() {
        var sw = document.getElementById('lang-switcher');
        if (!sw) return;
        sw.innerHTML =
            '<button class="lang-btn' + (currentLang === 'ru' ? ' active' : '') +
            '" data-lang="ru" title="Русский">RU</button>' +
            '<span style="color:var(--border-light);pointer-events:none;padding:0 2px">|</span>' +
            '<button class="lang-btn' + (currentLang === 'en' ? ' active' : '') +
            '" data-lang="en" title="English">EN</button>';
        sw.addEventListener('click', function (e) {
            var btn = e.target.closest && e.target.closest('.lang-btn');
            if (btn) setLang(btn.dataset.lang);
        });
    }

    // Публичный объект
    var I18n = {
        t: translate,
        getLang: function () { return currentLang; },
        setLang: setLang,
        applyToPage: applyToPage,
        initSwitcher: initSwitcher
    };

    global.I18n = I18n;

    // Глобальная функция t() — обычное объявление function, не const/let,
    // поэтому не конфликтует ни с чем
    global.t = function t(key, vars) {
        return I18n.t(key, vars);
    };

}(window));