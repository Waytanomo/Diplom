using Microsoft.AspNetCore.SignalR;
using TicketMonitor.Api.Hubs;
using TicketMonitor.Core.DTOs;
using TicketMonitor.Core.Enums;
using TicketMonitor.Core.Interfaces;

namespace TicketMonitor.Api.Services;

public class SignalRTicketNotificationService
    : ITicketNotificationService
{
    private readonly IHubContext<TicketHub> _hubContext;

    public SignalRTicketNotificationService(
        IHubContext<TicketHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task TicketCreated(TicketDto ticket)
    {
        await _hubContext.Clients.All
            .SendAsync("TicketCreated", ticket);
    }

    public async Task TicketUpdated(int ticketId)
    {
        await _hubContext.Clients.All
            .SendAsync("TicketUpdated", new
            {
                ticketId
            });
    }

    public async Task TicketDeleted(int ticketId)
    {
        await _hubContext.Clients.All
            .SendAsync("TicketDeleted", new
            {
                ticketId
            });
    }

    public async Task StatusChanged(
        int ticketId,
        TicketStatus oldStatus,
        TicketStatus newStatus)
    {
        await _hubContext.Clients
            .Group($"ticket-{ticketId}")
            .SendAsync("StatusChanged", new
            {
                ticketId,
                oldStatus = oldStatus.ToString(),
                newStatus = newStatus.ToString()
            });
    }

    public async Task CommentAdded(
        int ticketId,
        CommentDto comment)
    {
        await _hubContext.Clients
            .Group($"ticket-{ticketId}")
            .SendAsync("CommentAdded", comment);
    }

    public async Task TicketAssigned(
        int ticketId,
        string assigneeId)
    {
        await _hubContext.Clients
            .Group($"ticket-{ticketId}")
            .SendAsync("TicketAssigned", new
            {
                ticketId,
                assigneeId
            });
    }
}