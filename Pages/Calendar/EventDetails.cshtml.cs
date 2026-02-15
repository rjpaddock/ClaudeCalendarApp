using CalendarManagement.Services;
using CalendarManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CalendarManagement.Pages.Calendar
{
    public class EventDetailsModel : PageModel
    {
        private readonly ICalendarEventService _calendarEventService;

        public EventDetailsModel(ICalendarEventService calendarEventService)
        {
            _calendarEventService = calendarEventService;
        }

        public EventDetailsViewModel ViewModel { get; set; } = new EventDetailsViewModel();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var viewModel = await _calendarEventService.GetEventDetailsAsync(id);

            if (viewModel == null)
            {
                return NotFound();
            }

            ViewModel = viewModel;
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _calendarEventService.DeleteEventAsync(id);
            return RedirectToPage("Month");
        }
    }
}
