using CalendarManagement.Data;
using CalendarManagement.Models;
using CalendarManagement.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CalendarManagement.Services
{
    public class CalendarEventService : ICalendarEventService
    {
        private readonly ApplicationDbContext _context;

        public CalendarEventService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MonthCalendarViewModel> GetMonthCalendarViewModelAsync(int? year, int? month)
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

            return viewModel;
        }

        public async Task<WeekCalendarViewModel> GetWeekCalendarViewModelAsync(int? year, int? week)
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

            return viewModel;
        }

        public async Task<DayCalendarViewModel> GetDayCalendarViewModelAsync(int? year, int? month, int? day)
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

            return viewModel;
        }

        public async Task<EventDetailsViewModel?> GetEventDetailsAsync(int id)
        {
            var calendarEvent = await _context.CalendarEvents
                .Include(e => e.CreatedBy)
                .Include(e => e.Attendees)
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (calendarEvent == null)
            {
                return null;
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

            return viewModel;
        }

        public async Task<CalendarEvent> CreateEventAsync(CreateEventViewModel input)
        {
            var calendarEvent = new CalendarEvent
            {
                Title = input.Title,
                Description = input.Description,
                StartDateTime = input.StartDateTime,
                EndDateTime = input.EndDateTime,
                Location = input.Location,
                CreatedById = input.CreatedById,
                CreatedAt = DateTime.UtcNow
            };

            _context.CalendarEvents.Add(calendarEvent);
            await _context.SaveChangesAsync();

            // Add attendees
            if (input.SelectedUserIds != null && input.SelectedUserIds.Any())
            {
                foreach (var userId in input.SelectedUserIds)
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

            return calendarEvent;
        }

        public async Task<bool> UpdateEventAsync(int id, CreateEventViewModel input)
        {
            var calendarEvent = await _context.CalendarEvents
                .Include(e => e.Attendees)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (calendarEvent == null)
            {
                return false;
            }

            calendarEvent.Title = input.Title;
            calendarEvent.Description = input.Description;
            calendarEvent.StartDateTime = input.StartDateTime;
            calendarEvent.EndDateTime = input.EndDateTime;
            calendarEvent.Location = input.Location;
            calendarEvent.CreatedById = input.CreatedById;

            // Update attendees
            _context.EventAttendees.RemoveRange(calendarEvent.Attendees);

            if (input.SelectedUserIds != null && input.SelectedUserIds.Any())
            {
                foreach (var userId in input.SelectedUserIds)
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
            return true;
        }

        public async Task<bool> DeleteEventAsync(int id)
        {
            var calendarEvent = await _context.CalendarEvents.FindAsync(id);
            if (calendarEvent == null)
            {
                return false;
            }

            _context.CalendarEvents.Remove(calendarEvent);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CalendarEvent?> GetEventByIdAsync(int id)
        {
            return await _context.CalendarEvents
                .Include(e => e.Attendees)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.OrderBy(u => u.Name).ToListAsync();
        }

        public (bool IsValid, string? ErrorMessage) ValidateEventDates(DateTime startDateTime, DateTime endDateTime)
        {
            if (endDateTime <= startDateTime)
            {
                return (false, "End time must be after start time.");
            }

            return (true, null);
        }
    }
}
