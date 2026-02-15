using CalendarManagement.Services;
using CalendarManagement.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CalendarManagement.Pages.Calendar
{
    public class WeekModel : PageModel
    {
        private readonly ICalendarEventService _calendarEventService;

        public WeekModel(ICalendarEventService calendarEventService)
        {
            _calendarEventService = calendarEventService;
        }

        public WeekCalendarViewModel ViewModel { get; set; } = new WeekCalendarViewModel();

        public async Task OnGetAsync(int? year, int? week)
        {
            ViewModel = await _calendarEventService.GetWeekCalendarViewModelAsync(year, week);
        }
    }
}
