using System;
using System.Collections.Generic;
using System.Text;
using TicketMonitor.Core.Enums;

namespace TicketMonitor.Core.Entities
{
    public class StatusLog
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public TicketStatus OldStatus { get; set; }
        public TicketStatus NewStatus { get; set; }
        public string ChangedById { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public Ticket Ticket { get; set; } = null!;
        public ApplicationUser ChangedBy { get; set; } = null!;
    }
}
