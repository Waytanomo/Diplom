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
            using var scope = _scopeFactory.CreateScope();
            var sp = scope.ServiceProvider;
            var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

            // Актуальные роли: Administrator, Manager, Executor
            // Роль Client УДАЛЕНА
            var validRoles = new[] { "Administrator", "Manager", "Executor" };

            foreach (var role in validRoles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Миграция: переводим всех Client → Executor
            var clientRole = await roleManager.FindByNameAsync("Client");
            if (clientRole != null)
            {
                var clientUsers = await userManager.GetUsersInRoleAsync("Client");
                foreach (var u in clientUsers)
                {
                    await userManager.RemoveFromRoleAsync(u, "Client");
                    var currentRoles = await userManager.GetRolesAsync(u);
                    if (!currentRoles.Any())
                        await userManager.AddToRoleAsync(u, "Executor");
                }
                await roleManager.DeleteAsync(clientRole);
            }

            // Создание роли администратора по умолчанию, если он отсутствует в базе данных
            if (!await userManager.Users.AnyAsync(u => u.UserName == "admin", cancellationToken))
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@local.tier",
                    EmailConfirmed = true
                };
                var res = await userManager.CreateAsync(admin, "xKGl1MvrWwdX3MVp9CdP");
                if (res.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Administrator");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}