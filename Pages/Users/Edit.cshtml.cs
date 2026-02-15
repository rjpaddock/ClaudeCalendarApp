using CalendarManagement.Models;
using CalendarManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CalendarManagement.Pages.Users
{
    public class EditModel : PageModel
    {
        private readonly IUserService _userService;

        public EditModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public User User { get; set; } = new User();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            User = user;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if email already exists for another user
            if (await _userService.EmailExistsAsync(User.Email, User.Id))
            {
                ModelState.AddModelError("User.Email", "A user with this email already exists.");
                return Page();
            }

            var updated = await _userService.UpdateUserAsync(User);
            if (!updated)
            {
                return NotFound();
            }

            return RedirectToPage("Index");
        }
    }
}
