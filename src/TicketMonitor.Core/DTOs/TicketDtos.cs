using TicketMonitor.Core.Enums;

namespace TicketMonitor.Core.DTOs
{
    public record CreateTicketDto(string Title, string Description, TicketPriority Priority);
    public record ChangeStatusDto(TicketStatus NewStatus);
    public record AssignTicketDto(string AssigneeId);
    public record AddCommentDto(string Text);

    public record TicketDto(int Id, string Title, string Description, TicketStatus Status, TicketPriority Priority,
        DateTime CreatedAt, DateTime? ClosedAt, string? AssignedToId, string? AssignedToName, string CreatedByName, string CreatedById);

    public record CommentDto(int Id, string Text, string AuthorName, string AuthorId, DateTime CreatedAt);

    public record TicketStatsDto(int Total, Dictionary<string, int> ByStatus, Dictionary<string, int> ByPriority,
        Dictionary<string, int> ByAssignee, double AvgResolutionHours);

    public record UserDto(string Id, string UserName, string Email, IList<string> Roles);

    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
    }
}