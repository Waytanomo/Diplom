using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TicketMonitor.Core.Entities;
using TicketMonitor.Core.Interfaces;
using TicketMonitor.Infrastructure.Data;
using TicketMonitor.Infrastructure.Services;

namespace TicketMonitor.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ApplicationDbContext>(opt =>
                opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opt => {
                opt.Password.RequireDigit = false;
                opt.Password.RequiredLength = 6;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(opt => {
                opt.Events.OnRedirectToLogin = ctx => { ctx.Response.StatusCode = 401; return Task.CompletedTask; };
                opt.Events.OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = 403; return Task.CompletedTask; };
            });

            builder.Services.AddScoped<ITicketService, TicketService>();
            builder.Services.AddHostedService<RoleInitializer>();
            builder.Services.AddControllers()
                .AddJsonOptions(opt => {
                    // 🔹 Разрешаем case-insensitive привязку: фронтенд может слать camelCase
                    opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    // 🔹 Опционально: выводим ответ в camelCase (стандарт JSON)
                    opt.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                });
            var app = builder.Build();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseStaticFiles(); // ✅ Только здесь, без builder.Services

            app.MapDefaultControllerRoute();
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}
