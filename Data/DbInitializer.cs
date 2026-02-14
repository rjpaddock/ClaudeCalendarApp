using CalendarManagement.Models;

namespace CalendarManagement.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Ensure the database is created
            context.Database.EnsureCreated();

            // No seed data - database starts empty
        }
    }
}
