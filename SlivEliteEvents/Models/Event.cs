using System.ComponentModel.DataAnnotations;

namespace SlivEliteEvents.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; } // Nullable for all-day events

        public bool IsAllDay { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; }

        [Range(1, 500)]
        public int GuestCapacity { get; set; }

        [StringLength(20)]
        public string ContactPhone { get; set; }

        public string Status { get; set; } // e.g., "Pending", "Confirmed", "Completed"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}