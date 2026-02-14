using CalendarManagement.Data;
using CalendarManagement.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CalendarManagement.Pages.Calendar
{
    public class WeekModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public WeekModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public WeekCalendarViewModel ViewModel { get; set; } = new WeekCalendarViewModel();

        public async Task OnGetAsync(int? year, int? week)
        {
            var targetDate = DateTime.Today;
            if (year.HasValue && week.HasValue)
            {
                // Calculate date from ISO week number
                var jan1 = new DateTime(year.Value, 1, 1);
                var daysOffset = DayOfWeek.Monday - jan1.DayOfWeek;
                var firstMonday = jan1.AddDays(daysOffset);
                targetDate = firstMonday.AddDays((week.Value - 1) * 7);
            }

            // Get start of week (Sunday)
            var startOfWeek = targetDate.AddDays(-(int)targetDate.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            var culture = CultureInfo.CurrentCulture;
            var weekNumber = culture.Calendar.GetWeekOfYear(startOfWeek, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);

            ViewModel.Year = startOfWeek.Year;
            ViewModel.Week = weekNumber;
            ViewModel.StartDate = startOfWeek;
            ViewModel.EndDate = endOfWeek.AddDays(-1);
            ViewModel.Hours = Enumerable.Range(6, 17).ToList(); // 6 AM to 10 PM

            // Get events for the week
            var events = await _context.CalendarEvents
                .Where(e => e.StartDateTime >= startOfWeek && e.StartDateTime < endOfWeek)
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();

            // Build day columns
            for (int i = 0; i < 7; i++)
            {
                var date = startOfWeek.AddDays(i);
                var dayEvents = events.Where(e => e.StartDateTime.Date == date.Date).ToList();

                ViewModel.Days.Add(new DayColumn
                {
                    Date = date,
                    DayName = date.ToString("ddd, MMM d"),
                    IsToday = date.Date == DateTime.Today,
                    Events = dayEvents
                });
            }
        }
    }
}
