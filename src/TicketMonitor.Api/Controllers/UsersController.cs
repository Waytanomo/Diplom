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

        private static readonly string[] AllowedRoles = { "Administrator", "Manager", "Executor" };

        public UsersController(ITicketService svc, UserManager<ApplicationUser> userManager)
        {
            _svc = svc;
            _userManager = userManager;
        }

        //Получить всех пользователей
        [HttpGet]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> GetAll()
            => Ok(await _svc.GetUsersAsync());

        //Создать нового пользователя
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName))
                return BadRequest(new { message = "Имя пользователя обязательно" });

            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Пароль обязателен" });

            if (!AllowedRoles.Contains(request.Role))
                return BadRequest(new { message = $"Недопустимая роль" });

            var generatedEmail = $"{request.UserName.Trim().ToLower()}@local.tier";

            var user = new ApplicationUser
            {
                UserName = request.UserName.Trim(),
                Email = generatedEmail,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { message = "Ошибка создания", errors });
            }

            await _userManager.AddToRoleAsync(user, request.Role);
            return CreatedAtAction(nameof(GetAll), new UserDto(user.Id, user.UserName ?? ""));
        }

        //Изменить роль пользователя
        [HttpPut("{id}/role")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ChangeRole(string id, [FromBody] ChangeRoleRequest request)
        {
            if (!AllowedRoles.Contains(request.Role))
                return BadRequest(new { message = "Недопустимая роль" });

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var current = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, current);
            await _userManager.AddToRoleAsync(user, request.Role);
            return Ok();
        }

        //Удалить пользователя
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