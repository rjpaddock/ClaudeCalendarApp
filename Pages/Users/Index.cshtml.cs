using CalendarManagement.Models;
using CalendarManagement.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CalendarManagement.Pages.Users
{
    public class IndexModel : PageModel
    {
        private readonly IUserService _userService;

        public IndexModel(IUserService userService)
        {
            _userService = userService;
        }

        public List<User> Users { get; set; } = new List<User>();

        public async Task OnGetAsync()
        {
            Users = await _userService.GetAllUsersAsync();
        }
    }
}
