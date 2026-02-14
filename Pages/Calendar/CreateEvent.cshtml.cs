using CalendarManagement.Data;
using CalendarManagement.Models;
using CalendarManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CalendarManagement.Pages.Calendar
{
    public class CreateEventModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateEventModel(ApplicationDbContext context)
        {
            _context = context;
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

            AllUsers = await _context.Users.OrderBy(u => u.Name).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                AllUsers = await _context.Users.OrderBy(u => u.Name).ToListAsync();
                return Page();
            }

            // Validate end time is after start time
            if (Input.EndDateTime <= Input.StartDateTime)
            {
                ModelState.AddModelError("Input.EndDateTime", "End time must be after start time.");
                AllUsers = await _context.Users.OrderBy(u => u.Name).ToListAsync();
                return Page();
            }

            var calendarEvent = new CalendarEvent
            {
                Title = Input.Title,
                Description = Input.Description,
                StartDateTime = Input.StartDateTime,
                EndDateTime = Input.EndDateTime,
                Location = Input.Location,
                CreatedById = Input.CreatedById,
                CreatedAt = DateTime.UtcNow
            };

            _context.CalendarEvents.Add(calendarEvent);
            await _context.SaveChangesAsync();

            // Add attendees
            if (Input.SelectedUserIds != null && Input.SelectedUserIds.Any())
            {
                foreach (var userId in Input.SelectedUserIds)
                {
                    var attendee = new EventAttendee
                    {
                        EventId = calendarEvent.Id,
                        UserId = userId,
                        ResponseStatus = ResponseStatus.Pending
                    };
                    _context.EventAttendees.Add(attendee);
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("Month");
        }
    }
}
