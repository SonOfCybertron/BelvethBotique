using System.ComponentModel.DataAnnotations;

namespace BelvethBotique.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string Role { get; set; } = "User";
    }
}
