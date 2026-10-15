using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayContext _db;
    public CategoriesController(RaceDayContext db) => _db = db;
    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _db.Categories.ToListAsync());
    [HttpPost] public async Task<IActionResult> Create(CategoryRequest request) { if (SessionUser.Id(HttpContext) == null) return Unauthorized(); if (!SessionUser.Is(HttpContext, "Organiser")) return Forbid(); if (await _db.Categories.AnyAsync(x => x.Name == request.Name)) return Conflict(); var item = new Category { Name = request.Name, Description = request.Description }; _db.Categories.Add(item); await _db.SaveChangesAsync(); return Created($"/api/categories/{item.CategoryId}", item); }
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id, CategoryRequest request) { if (SessionUser.Id(HttpContext) == null) return Unauthorized(); if (!SessionUser.Is(HttpContext, "Organiser")) return Forbid(); var item = await _db.Categories.FindAsync(id); if (item == null) return NotFound(); item.Name = request.Name; item.Description = request.Description; await _db.SaveChangesAsync(); return Ok(item); }
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id) { if (SessionUser.Id(HttpContext) == null) return Unauthorized(); if (!SessionUser.Is(HttpContext, "Organiser")) return Forbid(); var item = await _db.Categories.FindAsync(id); if (item == null) return NotFound(); if (await _db.EventCategories.AnyAsync(x => x.CategoryId == id)) return Conflict(); _db.Categories.Remove(item); await _db.SaveChangesAsync(); return NoContent(); }
}
