using System.ComponentModel.DataAnnotations;

namespace DattingApp.API.Dtos
{
    public class UserToRegisterDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength(8, MinimumLength = 4, ErrorMessage = "You must specify password between 4 and 8 caracteres")]
        public string Password { get; set; }
    }
}