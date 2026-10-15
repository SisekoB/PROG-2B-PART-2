namespace RaceDay.Api.Models;

public record RegisterRequest(string FullName, string Email, string Password, string? Phone, string UserRole);
public record LoginRequest(string Email, string Password);
public record ProfileRequest(string FullName, string? Phone);
public record EventRequest(string Name, string? Description, string EventType, DateTime EventDate, string Location, decimal DistanceKm, int MaxParticipants);
public record CategoryRequest(string Name, string? Description);
public record EnrolmentRequest(int CategoryId, string EmergencyContactName, string EmergencyContactPhone);
public record ResultRequest(int ParticipantId, TimeSpan? FinishTime, int? Position, string Status);
