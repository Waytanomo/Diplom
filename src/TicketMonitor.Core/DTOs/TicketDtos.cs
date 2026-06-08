using TicketMonitor.Core.Enums;

namespace TicketMonitor.Core.DTOs
{
    public record CreateTicketDto(string Title, string Description, TicketPriority Priority);
    public record ChangeStatusDto(TicketStatus NewStatus);
    public record AssignTicketDto(string? AssigneeId);
    public record AddCommentDto(string Text);

    public record TicketDto(
        int Id, string Title, string Description,
        TicketStatus Status, TicketPriority Priority,
        DateTime CreatedAt, DateTime? ClosedAt,
        string? AssignedToId, string? AssignedToName,
        string CreatedByName, string CreatedById);

    public record CommentDto(int Id, string Text, string AuthorName, string AuthorId, DateTime CreatedAt);

    // Баг 2: Roles возвращены обратно — без них фронтенд не знает роль пользователя
    public record UserDto(string Id, string UserName, IList<string> Roles);

    public record StatusLogDto(
        int Id,
        string OldStatus,
        string NewStatus,
        string ChangedByName,
        DateTime Timestamp);

    public record TicketStatsDto(
        int Total,
        Dictionary<string, int> ByStatus,
        Dictionary<string, int> ByPriority,
        Dictionary<string, int> ByAssignee,
        double AvgResolutionHours);

    // Email удалён из CreateUserRequest — генерируется автоматически
    public record CreateUserRequest(string UserName, string Password, string Role);

    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
        public bool IsFiltered { get; set; }
    }
}