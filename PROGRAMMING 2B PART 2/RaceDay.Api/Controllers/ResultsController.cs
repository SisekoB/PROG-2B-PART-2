using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
public class ResultsController : ControllerBase
{
    private readonly RaceDayContext _db;
    public ResultsController(RaceDayContext db) => _db = db;
    [HttpGet("events/{eventId:int}/results")] public async Task<IActionResult> Get(int eventId) => Ok(await _db.Results.Where(x => x.EventId == eventId).ToListAsync());
    [HttpPost("events/{eventId:int}/results")] public async Task<IActionResult> Create(int eventId, ResultRequest request) { if (SessionUser.Id(HttpContext) == null) return Unauthorized(); if (!SessionUser.Is(HttpContext, "Organiser")) return Forbid(); if (!await _db.Events.AnyAsync(x => x.EventId == eventId && x.OrganiserId == SessionUser.Id(HttpContext))) return Forbid(); if (await _db.Results.AnyAsync(x => x.EventId == eventId && x.ParticipantId == request.ParticipantId)) return Conflict(); var item = new Result { EventId = eventId, ParticipantId = request.ParticipantId, FinishTime = request.FinishTime, Position = request.Position, ResultStatus = request.Status }; _db.Results.Add(item); await _db.SaveChangesAsync(); return Created($"/api/results/{item.ResultId}", item); }
    [HttpPut("results/{id:int}")] public async Task<IActionResult> Update(int id, ResultRequest request) { if (SessionUser.Id(HttpContext) == null) return Unauthorized(); if (!SessionUser.Is(HttpContext, "Organiser")) return Forbid(); var item = await _db.Results.FindAsync(id); if (item == null) return NotFound(); if (!await _db.Events.AnyAsync(x => x.EventId == item.EventId && x.OrganiserId == SessionUser.Id(HttpContext))) return Forbid(); item.FinishTime = request.FinishTime; item.Position = request.Position; item.ResultStatus = request.Status; await _db.SaveChangesAsync(); return Ok(item); }
}
