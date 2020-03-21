using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DattingApp.API.Dtos
{
    public class UserToLoginDto
    {
        [Required]
        public string Username { get; set; }

         [Required]
         [StringLength(8, MinimumLength = 4, ErrorMessage = "You must specify password between 4 and 8 caracteres")]
        public string Password { get; set; }
    }
}
