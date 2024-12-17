using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModel
{
    public class ProfileViewModel
    {
        [Required, StringLength(100, MinimumLength = 8)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        public string Address { get; set; }

        public List<RoleVM> Roles { get; set; }
        public string? UserName { get; set; }
    }
}
