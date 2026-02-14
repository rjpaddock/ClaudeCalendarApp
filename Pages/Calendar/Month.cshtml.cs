using CalendarManagement.Data;
using CalendarManagement.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CalendarManagement.Pages.Calendar
{
    public class MonthModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public MonthModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public MonthCalendarViewModel ViewModel { get; set; } = new MonthCalendarViewModel();

        public async Task OnGetAsync(int? year, int? month)
        {
            var targetDate = year.HasValue && month.HasValue
                ? new DateTime(year.Value, month.Value, 1)
                : new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            ViewModel.Year = targetDate.Year;
            ViewModel.Month = targetDate.Month;
            ViewModel.MonthName = targetDate.ToString("MMMM yyyy");

            // Get all events for this month and adjacent days shown in calendar
            var firstDayOfMonth = new DateTime(targetDate.Year, targetDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Start from the first day of week containing the first day of month
            var startDate = firstDayOfMonth.AddDays(-(int)firstDayOfMonth.DayOfWeek);
            // End at the last day of week containing the last day of month
            var endDate = lastDayOfMonth.AddDays(6 - (int)lastDayOfMonth.DayOfWeek);

            var events = await _context.CalendarEvents
                .Where(e => e.StartDateTime >= startDate && e.StartDateTime <= endDate.AddDays(1))
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();

            // Build calendar grid
            var currentDate = startDate;
            while (currentDate <= endDate)
            {
                var week = new List<DayCell>();
                for (int i = 0; i < 7; i++)
                {
                    var dayEvents = events
                        .Where(e => e.StartDateTime.Date == currentDate.Date)
                        .ToList();

                    week.Add(new DayCell
                    {
                        Day = currentDate.Day,
                        Date = currentDate,
                        IsCurrentMonth = currentDate.Month == targetDate.Month,
                        IsToday = currentDate.Date == DateTime.Today,
                        Events = dayEvents
                    });

                    currentDate = currentDate.AddDays(1);
                }
                ViewModel.Weeks.Add(week);
            }
        }
    }
}
