using TicketMonitor.Core.DTOs;

namespace TicketMonitor.Core.Interfaces
{
    public interface ITicketService
    {
        /// <summary>
        /// Возвращает тикеты с учётом роли вызывающего.
        /// Клиенты видят все тикеты (они создатели).
        /// Менеджеры/Администраторы видят все тикеты.
        /// Работники (Client-роль без менеджера) — только назначенные им.
        /// Логика фильтрации реализована через callerRoles + callerUserId.
        /// </summary>
        Task<PagedResult<TicketDto>> GetAllAsync(
            string callerUserId,
            IEnumerable<string> callerRoles,
            int page = 1,
            int pageSize = 10,
            string? status = null,
            string? priority = null,
            string? search = null);

        Task<TicketDto?> GetByIdAsync(int id);
        Task<IEnumerable<CommentDto>> GetCommentsAsync(int id);
        Task<TicketDto> CreateAsync(CreateTicketDto dto, string userId);
        Task<bool> ChangeStatusAsync(int id, ChangeStatusDto dto, string userId);
        Task<bool> AssignAsync(int id, AssignTicketDto dto, string userId);
        Task<CommentDto> AddCommentAsync(int id, AddCommentDto dto, string userId);
        Task<TicketStatsDto> GetStatsAsync();
        Task<bool> DeleteAsync(int id, string userId);
        Task<IEnumerable<UserDto>> GetUsersAsync();
    }
}