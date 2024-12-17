using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class ApplicationUser : IdentityUser
    { 
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Address { get; set; }

        public byte[] ProfilePicture { get; set; }
       

    }
}
