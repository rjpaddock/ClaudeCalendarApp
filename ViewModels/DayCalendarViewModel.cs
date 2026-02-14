using CalendarManagement.Models;

namespace CalendarManagement.ViewModels
{
    public class DayCalendarViewModel
    {
        public DateTime Date { get; set; }
        public string DateFormatted { get; set; } = string.Empty;
        public bool IsToday { get; set; }
        public List<CalendarEvent> Events { get; set; } = new List<CalendarEvent>();
        public List<int> Hours { get; set; } = new List<int>();
    }
}
