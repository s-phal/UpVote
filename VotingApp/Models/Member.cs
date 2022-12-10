using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace VotingApp.Models
{
    public class Member : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? DisplayName { get; set; } = GenerateDisplayName();

        public string? AvatarFileName { get; set; } = "default.png";
        [NotMapped]
        public IFormFile? AvatarImageFile { get; set; }

        public virtual ICollection<Idea> Ideas { get; set; } = new HashSet<Idea>();



        private static string GenerateDisplayName()
        {
            Random random= new Random();

            string DisplayName = "User" + random.Next(1000,9999).ToString();

            return DisplayName;
        }
    }

}

