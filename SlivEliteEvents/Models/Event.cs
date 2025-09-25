namespace SlivEliteEvents.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsAllDay { get; set; }
        public string Location { get; set; }
        public int GuestCapacity { get; set; }
        public string? ContactPhone { get; set; }
        public string Status { get; set; } // Pending, Confirmed, Completed
        public DateTime CreatedAt { get; set; }
    }
}