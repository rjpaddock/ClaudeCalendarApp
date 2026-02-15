using CalendarManagement.Services;
using CalendarManagement.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CalendarManagement.Pages.Calendar
{
    public class DayModel : PageModel
    {
        private readonly ICalendarEventService _calendarEventService;

        public DayModel(ICalendarEventService calendarEventService)
        {
            _calendarEventService = calendarEventService;
        }

        public DayCalendarViewModel ViewModel { get; set; } = new DayCalendarViewModel();

        public async Task OnGetAsync(int? year, int? month, int? day)
        {
            ViewModel = await _calendarEventService.GetDayCalendarViewModelAsync(year, month, day);
        }
    }
}
