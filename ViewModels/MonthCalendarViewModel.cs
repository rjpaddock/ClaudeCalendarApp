using CalendarManagement.Models;

namespace CalendarManagement.ViewModels
{
    public class MonthCalendarViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public List<List<DayCell>> Weeks { get; set; } = new List<List<DayCell>>();
    }

    public class DayCell
    {
        public int Day { get; set; }
        public DateTime Date { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsToday { get; set; }
        public List<CalendarEvent> Events { get; set; } = new List<CalendarEvent>();
    }
}
