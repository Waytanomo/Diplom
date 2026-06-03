using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TicketMonitor.Core.DTOs;
using TicketMonitor.Core.Interfaces;

namespace TicketMonitor.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _svc;

        public TicketsController(ITicketService svc) => _svc = svc;

        private string UserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException();

        private IEnumerable<string> UserRoles =>
            User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);

        // Все аутентифицированные — включая Executor (видит только свои через сервис)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null,
            [FromQuery] string? priority = null,
            [FromQuery] string? search = null)
        {
            return Ok(await _svc.GetAllAsync(UserId, UserRoles, page, pageSize, status, priority, search));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ticket = await _svc.GetByIdAsync(id);
            return ticket is null ? NotFound() : Ok(ticket);
        }

        [HttpGet("{id}/comments")]
        public async Task<IActionResult> GetComments(int id)
            => Ok(await _svc.GetCommentsAsync(id));

        // Executor НЕ может создавать тикеты
        [HttpPost]
        [Authorize(Roles = "Manager,Administrator")]
        public async Task<IActionResult> Create([FromBody] CreateTicketDto dto)
        {
            var res = await _svc.CreateAsync(dto, UserId);
            return CreatedAtAction(nameof(GetById), new { id = res.Id }, res);
        }

        // Executor может менять статус своих тикетов
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Manager,Executor,Administrator")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusDto dto)
            => await _svc.ChangeStatusAsync(id, dto, UserId) ? Ok() : NotFound();

        // Назначать может только Manager / Administrator
        [HttpPut("{id}/assign")]
        [Authorize(Roles = "Manager,Administrator")]
        public async Task<IActionResult> Assign(int id, [FromBody] AssignTicketDto dto)
            => await _svc.AssignAsync(id, dto, UserId) ? Ok() : NotFound();

        // Комментировать может любой аутентифицированный
        [HttpPost("{id}/comments")]
        public async Task<IActionResult> AddComment(int id, [FromBody] AddCommentDto dto)
            => Ok(await _svc.AddCommentAsync(id, dto, UserId));

        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager,Administrator")]
        public async Task<IActionResult> Delete(int id)
            => await _svc.DeleteAsync(id, UserId) ? NoContent() : NotFound();

        [HttpGet("stats")]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> GetStats()
            => Ok(await _svc.GetStatsAsync());
    }
}