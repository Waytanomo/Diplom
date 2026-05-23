using TicketMonitor.Core.Enums;

namespace TicketMonitor.Core.DTOs
{
    public record CreateTicketDto(string Title, string Description, TicketPriority Priority);
    public record ChangeStatusDto(TicketStatus NewStatus);
    public record AssignTicketDto(string AssigneeId);
    public record AddCommentDto(string Text);

    public record TicketDto(int Id, string Title, string Description, TicketStatus Status, TicketPriority Priority,
        DateTime CreatedAt, DateTime? ClosedAt, string? AssignedToName, string CreatedByName);

    public record CommentDto(int Id, string Text, string AuthorName, DateTime CreatedAt);

    public record TicketStatsDto(int Total, Dictionary<string, int> ByStatus, Dictionary<string, int> ByPriority,
        Dictionary<string, int> ByAssignee, double AvgResolutionHours);
}