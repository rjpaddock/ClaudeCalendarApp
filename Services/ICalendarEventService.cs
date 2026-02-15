using CalendarManagement.Models;
using CalendarManagement.ViewModels;

namespace CalendarManagement.Services
{
    public interface ICalendarEventService
    {
        Task<MonthCalendarViewModel> GetMonthCalendarViewModelAsync(int? year, int? month);
        Task<WeekCalendarViewModel> GetWeekCalendarViewModelAsync(int? year, int? week);
        Task<DayCalendarViewModel> GetDayCalendarViewModelAsync(int? year, int? month, int? day);
        Task<EventDetailsViewModel?> GetEventDetailsAsync(int id);
        Task<CalendarEvent> CreateEventAsync(CreateEventViewModel input);
        Task<bool> UpdateEventAsync(int id, CreateEventViewModel input);
        Task<bool> DeleteEventAsync(int id);
        Task<CalendarEvent?> GetEventByIdAsync(int id);
        Task<List<User>> GetAllUsersAsync();
        (bool IsValid, string? ErrorMessage) ValidateEventDates(DateTime startDateTime, DateTime endDateTime);
    }
}
