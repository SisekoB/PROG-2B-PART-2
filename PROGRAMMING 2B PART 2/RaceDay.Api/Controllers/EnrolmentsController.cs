using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayContext _db;
    public EnrolmentsController(RaceDayContext db) => _db = db;

    [HttpPost("events/{eventId:int}/enrolments")]
    public async Task<IActionResult> Create(int eventId, EnrolmentRequest request)
    {
        var participant = SessionUser.Id(HttpContext); if (participant == null) return Unauthorized(); if (!SessionUser.Is(HttpContext, "Participant")) return Forbid();
        var race = await _db.Events.FindAsync(eventId); if (race == null) return NotFound();
        if (!await _db.EventCategories.AnyAsync(x => x.EventId == eventId && x.CategoryId == request.CategoryId)) return BadRequest("Category is not available for this event.");
        if (await _db.Enrolments.AnyAsync(x => x.EventId == eventId && x.ParticipantId == participant)) return Conflict("Already enrolled.");
        if (await _db.Enrolments.CountAsync(x => x.EventId == eventId && x.EnrolmentStatus == "Active") >= race.MaxParticipants) return Conflict("Event is full.");
        var item = new Enrolment { EventId = eventId, ParticipantId = participant.Value, CategoryId = request.CategoryId, EmergencyContactName = request.EmergencyContactName, EmergencyContactPhone = request.EmergencyContactPhone };
        _db.Enrolments.Add(item); await _db.SaveChangesAsync(); return Created($"/api/enrolments/{item.EnrolmentId}", item);
    }

    [HttpGet("events/{eventId:int}/enrolments")]
    public async Task<IActionResult> GetEventEnrolments(int eventId)
    {
        var organiser = SessionUser.Id(HttpContext); if (organiser == null) return Unauthorized(); if (!SessionUser.Is(HttpContext, "Organiser")) return Forbid();
        if (!await _db.Events.AnyAsync(x => x.EventId == eventId && x.OrganiserId == organiser)) return Forbid();
        return Ok(await _db.Enrolments.Where(x => x.EventId == eventId).ToListAsync());
    }

    [HttpDelete("enrolments/{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var participant = SessionUser.Id(HttpContext); if (participant == null) return Unauthorized(); if (!SessionUser.Is(HttpContext, "Participant")) return Forbid();
        var item = await _db.Enrolments.FindAsync(id); if (item == null) return NotFound(); if (item.ParticipantId != participant) return Forbid();
        item.EnrolmentStatus = "Cancelled"; await _db.SaveChangesAsync(); return NoContent();
    }
}
