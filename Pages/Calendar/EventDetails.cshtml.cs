using CalendarManagement.Data;
using CalendarManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CalendarManagement.Pages.Calendar
{
    public class EventDetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EventDetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public EventDetailsViewModel ViewModel { get; set; } = new EventDetailsViewModel();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var calendarEvent = await _context.CalendarEvents
                .Include(e => e.CreatedBy)
                .Include(e => e.Attendees)
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (calendarEvent == null)
            {
                return NotFound();
            }

            ViewModel.Event = calendarEvent;
            ViewModel.Attendees = calendarEvent.Attendees.Select(a => new EventAttendeeInfo
            {
                UserId = a.UserId,
                UserName = a.User.Name,
                UserEmail = a.User.Email,
                ResponseStatus = a.ResponseStatus
            }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var calendarEvent = await _context.CalendarEvents.FindAsync(id);
            if (calendarEvent != null)
            {
                _context.CalendarEvents.Remove(calendarEvent);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("Month");
        }
    }
}
