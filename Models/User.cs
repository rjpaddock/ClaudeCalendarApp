using System.ComponentModel.DataAnnotations;

namespace CalendarManagement.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<CalendarEvent> CreatedEvents { get; set; } = new List<CalendarEvent>();
        public ICollection<EventAttendee> EventAttendees { get; set; } = new List<EventAttendee>();
    }
}
