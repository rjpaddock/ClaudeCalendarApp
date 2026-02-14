using CalendarManagement.Models;

namespace CalendarManagement.ViewModels
{
    public class WeekCalendarViewModel
    {
        public int Year { get; set; }
        public int Week { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<DayColumn> Days { get; set; } = new List<DayColumn>();
        public List<int> Hours { get; set; } = new List<int>();
    }

    public class DayColumn
    {
        public DateTime Date { get; set; }
        public string DayName { get; set; } = string.Empty;
        public bool IsToday { get; set; }
        public List<CalendarEvent> Events { get; set; } = new List<CalendarEvent>();
    }
}
