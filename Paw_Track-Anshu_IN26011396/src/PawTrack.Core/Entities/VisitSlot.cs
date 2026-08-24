namespace PawTrack.Core.Entities;

public class VisitSlot
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int Capacity { get; set; }
    public int BookedCount { get; set; }

    // Navigation properties
    public Branch? Branch { get; set; }
    public ICollection<VisitBooking> VisitBookings { get; set; } = new List<VisitBooking>();
}
