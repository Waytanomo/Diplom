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
        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();

        [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());
        [HttpGet("{id}")] public async Task<IActionResult> GetById(int id) => await _svc.GetByIdAsync(id) is { } t ? Ok(t) : NotFound();

        [HttpPost]
        [Authorize(Roles = "Manager,Client")]
        public async Task<IActionResult> Create([FromBody] CreateTicketDto dto)
        {
            var res = await _svc.CreateAsync(dto, UserId);
            return CreatedAtAction(nameof(GetById), new { id = res.Id }, res);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Manager,Executor")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusDto dto)
            => await _svc.ChangeStatusAsync(id, dto, UserId) ? Ok() : NotFound();

        [HttpPut("{id}/assign")]
        [Authorize(Roles = "Manager,Administrator")]
        public async Task<IActionResult> Assign(int id, [FromBody] AssignTicketDto dto)
            => await _svc.AssignAsync(id, dto, UserId) ? Ok() : NotFound();

        [HttpPost("{id}/comments")]
        public async Task<IActionResult> AddComment(int id, [FromBody] AddCommentDto dto)
            => Ok(await _svc.AddCommentAsync(id, dto, UserId));

        [HttpGet("stats")]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> GetStats() => Ok(await _svc.GetStatsAsync());

        /*[HttpDelete("{id}/delete")]
        [Authorize(Roles = "Manager, Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var res =
        }*/
    }
}
