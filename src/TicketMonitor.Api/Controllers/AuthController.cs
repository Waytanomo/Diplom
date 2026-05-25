using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TicketMonitor.Core.Entities;

namespace TicketMonitor.Api.Controllers
{
    // DTO для входа — явные свойства, никаких "магических строк"
    public record LoginRequest(string Username, string Password);
    public record RegisterRequest(string Username, string Email, string Password, string Role = "Client");

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, request.Role);
            return Ok(new { message = "User created" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _signInManager.PasswordSignInAsync(
                request.Username,
                request.Password,
                isPersistent: true,
                lockoutOnFailure: false);

            return result.Succeeded
                ? Ok(new { message = "Успешный вход" })
                : Unauthorized(new { message = "Неверные логин или пароль" });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }
    }
}