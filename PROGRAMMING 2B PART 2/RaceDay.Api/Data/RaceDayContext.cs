using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class RaceDayContext : DbContext
{
    public RaceDayContext(DbContextOptions<RaceDayContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().Property(x => x.FullName).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<User>().Property(x => x.Email).HasMaxLength(150).IsRequired();
        modelBuilder.Entity<User>().Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
        modelBuilder.Entity<User>().Property(x => x.Phone).HasMaxLength(30);
        modelBuilder.Entity<User>().Property(x => x.UserRole).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<Event>().Property(x => x.Name).HasMaxLength(150).IsRequired();
        modelBuilder.Entity<Event>().Property(x => x.Description).HasMaxLength(500);
        modelBuilder.Entity<Event>().Property(x => x.EventType).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<Event>().Property(x => x.Location).HasMaxLength(150).IsRequired();
        modelBuilder.Entity<Category>().Property(x => x.Name).HasMaxLength(80).IsRequired();
        modelBuilder.Entity<Category>().Property(x => x.Description).HasMaxLength(250);
        modelBuilder.Entity<Enrolment>().Property(x => x.EmergencyContactName).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<Enrolment>().Property(x => x.EmergencyContactPhone).HasMaxLength(30).IsRequired();
        modelBuilder.Entity<Enrolment>().Property(x => x.EnrolmentStatus).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<Result>().Property(x => x.ResultStatus).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Category>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<EventCategory>().HasKey(x => new { x.EventId, x.CategoryId });
        modelBuilder.Entity<Enrolment>().HasIndex(x => new { x.EventId, x.ParticipantId }).IsUnique();
        modelBuilder.Entity<Result>().HasIndex(x => new { x.EventId, x.ParticipantId }).IsUnique();
        modelBuilder.Entity<User>().Property(x => x.UserRole).HasDefaultValue("Participant");
        modelBuilder.Entity<Enrolment>().Property(x => x.EnrolmentStatus).HasDefaultValue("Active");
        modelBuilder.Entity<Result>().Property(x => x.ResultStatus).HasDefaultValue("Finished");
        modelBuilder.Entity<Event>().ToTable("Events", table => table.HasCheckConstraint("CK_Events_Distance", "DistanceKm > 0"));
        modelBuilder.Entity<Event>().ToTable("Events", table => table.HasCheckConstraint("CK_Events_MaxParticipants", "MaxParticipants > 0"));
        modelBuilder.Entity<Event>().ToTable("Events", table => table.HasCheckConstraint("CK_Events_Type", "EventType IN ('Running', 'Walking', 'Cycling')"));
        modelBuilder.Entity<User>().ToTable("Users", table => table.HasCheckConstraint("CK_Users_Role", "UserRole IN ('Organiser', 'Participant')"));
        modelBuilder.Entity<Enrolment>().ToTable("Enrolments", table => table.HasCheckConstraint("CK_Enrolments_Status", "EnrolmentStatus IN ('Active', 'Cancelled')"));
        modelBuilder.Entity<Result>().ToTable("Results", table => table.HasCheckConstraint("CK_Results_Position", "Position IS NULL OR Position > 0"));
        modelBuilder.Entity<Result>().ToTable("Results", table => table.HasCheckConstraint("CK_Results_Status", "ResultStatus IN ('Finished', 'Did Not Finish', 'Disqualified')"));
    }
}
