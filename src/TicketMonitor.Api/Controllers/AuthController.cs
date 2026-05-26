using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TicketMonitor.Core.Entities;

namespace TicketMonitor.Api.Controllers
{
    public record LoginRequest(string Username, string Password);
    public record RegisterRequest(string UserName, string Email, string Password, string Role = "Client");

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var allowedRoles = new[] { "Administrator", "Manager", "Executor", "Client" };
            var role = allowedRoles.Contains(request.Role) ? request.Role : "Client";
            await _userManager.AddToRoleAsync(user, role);

            return Ok(new { message = "User created" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
                return Unauthorized(new { message = "Invalid username or password" });

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid username or password" });

            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwt(user, roles);

            return Ok(new
            {
                token,
                user = new
                {
                    id = user.Id,
                    username = user.UserName,
                    email = user.Email,
                    roles
                }
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        [HttpGet("me")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null) return Unauthorized();
            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new { id = user.Id, username = user.UserName, email = user.Email, roles });
        }

        private string GenerateJwt(ApplicationUser user, IList<string> roles)
        {
            var jwtKey = _config["Jwt:Key"] ?? "SuperSecretKeyThatIsAtLeast32CharactersLong!";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? ""),
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var expireHours = int.TryParse(_config["Jwt:ExpireHours"], out var h) ? h : 8;

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "TicketMonitor",
                audience: _config["Jwt:Audience"] ?? "TicketMonitor",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expireHours),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}