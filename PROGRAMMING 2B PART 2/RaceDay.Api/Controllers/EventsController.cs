using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayContext _db;
    public EventsController(RaceDayContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(string? type, string? location)
    {
        var query = _db.Events.Where(x => x.EventDate >= DateTime.Today).AsQueryable();
        if (!string.IsNullOrWhiteSpace(type)) query = query.Where(x => x.EventType == type);
        if (!string.IsNullOrWhiteSpace(location)) query = query.Where(x => x.Location.Contains(location));
        return Ok(await query.ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _db.Events.FindAsync(id); if (item == null) return NotFound();
        var categoryIds = await _db.EventCategories.Where(x => x.EventId == id).Select(x => x.CategoryId).ToListAsync();
        var placesTaken = await _db.Enrolments.CountAsync(x => x.EventId == id && x.EnrolmentStatus == "Active");
        return Ok(new { item, categoryIds, availablePlaces = item.MaxParticipants - placesTaken });
    }

    [HttpPost("{eventId:int}/categories/{categoryId:int}")]
    public async Task<IActionResult> AddCategory(int eventId, int categoryId)
    {
        if (SessionUser.Id(HttpContext) == null) return Unauthorized();
        if (!SessionUser.Is(HttpContext, "Organiser")) return Forbid();
        if (!await _db.Events.AnyAsync(x => x.EventId == eventId && x.OrganiserId == SessionUser.Id(HttpContext))) return Forbid();
        if (!await _db.Categories.AnyAsync(x => x.CategoryId == categoryId)) return NotFound();
        if (await _db.EventCategories.AnyAsync(x => x.EventId == eventId && x.CategoryId == categoryId)) return Conflict();
        _db.EventCategories.Add(new EventCategory { EventId = eventId, CategoryId = categoryId }); await _db.SaveChangesAsync();
        return Created($"/api/events/{eventId}/categories/{categoryId}", new { eventId, categoryId });
    }

    [HttpPost]
    public async Task<IActionResult> Create(EventRequest request)
    {
        if (SessionUser.Id(HttpContext) == null) return Unauthorized();
        if (!SessionUser.Is(HttpContext, "Organiser")) return Forbid();
        if (request.DistanceKm <= 0 || request.MaxParticipants <= 0) return BadRequest("Distance and places must be positive.");
        if (request.EventType != "Running" && request.EventType != "Walking" && request.EventType != "Cycling") return BadRequest("Event type is invalid.");
        var item = new Event { OrganiserId = SessionUser.Id(HttpContext)!.Value, Name = request.Name, Description = request.Description, EventType = request.EventType, EventDate = request.EventDate, Location = request.Location, DistanceKm = request.DistanceKm, MaxParticipants = request.MaxParticipants };
        _db.Events.Add(item); await _db.SaveChangesAsync(); return Created($"/api/events/{item.EventId}", item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, EventRequest request)
    {
        var item = await _db.Events.FindAsync(id); if (item == null) return NotFound();
        if (SessionUser.Id(HttpContext) == null) return Unauthorized();
        if (!SessionUser.Is(HttpContext, "Organiser") || item.OrganiserId != SessionUser.Id(HttpContext)) return Forbid();
        item.Name = request.Name; item.Description = request.Description; item.EventType = request.EventType; item.EventDate = request.EventDate; item.Location = request.Location; item.DistanceKm = request.DistanceKm; item.MaxParticipants = request.MaxParticipants;
        await _db.SaveChangesAsync(); return Ok(item);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Events.FindAsync(id); if (item == null) return NotFound();
        if (SessionUser.Id(HttpContext) == null) return Unauthorized();
        if (!SessionUser.Is(HttpContext, "Organiser") || item.OrganiserId != SessionUser.Id(HttpContext)) return Forbid();
        if (await _db.Enrolments.AnyAsync(x => x.EventId == id)) return Conflict("Event has enrolments.");
        _db.Events.Remove(item); await _db.SaveChangesAsync(); return NoContent();
    }
}
