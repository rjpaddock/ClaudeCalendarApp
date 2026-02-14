using CalendarManagement.Data;
using CalendarManagement.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CalendarManagement.Pages.Calendar
{
    public class DayModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DayModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public DayCalendarViewModel ViewModel { get; set; } = new DayCalendarViewModel();

        public async Task OnGetAsync(int? year, int? month, int? day)
        {
            var targetDate = year.HasValue && month.HasValue && day.HasValue
                ? new DateTime(year.Value, month.Value, day.Value)
                : DateTime.Today;

            ViewModel.Date = targetDate;
            ViewModel.DateFormatted = targetDate.ToString("dddd, MMMM d, yyyy");
            ViewModel.IsToday = targetDate.Date == DateTime.Today;
            ViewModel.Hours = Enumerable.Range(6, 17).ToList(); // 6 AM to 10 PM

            // Get events for this day
            var nextDay = targetDate.AddDays(1);
            ViewModel.Events = await _context.CalendarEvents
                .Where(e => e.StartDateTime >= targetDate && e.StartDateTime < nextDay)
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();
        }
    }
}
