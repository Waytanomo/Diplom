using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TicketMonitor.Core.DTOs;
using TicketMonitor.Core.Entities;
using TicketMonitor.Core.Interfaces;

namespace TicketMonitor.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ITicketService _svc;
        private readonly UserManager<ApplicationUser> _userManager;

        // Executor исключён из допустимых ролей
        private static readonly string[] AllowedRoles = { "Administrator", "Manager", "Client" };

        public UsersController(ITicketService svc, UserManager<ApplicationUser> userManager)
        {
            _svc = svc;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _svc.GetUsersAsync());
        }

        /// <summary>
        /// Создание пользователя администратором.
        /// Email необязателен — если не передан, генерируется username@local.tier
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName))
                return BadRequest(new { message = "Имя пользователя обязательно" });

            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Пароль обязателен" });

            if (!AllowedRoles.Contains(request.Role))
                return BadRequest(new { message = $"Недопустимая роль. Допустимые: {string.Join(", ", AllowedRoles)}" });

            // Email: используем переданный или генерируем заглушку
            var email = !string.IsNullOrWhiteSpace(request.Email)
                ? request.Email
                : $"{request.UserName.ToLower().Trim()}@local.tier";

            var user = new ApplicationUser
            {
                UserName = request.UserName.Trim(),
                Email = email,
                EmailConfirmed = true   // подтверждение не требуется во внутренней системе
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { message = "Ошибка создания пользователя", errors });
            }

            await _userManager.AddToRoleAsync(user, request.Role);

            var roles = await _userManager.GetRolesAsync(user);
            return CreatedAtAction(nameof(GetAll), new UserDto(
                user.Id,
                user.UserName ?? "",
                user.Email ?? "",
                roles));
        }

        [HttpPut("{id}/role")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ChangeRole(string id, [FromBody] ChangeRoleRequest request)
        {
            if (!AllowedRoles.Contains(request.Role))
                return BadRequest(new { message = $"Недопустимая роль" });

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, request.Role);

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded ? NoContent() : BadRequest(result.Errors);
        }
    }

    public record ChangeRoleRequest(string Role);
}