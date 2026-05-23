using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using TicketMonitor.Core.Enums;

namespace TicketMonitor.Core.Entities
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TicketStatus Status { get; set; } = TicketStatus.Open;
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }
        public string CreatedById { get; set; } = string.Empty;
        public string? AssignedToId { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public ApplicationUser CreatedBy { get; set; } = null!;
        public ApplicationUser? AssignedTo { get; set; }
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<StatusLog> StatusLogs { get; set; } = new List<StatusLog>();
    }
}
