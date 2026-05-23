using System;
using System.Collections.Generic;
using System.Text;

namespace TicketMonitor.Core.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string AuthorId { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Ticket Ticket { get; set; } = null!;
        public ApplicationUser Author { get; set; } = null!;
    }
}
