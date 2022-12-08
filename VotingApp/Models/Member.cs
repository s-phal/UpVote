using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace VotingApp.Models
{
    public class Member : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? DisplayName { get; set; }
        public string? AvatarFileName { get; set; } = "default.png";
        [NotMapped]
        public IFormFile? AvatarImageFile { get; set; }
    }
}
