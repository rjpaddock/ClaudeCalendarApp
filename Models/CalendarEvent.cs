using System.ComponentModel.DataAnnotations;

namespace CalendarManagement.Models
{
    public class CalendarEvent
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [Required]
        public int CreatedById { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User CreatedBy { get; set; } = null!;
        public ICollection<EventAttendee> Attendees { get; set; } = new List<EventAttendee>();
    }
}
