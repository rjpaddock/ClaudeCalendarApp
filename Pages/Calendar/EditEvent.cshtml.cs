using CalendarManagement.Models;
using CalendarManagement.Services;
using CalendarManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CalendarManagement.Pages.Calendar
{
    public class EditEventModel : PageModel
    {
        private readonly ICalendarEventService _calendarEventService;

        public EditEventModel(ICalendarEventService calendarEventService)
        {
            _calendarEventService = calendarEventService;
        }

        [BindProperty]
        public CreateEventViewModel Input { get; set; } = new CreateEventViewModel();

        public List<User> AllUsers { get; set; } = new List<User>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var calendarEvent = await _calendarEventService.GetEventByIdAsync(id);

            if (calendarEvent == null)
            {
                return NotFound();
            }

            Input = new CreateEventViewModel
            {
                Title = calendarEvent.Title,
                Description = calendarEvent.Description,
                StartDateTime = calendarEvent.StartDateTime,
                EndDateTime = calendarEvent.EndDateTime,
                Location = calendarEvent.Location,
                CreatedById = calendarEvent.CreatedById,
                SelectedUserIds = calendarEvent.Attendees.Select(a => a.UserId).ToList()
            };

            // Store the ID for the form
            ViewData["EventId"] = id;
            Input.AllUsers = await _calendarEventService.GetAllUsersAsync();
            AllUsers = Input.AllUsers;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                AllUsers = await _calendarEventService.GetAllUsersAsync();
                Input.AllUsers = AllUsers;
                return Page();
            }

            // Validate end time is after start time
            var validation = _calendarEventService.ValidateEventDates(Input.StartDateTime, Input.EndDateTime);
            if (!validation.IsValid)
            {
                ModelState.AddModelError("Input.EndDateTime", validation.ErrorMessage!);
                AllUsers = await _calendarEventService.GetAllUsersAsync();
                Input.AllUsers = AllUsers;
                return Page();
            }

            var updated = await _calendarEventService.UpdateEventAsync(id, Input);
            if (!updated)
            {
                return NotFound();
            }

            return RedirectToPage("EventDetails", new { id = id });
        }
    }
}
