using TicketMonitor.Core.DTOs;
using TicketMonitor.Core.Enums;

namespace TicketMonitor.Core.Interfaces;

public interface ITicketNotificationService
{
    Task TicketCreated(TicketDto ticket);

    Task TicketUpdated(int ticketId);

    Task TicketDeleted(int ticketId);

    Task StatusChanged(
        int ticketId,
        TicketStatus oldStatus,
        TicketStatus newStatus);

    Task CommentAdded(
        int ticketId,
        CommentDto comment);

    Task TicketAssigned(
        int ticketId,
        string assigneeId);
}