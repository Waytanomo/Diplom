using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TicketMonitor.Core.Entities;

namespace TicketMonitor.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<StatusLog> StatusLogs => Set<StatusLog>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Ticket>(e =>
            {
                e.Property(t => t.RowVersion).IsRowVersion();
                e.HasOne(t => t.CreatedBy).WithMany().HasForeignKey(t => t.CreatedById).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(t => t.AssignedTo).WithMany().HasForeignKey(t => t.AssignedToId).OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Comment>(e =>
            {
                e.HasOne(c => c.Ticket).WithMany(t => t.Comments).HasForeignKey(c => c.TicketId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(c => c.Author).WithMany().HasForeignKey(c => c.AuthorId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<StatusLog>(e =>
            {
                e.HasOne(s => s.Ticket).WithMany(t => t.StatusLogs).HasForeignKey(s => s.TicketId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(s => s.ChangedBy).WithMany().HasForeignKey(s => s.ChangedById).OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}