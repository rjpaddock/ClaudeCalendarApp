using CalendarManagement.Models;
using CalendarManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CalendarManagement.Pages.Users
{
    public class CreateModel : PageModel
    {
        private readonly IUserService _userService;

        public CreateModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public User User { get; set; } = new User();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if email already exists
            if (await _userService.EmailExistsAsync(User.Email))
            {
                ModelState.AddModelError("User.Email", "A user with this email already exists.");
                return Page();
            }

            await _userService.CreateUserAsync(User);
            return RedirectToPage("Index");
        }
    }
}
