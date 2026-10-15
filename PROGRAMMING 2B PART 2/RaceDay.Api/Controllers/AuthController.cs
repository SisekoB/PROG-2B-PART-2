using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayContext _db;
    public AuthController(RaceDayContext db) => _db = db;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password)) return BadRequest("Email and password are required.");
        if (request.UserRole != "Organiser" && request.UserRole != "Participant") return BadRequest("Role must be Organiser or Participant.");
        if (await _db.Users.AnyAsync(x => x.Email == request.Email)) return Conflict("Email already exists.");
        var user = new User { FullName = request.FullName, Email = request.Email, Phone = request.Phone, UserRole = request.UserRole };
        user.PasswordHash = SessionUser.Hasher.HashPassword(user, request.Password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Created($"/api/users/{user.UserId}", new { user.UserId, user.FullName, user.Email, user.UserRole });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == request.Email);
        if (user == null || SessionUser.Hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
            return Unauthorized("Email or password is incorrect.");
        HttpContext.Session.SetInt32("UserId", user.UserId);
        HttpContext.Session.SetString("Role", user.UserRole);
        return Ok(new { user.UserId, user.FullName, user.Email, user.UserRole, message = "Logged in" });
    }

    [HttpPost("logout")]
    public IActionResult Logout() { HttpContext.Session.Clear(); return Ok(new { message = "Logged out" }); }
}
