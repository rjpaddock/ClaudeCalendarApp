using CalendarManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace CalendarManagement.ViewModels
{
    public class CreateEventViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Start Date & Time")]
        public DateTime StartDateTime { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "End Date & Time")]
        public DateTime EndDateTime { get; set; } = DateTime.Now.AddHours(1);

        [StringLength(200)]
        public string? Location { get; set; }

        [Required]
        [Display(Name = "Created By")]
        public int CreatedById { get; set; }

        [Display(Name = "Invite Users")]
        public List<int> SelectedUserIds { get; set; } = new List<int>();

        // For dropdown lists
        public List<User> AllUsers { get; set; } = new List<User>();
    }
}
