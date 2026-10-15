using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly RaceDayContext _db;
    public UsersController(RaceDayContext db) => _db = db;

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var id = SessionUser.Id(HttpContext);
        if (id == null) return Unauthorized();
        var user = await _db.Users.FindAsync(id);
        return user == null ? NotFound() : Ok(new { user.UserId, user.FullName, user.Email, user.Phone, user.UserRole });
    }

    [HttpPut("me")]
    public async Task<IActionResult> Update(ProfileRequest request)
    {
        var id = SessionUser.Id(HttpContext);
        if (id == null) return Unauthorized();
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        user.FullName = request.FullName; user.Phone = request.Phone;
        await _db.SaveChangesAsync();
        return Ok(new { user.UserId, user.FullName, user.Email, user.Phone, user.UserRole });
    }

    [HttpGet("me/enrolments")]
    public async Task<IActionResult> Enrolments()
    {
        var id = SessionUser.Id(HttpContext);
        if (id == null) return Unauthorized();
        if (!SessionUser.Is(HttpContext, "Participant")) return Forbid();
        return Ok(await _db.Enrolments.Where(x => x.ParticipantId == id).ToListAsync());
    }
}
