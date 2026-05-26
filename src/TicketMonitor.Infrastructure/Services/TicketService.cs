using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TicketMonitor.Api.Hubs;
using TicketMonitor.Core.DTOs;
using TicketMonitor.Core.Entities;
using TicketMonitor.Core.Enums;
using TicketMonitor.Core.Interfaces;
using TicketMonitor.Infrastructure.Data;

namespace TicketMonitor.Infrastructure.Services
{
    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<TicketHub> _hubContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketService(
            ApplicationDbContext context,
            IHubContext<TicketHub> hubContext,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _hubContext = hubContext;
            _userManager = userManager;
        }

        public async Task<PagedResult<TicketDto>> GetAllAsync(
            int page = 1,
            int pageSize = 10,
            string? status = null,
            string? priority = null,
            string? search = null)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.Tickets
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TicketStatus>(status, true, out var statusEnum))
                query = query.Where(t => t.Status == statusEnum);

            if (!string.IsNullOrWhiteSpace(priority) && Enum.TryParse<TicketPriority>(priority, true, out var priorityEnum))
                query = query.Where(t => t.Priority == priorityEnum);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(t => t.Title.Contains(search) || t.Description.Contains(search));

            var total = await query.CountAsync();

            var tickets = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<TicketDto>
            {
                Items = tickets.Select(MapToDto),
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<TicketDto?> GetByIdAsync(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(t => t.Id == id);

            return ticket == null ? null : MapToDto(ticket);
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsAsync(int id)
        {
            return await _context.Comments
                .Include(c => c.Author)
                .Where(c => c.TicketId == id)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentDto(
                    c.Id,
                    c.Text,
                    c.Author.UserName ?? "Unknown",
                    c.AuthorId,
                    c.CreatedAt))
                .ToListAsync();
        }

        public async Task<TicketDto> CreateAsync(CreateTicketDto dto, string userId)
        {
            var ticket = new Ticket
            {
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                CreatedById = userId
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            var createdTicket = await _context.Tickets
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .FirstAsync(t => t.Id == ticket.Id);

            var result = MapToDto(createdTicket);

            await _hubContext.Clients.All.SendAsync("TicketCreated", result);

            return result;
        }

        public async Task<bool> ChangeStatusAsync(int id, ChangeStatusDto dto, string userId)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return false;

            var oldStatus = ticket.Status;
            ticket.Status = dto.NewStatus;
            ticket.ClosedAt =
                dto.NewStatus == TicketStatus.Closed || dto.NewStatus == TicketStatus.Resolved
                    ? DateTime.UtcNow
                    : null;

            _context.StatusLogs.Add(new StatusLog
            {
                TicketId = id,
                OldStatus = oldStatus,
                NewStatus = dto.NewStatus,
                ChangedById = userId
            });

            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"ticket-{id}").SendAsync("StatusChanged", new
            {
                ticketId = id,
                oldStatus = oldStatus.ToString(),
                newStatus = dto.NewStatus.ToString()
            });

            await _hubContext.Clients.All.SendAsync("TicketUpdated", new { ticketId = id });

            return true;
        }

        public async Task<bool> AssignAsync(int id, AssignTicketDto dto, string userId)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return false;

            bool userExists = await _context.Users.AnyAsync<ApplicationUser>(u => u.Id == dto.AssigneeId);
            if (!userExists) return false;

            ticket.AssignedToId = dto.AssigneeId;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"ticket-{id}").SendAsync("TicketAssigned", new { ticketId = id, assigneeId = dto.AssigneeId });
            await _hubContext.Clients.All.SendAsync("TicketUpdated", new { ticketId = id });

            return true;
        }

        public async Task<CommentDto> AddCommentAsync(int id, AddCommentDto dto, string userId)
        {
            if (!await _context.Tickets.AnyAsync<Ticket>(t => t.Id == id))
                throw new KeyNotFoundException("Ticket not found");

            var comment = new Comment
            {
                TicketId = id,
                Text = dto.Text,
                AuthorId = userId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var savedComment = await _context.Comments
                .Include(c => c.Author)
                .FirstAsync(c => c.Id == comment.Id);

            var result = new CommentDto(
                savedComment.Id,
                savedComment.Text,
                savedComment.Author.UserName ?? "Unknown",
                savedComment.AuthorId,
                savedComment.CreatedAt);

            await _hubContext.Clients.Group($"ticket-{id}").SendAsync("CommentAdded", result);

            return result;
        }

        public async Task<TicketStatsDto> GetStatsAsync()
        {
            var total = await _context.Tickets.CountAsync();

            var byStatus = await _context.Tickets
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            var byPriority = await _context.Tickets
                .GroupBy(t => t.Priority)
                .Select(g => new { Priority = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(x => x.Priority, x => x.Count);

            var byAssignee = await _context.Tickets
                .Where(t => t.AssignedTo != null)
                .GroupBy(t => t.AssignedTo!.UserName)
                .Select(g => new { Assignee = g.Key!, Count = g.Count() })
                .ToDictionaryAsync(x => x.Assignee, x => x.Count);

            var closed = await _context.Tickets
                .Where(t => t.ClosedAt.HasValue)
                .ToListAsync();

            var avgHours = closed.Any()
                ? closed.Average(t => (t.ClosedAt!.Value - t.CreatedAt).TotalHours)
                : 0.0;

            return new TicketStatsDto(total, byStatus, byPriority, byAssignee, Math.Round(avgHours, 2));
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return false;

            ticket.IsDeleted = true;
            ticket.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("TicketDeleted", new { ticketId = id });

            return true;
        }

        public async Task<IEnumerable<UserDto>> GetUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var result = new List<UserDto>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                result.Add(new UserDto(u.Id, u.UserName ?? "", u.Email ?? "", roles));
            }
            return result;
        }

        private static TicketDto MapToDto(Ticket t) => new(
            t.Id,
            t.Title,
            t.Description,
            t.Status,
            t.Priority,
            t.CreatedAt,
            t.ClosedAt,
            t.AssignedToId,
            t.AssignedTo?.UserName,
            t.CreatedBy?.UserName ?? "Unknown",
            t.CreatedById);
    }
}