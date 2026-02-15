using CalendarManagement.Models;
using CalendarManagement.Services;
using CalendarManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CalendarManagement.Pages.Calendar
{
    public class CreateEventModel : PageModel
    {
        private readonly ICalendarEventService _calendarEventService;

        public CreateEventModel(ICalendarEventService calendarEventService)
        {
            _calendarEventService = calendarEventService;
        }

        [BindProperty]
        public CreateEventViewModel Input { get; set; } = new CreateEventViewModel();

        public List<User> AllUsers { get; set; } = new List<User>();

        public async Task OnGetAsync(int? year, int? month, int? day, int? hour)
        {
            DateTime startDateTime;

            if (year.HasValue && month.HasValue && day.HasValue)
            {
                // Use provided date
                var date = new DateTime(year.Value, month.Value, day.Value);
                var startHour = hour ?? 12; // Default to 12 PM (noon) if no hour specified
                startDateTime = date.AddHours(startHour);
            }
            else
            {
                // Default to today at 12 PM (noon)
                startDateTime = DateTime.Now.Date.AddHours(12);
            }

            Input.StartDateTime = startDateTime;
            Input.EndDateTime = startDateTime.AddHours(1); // Default 1 hour duration

            AllUsers = await _calendarEventService.GetAllUsersAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                AllUsers = await _calendarEventService.GetAllUsersAsync();
                return Page();
            }

            // Validate end time is after start time
            var validation = _calendarEventService.ValidateEventDates(Input.StartDateTime, Input.EndDateTime);
            if (!validation.IsValid)
            {
                ModelState.AddModelError("Input.EndDateTime", validation.ErrorMessage!);
                AllUsers = await _calendarEventService.GetAllUsersAsync();
                return Page();
            }

            await _calendarEventService.CreateEventAsync(Input);
            return RedirectToPage("Month");
        }
    }
}
