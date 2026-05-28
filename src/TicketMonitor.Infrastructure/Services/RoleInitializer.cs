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
            var serviceProvider = scope.ServiceProvider;

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Роль Executor УДАЛЕНА. Текущие роли: Administrator, Manager, Client
            var validRoles = new[] { "Administrator", "Manager", "Client" };

            foreach (var role in validRoles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Удаляем роль Executor из БД если она ещё есть
            var executorRole = await roleManager.FindByNameAsync("Executor");
            if (executorRole != null)
            {
                // Переназначаем всех Executor → Client
                var usersInExecutor = await userManager.GetUsersInRoleAsync("Executor");
                foreach (var u in usersInExecutor)
                {
                    await userManager.RemoveFromRoleAsync(u, "Executor");
                    if (!(await userManager.GetRolesAsync(u)).Any())
                        await userManager.AddToRoleAsync(u, "Client");
                }
                await roleManager.DeleteAsync(executorRole);
            }

            // Создаём дефолтного админа
            if (!await userManager.Users.AnyAsync(u => u.UserName == "admin", cancellationToken))
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@local.tier",
                    EmailConfirmed = true
                };
                var res = await userManager.CreateAsync(admin, "Admin123!");
                if (res.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Administrator");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}