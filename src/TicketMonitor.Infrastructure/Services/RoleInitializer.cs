using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TicketMonitor.Core.Entities;

namespace TicketMonitor.Infrastructure.Services
{
    public class RoleInitializer : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public RoleInitializer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Создаём изолированный Scope для доступа к Scoped-сервисам из Singleton
            using var scope = _scopeFactory.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Создаём роли
            foreach (var role in new[] { "Administrator", "Manager", "Executor", "Client" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Создаём админа, если его нет
            if (!await userManager.Users.AnyAsync(u => u.UserName == "admin"))
            {
                var admin = new ApplicationUser { UserName = "admin", Email = "admin@system.local", EmailConfirmed = true };
                var res = await userManager.CreateAsync(admin, "Admin123!");
                if (res.Succeeded) await userManager.AddToRoleAsync(admin, "Administrator");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}