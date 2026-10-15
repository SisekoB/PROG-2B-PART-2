using Microsoft.AspNetCore.Identity;
using RaceDay.Api.Models;

namespace RaceDay.Api.Services;

public static class SessionUser
{
    public static int? Id(HttpContext context) => context.Session.GetInt32("UserId");
    public static string? Role(HttpContext context) => context.Session.GetString("Role");

    public static bool Is(HttpContext context, string role) => Role(context) == role;

    public static PasswordHasher<User> Hasher { get; } = new();
}
