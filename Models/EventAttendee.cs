using System.ComponentModel.DataAnnotations;

namespace CalendarManagement.Models
{
    public class EventAttendee
    {
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public int UserId { get; set; }

        public ResponseStatus ResponseStatus { get; set; } = ResponseStatus.Pending;

        // Navigation properties
        public CalendarEvent Event { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
