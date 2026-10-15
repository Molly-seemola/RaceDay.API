using Microsoft.EntityFrameworkCore;          
using RaceDay.API.Models;                     

namespace RaceDay.API.Data;                   

public class RaceDayDbContext : DbContext
{
    /// <summary>
    /// Constructor injects DbContextOptions (connection string, provider, etc.)
    /// configured in Program.cs.
    /// </summary>
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
        : base(options)
    {
    }

    // ============================================
    // DbSets — one per entity/table
    // ============================================
    public DbSet<User> Users { get; set; }                   // Users table (base for all accounts)
    public DbSet<Organiser> Organisers { get; set; }         // Organisers table (extends Users)
    public DbSet<Participant> Participants { get; set; }     // Participants table (extends Users)
    public DbSet<Event> Events { get; set; }                 // Events table (races, walks, cycles)
    public DbSet<Category> Categories { get; set; }          // Categories table (5km, 10km, etc.)
    public DbSet<Enrolment> Enrolments { get; set; }         // Enrolments table (Participant ↔ Event link)
    public DbSet<Result> Results { get; set; }               // Results table (finish times, positions)
    public DbSet<Weather> WeatherRecords { get; set; }       // Weather table (forecasts per event)

    /// <summary>
    /// Configures the database schema using EF Core's fluent API.
    /// Sets up primary keys, foreign keys, indexes, constraints, and relationships.
    /// Called once when the model is first created.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ============================================
        // USER CONFIGURATION
        // ============================================
        // Defines the shape of the Users table
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);                                       // Primary key
            entity.HasIndex(e => e.Email).IsUnique();                           // Email must be unique
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);       // Required, max 255 chars
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);// BCrypt hash, required
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(20);         // "Organiser" or "Participant"
            entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);

            // Database-level check: Role must be one of the two valid values
            entity.ToTable(t => t.HasCheckConstraint("CK_User_Role",
                "[Role] IN ('Organiser', 'Participant')"));
        });

        // ============================================
        // ORGANISER CONFIGURATION
        // ============================================
        // One-to-one relationship with User
        modelBuilder.Entity<Organiser>(entity =>
        {
            entity.HasKey(e => e.OrganiserId);                                  // Primary key
            entity.Property(e => e.CompanyName).HasMaxLength(200);              // Optional
            entity.Property(e => e.ContactNumber).IsRequired().HasMaxLength(20);

            // Each Organiser belongs to one User; cascade delete when User is removed
            entity.HasOne(e => e.User)
                .WithOne(u => u.Organiser)
                .HasForeignKey<Organiser>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================================
        // PARTICIPANT CONFIGURATION
        // ============================================
        // One-to-one relationship with User
        modelBuilder.Entity<Participant>(entity =>
        {
            entity.HasKey(e => e.ParticipantId);                                // Primary key
            entity.Property(e => e.Gender).HasMaxLength(10);                    // Optional
            entity.Property(e => e.EmergencyContact).HasMaxLength(100);
            entity.Property(e => e.MedicalInfo).HasMaxLength(500);
            entity.Property(e => e.EmergencyPhone).HasMaxLength(20);

            // Each Participant belongs to one User; cascade delete when User is removed
            entity.HasOne(e => e.User)
                .WithOne(u => u.Participant)
                .HasForeignKey<Participant>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================================
        // EVENT CONFIGURATION
        // ============================================
        // Many-to-one with Organiser
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId);                                      // Primary key
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Location).IsRequired().HasMaxLength(255);
            entity.Property(e => e.RouteMap).HasMaxLength(500);                 // URL to route map image
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);    // Running / Walking / Cycling
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);       // Draft / Published / etc.
            entity.Property(e => e.ImageUrl).HasMaxLength(500);                 // Azure Blob URL (Part 3)
            entity.Property(e => e.Distance).HasPrecision(5, 2);                // e.g. 21.10 km

            // Each Event belongs to one Organiser; cascade delete with the Organiser
            entity.HasOne(e => e.Organiser)
                .WithMany(o => o.Events)
                .HasForeignKey(e => e.OrganiserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Check constraint: EventType must be one of the three valid values
            entity.ToTable(t => t.HasCheckConstraint("CK_Event_Type",
                "[EventType] IN ('Running', 'Walking', 'Cycling')"));

            // Check constraint: Status must be one of the four valid values
            entity.ToTable(t => t.HasCheckConstraint("CK_Event_Status",
                "[Status] IN ('Draft', 'Published', 'Cancelled', 'Completed')"));
        });

        // ============================================
        // CATEGORY CONFIGURATION
        // ============================================
        // Many-to-one with Event
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId);                                   // Primary key
            entity.Property(e => e.CategoryName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Distance).HasPrecision(5, 2);                // e.g. 5.00 km
            entity.Property(e => e.EntryFee).HasPrecision(10, 2);               // e.g. 150.00

            // Each Category belongs to one Event; deleting an Event removes its Categories
            entity.HasOne(e => e.Event)
                .WithMany(ev => ev.Categories)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================================
        // ENROLMENT CONFIGURATION
        // ============================================
        // Junction table linking Participants, Events, and Categories
        modelBuilder.Entity<Enrolment>(entity =>
        {
            entity.HasKey(e => e.EnrolmentId);                                  // Primary key
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);       // Pending/Confirmed/etc.
            entity.Property(e => e.PaymentStatus).IsRequired().HasMaxLength(20);// Unpaid/Paid/Refunded

            // BibNumber must be unique, but multiple NULLs are allowed
            entity.HasIndex(e => e.BibNumber).IsUnique().HasFilter("[BibNumber] IS NOT NULL");

            // Each Enrolment references one Event
            entity.HasOne(e => e.Event)
                .WithMany(ev => ev.Enrolments)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Each Enrolment references one Participant
            entity.HasOne(e => e.Participant)
                .WithMany(p => p.Enrolments)
                .HasForeignKey(e => e.ParticipantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Each Enrolment references one Category
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Enrolments)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Prevent duplicate enrolments:
            // the same Participant cannot enrol in the same Category of the same Event twice
            entity.HasIndex(e => new { e.EventId, e.ParticipantId, e.CategoryId }).IsUnique();

            // Check constraint: Enrolment Status must be valid
            entity.ToTable(t => t.HasCheckConstraint("CK_Enrolment_Status",
                "[Status] IN ('Pending', 'Confirmed', 'Cancelled', 'Completed')"));

            // Check constraint: Payment Status must be valid
            entity.ToTable(t => t.HasCheckConstraint("CK_Enrolment_PaymentStatus",
                "[PaymentStatus] IN ('Unpaid', 'Paid', 'Refunded')"));
        });

        // ============================================
        // RESULT CONFIGURATION
        // ============================================
        // One-to-one with Enrolment
        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasKey(e => e.ResultId);                                     // Primary key
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);       // Pending/Confirmed/DQ

            // Each Result belongs to exactly one Enrolment
            entity.HasOne(e => e.Enrolment)
                .WithOne(en => en.Result)
                .HasForeignKey<Result>(e => e.EnrolmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Check constraint: Result Status must be one of the valid values
            entity.ToTable(t => t.HasCheckConstraint("CK_Result_Status",
                "[Status] IN ('Pending', 'Confirmed', 'Disqualified')"));
        });

        // ============================================
        // WEATHER CONFIGURATION
        // ============================================
        // Many-to-one with Event (multiple weather forecasts per event)
        modelBuilder.Entity<Weather>(entity =>
        {
            entity.HasKey(e => e.WeatherId);                                    // Primary key
            entity.Property(e => e.Conditions).HasMaxLength(100);               // "Sunny", "Rainy", etc.
            entity.Property(e => e.Temperature).HasPrecision(4, 1);             // e.g. 22.5 °C
            entity.Property(e => e.WindSpeed).HasPrecision(5, 2);               // e.g. 15.00 km/h
            entity.Property(e => e.Humidity).HasPrecision(3, 1);                // e.g. 65.0 %
            entity.Property(e => e.Precipitation).HasPrecision(4, 1);           // e.g. 10.0 %

            // Each Weather record belongs to one Event; cascade delete with Event
            entity.HasOne(e => e.Event)
                .WithMany(ev => ev.WeatherRecords)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================================
        // SEED DATA (Optional — for testing)
        // ============================================
        // Seed initial users into the database on creation.
        SeedData(modelBuilder);
    }

    /// <summary>
    /// Seeds the database with initial test data.
    /// Called from OnModelCreating to pre-populate the Users table.
    /// </summary>
    private void SeedData(ModelBuilder modelBuilder)
    {
        // Note: Passwords are hashed - use "Password123!" for testing
        // You should generate these hashes yourself

        // Seed 4 users: 2 Organisers and 2 Participants
        modelBuilder.Entity<User>().HasData(
            new User { UserId = 1, Email = "john.organiser@raceday.co.za", PasswordHash = "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewKyL6bqGxT9rKy", FirstName = "John", LastName = "Smith", Role = "Organiser", DateRegistered = DateTime.UtcNow, IsActive = true },
            new User { UserId = 2, Email = "sarah.organiser@raceday.co.za", PasswordHash = "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewKyL6bqGxT9rKy", FirstName = "Sarah", LastName = "Johnson", Role = "Organiser", DateRegistered = DateTime.UtcNow, IsActive = true },
            new User { UserId = 3, Email = "thabo.participant@raceday.co.za", PasswordHash = "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewKyL6bqGxT9rKy", FirstName = "Thabo", LastName = "Mokoena", Role = "Participant", DateRegistered = DateTime.UtcNow, IsActive = true },
            new User { UserId = 4, Email = "linda.participant@raceday.co.za", PasswordHash = "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewKyL6bqGxT9rKy", FirstName = "Linda", LastName = "van der Merwe", Role = "Participant", DateRegistered = DateTime.UtcNow, IsActive = true }
        );
    }
}