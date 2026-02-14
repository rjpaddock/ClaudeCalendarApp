using CalendarManagement.Data;
using CalendarManagement.Models;
using CalendarManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CalendarManagement.Pages.Calendar
{
    public class EditEventModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditEventModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CreateEventViewModel Input { get; set; } = new CreateEventViewModel();

        public List<User> AllUsers { get; set; } = new List<User>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var calendarEvent = await _context.CalendarEvents
                .Include(e => e.Attendees)
                .FirstOrDefaultAsync(e => e.Id == id);

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
            Input.AllUsers = await _context.Users.OrderBy(u => u.Name).ToListAsync();
            AllUsers = Input.AllUsers;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                AllUsers = await _context.Users.OrderBy(u => u.Name).ToListAsync();
                Input.AllUsers = AllUsers;
                return Page();
            }

            // Validate end time is after start time
            if (Input.EndDateTime <= Input.StartDateTime)
            {
                ModelState.AddModelError("Input.EndDateTime", "End time must be after start time.");
                AllUsers = await _context.Users.OrderBy(u => u.Name).ToListAsync();
                Input.AllUsers = AllUsers;
                return Page();
            }

            var calendarEvent = await _context.CalendarEvents
                .Include(e => e.Attendees)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (calendarEvent == null)
            {
                return NotFound();
            }

            calendarEvent.Title = Input.Title;
            calendarEvent.Description = Input.Description;
            calendarEvent.StartDateTime = Input.StartDateTime;
            calendarEvent.EndDateTime = Input.EndDateTime;
            calendarEvent.Location = Input.Location;
            calendarEvent.CreatedById = Input.CreatedById;

            // Update attendees
            _context.EventAttendees.RemoveRange(calendarEvent.Attendees);

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
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("EventDetails", new { id = id });
        }
    }
}
