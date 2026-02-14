using CalendarManagement.Data;
using CalendarManagement.Models;
using CalendarManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CalendarManagement.Controllers
{
    public class CalendarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CalendarController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Calendar/Month or Calendar/Month/2026/2
        public async Task<IActionResult> Month(int? year, int? month)
        {
            var targetDate = year.HasValue && month.HasValue
                ? new DateTime(year.Value, month.Value, 1)
                : new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            var viewModel = new MonthCalendarViewModel
            {
                Year = targetDate.Year,
                Month = targetDate.Month,
                MonthName = targetDate.ToString("MMMM yyyy")
            };

            // Get all events for this month and adjacent days shown in calendar
            var firstDayOfMonth = new DateTime(targetDate.Year, targetDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Start from the first day of week containing the first day of month
            var startDate = firstDayOfMonth.AddDays(-(int)firstDayOfMonth.DayOfWeek);
            // End at the last day of week containing the last day of month
            var endDate = lastDayOfMonth.AddDays(6 - (int)lastDayOfMonth.DayOfWeek);

            var events = await _context.CalendarEvents
                .Where(e => e.StartDateTime >= startDate && e.StartDateTime <= endDate.AddDays(1))
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();

            // Build calendar grid
            var currentDate = startDate;
            while (currentDate <= endDate)
            {
                var week = new List<DayCell>();
                for (int i = 0; i < 7; i++)
                {
                    var dayEvents = events
                        .Where(e => e.StartDateTime.Date == currentDate.Date)
                        .ToList();

                    week.Add(new DayCell
                    {
                        Day = currentDate.Day,
                        Date = currentDate,
                        IsCurrentMonth = currentDate.Month == targetDate.Month,
                        IsToday = currentDate.Date == DateTime.Today,
                        Events = dayEvents
                    });

                    currentDate = currentDate.AddDays(1);
                }
                viewModel.Weeks.Add(week);
            }

            return View(viewModel);
        }

        // GET: Calendar/Week or Calendar/Week/2026/6
        public async Task<IActionResult> Week(int? year, int? week)
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

            var viewModel = new WeekCalendarViewModel
            {
                Year = startOfWeek.Year,
                Week = weekNumber,
                StartDate = startOfWeek,
                EndDate = endOfWeek.AddDays(-1),
                Hours = Enumerable.Range(6, 17).ToList() // 6 AM to 10 PM
            };

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

                viewModel.Days.Add(new DayColumn
                {
                    Date = date,
                    DayName = date.ToString("ddd, MMM d"),
                    IsToday = date.Date == DateTime.Today,
                    Events = dayEvents
                });
            }

            return View(viewModel);
        }

        // GET: Calendar/Day or Calendar/Day/2026/2/14
        public async Task<IActionResult> Day(int? year, int? month, int? day)
        {
            var targetDate = year.HasValue && month.HasValue && day.HasValue
                ? new DateTime(year.Value, month.Value, day.Value)
                : DateTime.Today;

            var viewModel = new DayCalendarViewModel
            {
                Date = targetDate,
                DateFormatted = targetDate.ToString("dddd, MMMM d, yyyy"),
                IsToday = targetDate.Date == DateTime.Today,
                Hours = Enumerable.Range(6, 17).ToList() // 6 AM to 10 PM
            };

            // Get events for this day
            var nextDay = targetDate.AddDays(1);
            viewModel.Events = await _context.CalendarEvents
                .Where(e => e.StartDateTime >= targetDate && e.StartDateTime < nextDay)
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();

            return View(viewModel);
        }

        // GET: Calendar/CreateEvent
        public async Task<IActionResult> CreateEvent(int? year, int? month, int? day, int? hour)
        {
            DateTime startDateTime;

            if (year.HasValue && month.HasValue && day.HasValue)
            {
                // Use provided date
                var date = new DateTime(year.Value, month.Value, day.Value);
                var startHour = hour ?? 9; // Default to 9 AM if no hour specified
                startDateTime = date.AddHours(startHour);
            }
            else
            {
                // Default to today at 9 AM
                startDateTime = DateTime.Now.Date.AddHours(9);
            }

            var viewModel = new CreateEventViewModel
            {
                AllUsers = await _context.Users.OrderBy(u => u.Name).ToListAsync(),
                StartDateTime = startDateTime,
                EndDateTime = startDateTime.AddHours(1) // Default 1 hour duration
            };
            return View(viewModel);
        }

        // POST: Calendar/CreateEvent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(CreateEventViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validate end time is after start time
                if (model.EndDateTime <= model.StartDateTime)
                {
                    ModelState.AddModelError("EndDateTime", "End time must be after start time.");
                    model.AllUsers = await _context.Users.OrderBy(u => u.Name).ToListAsync();
                    return View(model);
                }

                var calendarEvent = new CalendarEvent
                {
                    Title = model.Title,
                    Description = model.Description,
                    StartDateTime = model.StartDateTime,
                    EndDateTime = model.EndDateTime,
                    Location = model.Location,
                    CreatedById = model.CreatedById,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CalendarEvents.Add(calendarEvent);
                await _context.SaveChangesAsync();

                // Add attendees
                if (model.SelectedUserIds != null && model.SelectedUserIds.Any())
                {
                    foreach (var userId in model.SelectedUserIds)
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

                return RedirectToAction(nameof(Month));
            }

            model.AllUsers = await _context.Users.OrderBy(u => u.Name).ToListAsync();
            return View(model);
        }

        // GET: Calendar/EventDetails/5
        public async Task<IActionResult> EventDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var calendarEvent = await _context.CalendarEvents
                .Include(e => e.CreatedBy)
                .Include(e => e.Attendees)
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (calendarEvent == null)
            {
                return NotFound();
            }

            var viewModel = new EventDetailsViewModel
            {
                Event = calendarEvent,
                Attendees = calendarEvent.Attendees.Select(a => new EventAttendeeInfo
                {
                    UserId = a.UserId,
                    UserName = a.User.Name,
                    UserEmail = a.User.Email,
                    ResponseStatus = a.ResponseStatus
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: Calendar/EditEvent/5
        public async Task<IActionResult> EditEvent(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var calendarEvent = await _context.CalendarEvents
                .Include(e => e.Attendees)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (calendarEvent == null)
            {
                return NotFound();
            }

            var viewModel = new CreateEventViewModel
            {
                Title = calendarEvent.Title,
                Description = calendarEvent.Description,
                StartDateTime = calendarEvent.StartDateTime,
                EndDateTime = calendarEvent.EndDateTime,
                Location = calendarEvent.Location,
                CreatedById = calendarEvent.CreatedById,
                SelectedUserIds = calendarEvent.Attendees.Select(a => a.UserId).ToList(),
                AllUsers = await _context.Users.OrderBy(u => u.Name).ToListAsync()
            };

            ViewBag.EventId = id;
            return View("CreateEvent", viewModel);
        }

        // POST: Calendar/EditEvent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(int id, CreateEventViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validate end time is after start time
                if (model.EndDateTime <= model.StartDateTime)
                {
                    ModelState.AddModelError("EndDateTime", "End time must be after start time.");
                    model.AllUsers = await _context.Users.OrderBy(u => u.Name).ToListAsync();
                    ViewBag.EventId = id;
                    return View("CreateEvent", model);
                }

                var calendarEvent = await _context.CalendarEvents
                    .Include(e => e.Attendees)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (calendarEvent == null)
                {
                    return NotFound();
                }

                calendarEvent.Title = model.Title;
                calendarEvent.Description = model.Description;
                calendarEvent.StartDateTime = model.StartDateTime;
                calendarEvent.EndDateTime = model.EndDateTime;
                calendarEvent.Location = model.Location;
                calendarEvent.CreatedById = model.CreatedById;

                // Update attendees
                _context.EventAttendees.RemoveRange(calendarEvent.Attendees);

                if (model.SelectedUserIds != null && model.SelectedUserIds.Any())
                {
                    foreach (var userId in model.SelectedUserIds)
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
                return RedirectToAction(nameof(EventDetails), new { id = id });
            }

            model.AllUsers = await _context.Users.OrderBy(u => u.Name).ToListAsync();
            ViewBag.EventId = id;
            return View("CreateEvent", model);
        }

        // POST: Calendar/DeleteEvent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var calendarEvent = await _context.CalendarEvents.FindAsync(id);
            if (calendarEvent != null)
            {
                _context.CalendarEvents.Remove(calendarEvent);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Month));
        }
    }
}
