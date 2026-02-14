using CalendarManagement.Models;

namespace CalendarManagement.ViewModels
{
    public class EventDetailsViewModel
    {
        public CalendarEvent Event { get; set; } = null!;
        public List<EventAttendeeInfo> Attendees { get; set; } = new List<EventAttendeeInfo>();
    }

    public class EventAttendeeInfo
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public ResponseStatus ResponseStatus { get; set; }
    }
}
