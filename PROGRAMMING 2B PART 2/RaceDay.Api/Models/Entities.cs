namespace RaceDay.Api.Models;

public class User
{
    public int UserId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string? Phone { get; set; }
    public string UserRole { get; set; } = "Participant";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Event
{
    public int EventId { get; set; }
    public int OrganiserId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string EventType { get; set; } = "Running";
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public int MaxParticipants { get; set; }
}

public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}

public class EventCategory
{
    public int EventId { get; set; }
    public int CategoryId { get; set; }
}

public class Enrolment
{
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public int ParticipantId { get; set; }
    public int CategoryId { get; set; }
    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
    public string EmergencyContactName { get; set; } = "";
    public string EmergencyContactPhone { get; set; } = "";
    public string EnrolmentStatus { get; set; } = "Active";
}

public class Result
{
    public int ResultId { get; set; }
    public int EventId { get; set; }
    public int ParticipantId { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? Position { get; set; }
    public string ResultStatus { get; set; } = "Finished";
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
