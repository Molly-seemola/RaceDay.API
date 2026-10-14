namespace RaceDay.API.Models;

public class Enrolment
{
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public int ParticipantId { get; set; }
    public int CategoryId { get; set; }
    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending"; // "Pending", "Confirmed", "Cancelled", "Completed"
    public string PaymentStatus { get; set; } = "Unpaid"; // "Unpaid", "Paid", "Refunded"
    public int? BibNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Event Event { get; set; } = null!;
    public Participant Participant { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public Result? Result { get; set; }
}