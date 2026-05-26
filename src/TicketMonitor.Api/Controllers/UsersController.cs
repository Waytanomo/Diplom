using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpPut("{id}/role")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ChangeRole(string id, [FromBody] ChangeRoleRequest request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var allowedRoles = new[] { "Administrator", "Manager", "Executor", "Client" };
            if (!allowedRoles.Contains(request.Role))
                return BadRequest(new { message = "Invalid role" });

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