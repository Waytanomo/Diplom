using TicketMonitor.Core.DTOs;

namespace TicketMonitor.Core.Interfaces
{
    public interface ITicketService
    {
        Task<IEnumerable<TicketDto>> GetAllAsync(
            int page = 1,
            int pageSize = 10);
        Task<TicketDto?> GetByIdAsync(int id);
        Task<TicketDto> CreateAsync(CreateTicketDto dto, string userId);
        Task<bool> ChangeStatusAsync(int id, ChangeStatusDto dto, string userId);
        Task<bool> AssignAsync(int id, AssignTicketDto dto, string userId);
        Task<CommentDto> AddCommentAsync(int id, AddCommentDto dto, string userId);
        Task<TicketStatsDto> GetStatsAsync();
        Task<bool> DeleteAsync(int id, string userId);
    }
}