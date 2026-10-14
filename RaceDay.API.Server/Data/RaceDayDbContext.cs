using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Organiser> Organisers { get; set; }
    public DbSet<Participant> Participants { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Enrolment> Enrolments { get; set; }
    public DbSet<Result> Results { get; set; }
    public DbSet<Weather> WeatherRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ============================================
        // USER CONFIGURATION
        // ============================================
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);

            entity.ToTable(t => t.HasCheckConstraint("CK_User_Role",
                "[Role] IN ('Organiser', 'Participant')"));
        });

        // ============================================
        // ORGANISER CONFIGURATION
        // ============================================
        modelBuilder.Entity<Organiser>(entity =>
        {
            entity.HasKey(e => e.OrganiserId);
            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.ContactNumber).IsRequired().HasMaxLength(20);

            entity.HasOne(e => e.User)
                .WithOne(u => u.Organiser)
                .HasForeignKey<Organiser>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================================
        // PARTICIPANT CONFIGURATION
        // ============================================
        modelBuilder.Entity<Participant>(entity =>
        {
            entity.HasKey(e => e.ParticipantId);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.EmergencyContact).HasMaxLength(100);
            entity.Property(e => e.MedicalInfo).HasMaxLength(500);
            entity.Property(e => e.EmergencyPhone).HasMaxLength(20);

            entity.HasOne(e => e.User)
                .WithOne(u => u.Participant)
                .HasForeignKey<Participant>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================================
        // EVENT CONFIGURATION
        // ============================================
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Location).IsRequired().HasMaxLength(255);
            entity.Property(e => e.RouteMap).HasMaxLength(500);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Distance).HasPrecision(5, 2);

            entity.HasOne(e => e.Organiser)
                .WithMany(o => o.Events)
                .HasForeignKey(e => e.OrganiserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.ToTable(t => t.HasCheckConstraint("CK_Event_Type",
                "[EventType] IN ('Running', 'Walking', 'Cycling')"));

            entity.ToTable(t => t.HasCheckConstraint("CK_Event_Status",
                "[Status] IN ('Draft', 'Published', 'Cancelled', 'Completed')"));
        });

        // ============================================
        // CATEGORY CONFIGURATION
        // ============================================
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId);
            entity.Property(e => e.CategoryName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Distance).HasPrecision(5, 2);
            entity.Property(e => e.EntryFee).HasPrecision(10, 2);

            entity.HasOne(e => e.Event)
                .WithMany(ev => ev.Categories)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================================
        // ENROLMENT CONFIGURATION
        // ============================================
        modelBuilder.Entity<Enrolment>(entity =>
        {
            entity.HasKey(e => e.EnrolmentId);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.PaymentStatus).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.BibNumber).IsUnique().HasFilter("[BibNumber] IS NOT NULL");

            entity.HasOne(e => e.Event)
                .WithMany(ev => ev.Enrolments)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Participant)
                .WithMany(p => p.Enrolments)
                .HasForeignKey(e => e.ParticipantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Enrolments)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Prevent duplicate enrolments
            entity.HasIndex(e => new { e.EventId, e.ParticipantId, e.CategoryId }).IsUnique();

            entity.ToTable(t => t.HasCheckConstraint("CK_Enrolment_Status",
                "[Status] IN ('Pending', 'Confirmed', 'Cancelled', 'Completed')"));

            entity.ToTable(t => t.HasCheckConstraint("CK_Enrolment_PaymentStatus",
                "[PaymentStatus] IN ('Unpaid', 'Paid', 'Refunded')"));
        });

        // ============================================
        // RESULT CONFIGURATION
        // ============================================
        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasKey(e => e.ResultId);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);

            entity.HasOne(e => e.Enrolment)
                .WithOne(en => en.Result)
                .HasForeignKey<Result>(e => e.EnrolmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.ToTable(t => t.HasCheckConstraint("CK_Result_Status",
                "[Status] IN ('Pending', 'Confirmed', 'Disqualified')"));
        });

        // ============================================
        // WEATHER CONFIGURATION
        // ============================================
        modelBuilder.Entity<Weather>(entity =>
        {
            entity.HasKey(e => e.WeatherId);
            entity.Property(e => e.Conditions).HasMaxLength(100);
            entity.Property(e => e.Temperature).HasPrecision(4, 1);
            entity.Property(e => e.WindSpeed).HasPrecision(5, 2);
            entity.Property(e => e.Humidity).HasPrecision(3, 1);
            entity.Property(e => e.Precipitation).HasPrecision(4, 1);

            entity.HasOne(e => e.Event)
                .WithMany(ev => ev.WeatherRecords)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================================
        // SEED DATA (Optional - for testing)
        // ============================================
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Note: Passwords are hashed - use "Password123!" for testing
        // You should generate these hashes yourself

        modelBuilder.Entity<User>().HasData(
            new User { UserId = 1, Email = "john.organiser@raceday.co.za", PasswordHash = "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewKyL6bqGxT9rKy", FirstName = "John", LastName = "Smith", Role = "Organiser", DateRegistered = DateTime.UtcNow, IsActive = true },
            new User { UserId = 2, Email = "sarah.organiser@raceday.co.za", PasswordHash = "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewKyL6bqGxT9rKy", FirstName = "Sarah", LastName = "Johnson", Role = "Organiser", DateRegistered = DateTime.UtcNow, IsActive = true },
            new User { UserId = 3, Email = "thabo.participant@raceday.co.za", PasswordHash = "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewKyL6bqGxT9rKy", FirstName = "Thabo", LastName = "Mokoena", Role = "Participant", DateRegistered = DateTime.UtcNow, IsActive = true },
            new User { UserId = 4, Email = "linda.participant@raceday.co.za", PasswordHash = "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewKyL6bqGxT9rKy", FirstName = "Linda", LastName = "van der Merwe", Role = "Participant", DateRegistered = DateTime.UtcNow, IsActive = true }
        );
    }
}