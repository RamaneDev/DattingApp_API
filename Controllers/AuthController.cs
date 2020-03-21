using System.Threading.Tasks;
using DattingApp.API.Data;
using DattingApp.API.Dtos;
using DattingApp.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace DattingApp.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        public AuthController(IAuthRepository repo)
        {
            this._repo = repo;

        }
        private IAuthRepository _repo { get; set; }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserToRegisterDto userToRegisterDto)
        {
            userToRegisterDto.Username = userToRegisterDto.Username.ToLower();

            if(await _repo.UserExists(userToRegisterDto.Username))
               return BadRequest("Username already exists");

            var userToCreate = new User()
            {
              Username = userToRegisterDto.Username              
            };

            var user = await _repo.Register(userToCreate, userToRegisterDto.Password);

            return StatusCode(201);                     

        }






    }
}