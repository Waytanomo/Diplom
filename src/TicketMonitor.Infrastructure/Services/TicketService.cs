using Microsoft.EntityFrameworkCore;
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

        public TicketService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TicketDto>> GetAllAsync(
            int page = 1,
            int pageSize = 10)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var tickets = await _context.Tickets
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return tickets.Select(MapToDto);
        }

        public async Task<TicketDto?> GetByIdAsync(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(t => t.Id == id);

            return ticket == null ? null : MapToDto(ticket);
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

            return MapToDto(ticket);
        }

        public async Task<bool> ChangeStatusAsync(int id, ChangeStatusDto dto, string userId)
        {
            var ticket = await _context.Tickets.FindAsync(id);

            if (ticket == null)
                return false;

            var oldStatus = ticket.Status;

            ticket.Status = dto.NewStatus;

            ticket.ClosedAt =
                dto.NewStatus == TicketStatus.Closed ||
                dto.NewStatus == TicketStatus.Resolved
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

            return true;
        }

        public async Task<bool> AssignAsync(int id, AssignTicketDto dto, string userId)
        {
            var ticket = await _context.Tickets.FindAsync(id);

            if (ticket == null)
                return false;

            bool userExists = await _context.Users
                .AnyAsync<ApplicationUser>(u => u.Id == dto.AssigneeId);

            if (!userExists)
                return false;

            ticket.AssignedToId = dto.AssigneeId;

            await _context.SaveChangesAsync();

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

            return new CommentDto(
                comment.Id,
                comment.Text,
                comment.Author.UserName ?? "Unknown",
                comment.CreatedAt);
        }

        public async Task<TicketStatsDto> GetStatsAsync()
        {
            var total = await _context.Tickets.CountAsync();

            var byStatus = await _context.Tickets
                .GroupBy(t => t.Status)
                .Select(g => new
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            var byPriority = await _context.Tickets
                .GroupBy(t => t.Priority)
                .Select(g => new
                {
                    Priority = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.Priority, x => x.Count);

            var byAssignee = await _context.Tickets
                .Where(t => t.AssignedTo != null)
                .GroupBy(t => t.AssignedTo!.UserName)
                .Select(g => new
                {
                    Assignee = g.Key!,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.Assignee, x => x.Count);

            var closed = await _context.Tickets
                .Where(t => t.ClosedAt.HasValue)
                .ToListAsync();

            var avgHours = closed.Any()
                ? closed.Average(t =>
                    (t.ClosedAt!.Value - t.CreatedAt).TotalHours)
                : 0.0;

            return new TicketStatsDto(
                total,
                byStatus,
                byPriority,
                byAssignee,
                Math.Round(avgHours, 2));
        }

        // ✅ SoftDelete
        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
                return false;

            ticket.IsDeleted = true;
            ticket.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        private static TicketDto MapToDto(Ticket t) => new(
            t.Id,
            t.Title,
            t.Description,
            t.Status,
            t.Priority,
            t.CreatedAt,
            t.ClosedAt,
            t.AssignedTo?.UserName,
            t.CreatedBy.UserName ?? "Unknown");
    }
}