using System.Threading.Tasks;
using DattingApp.API.Data;
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

        public async Task<IActionResult> Register(string username, string password)
        {
            username = username.ToLower();

            if(await _repo.UserExists(username))
               return BadRequest("Username already exists");

            var userToCreate = new User()
            {
              Username = username              
            };

            var user = await _repo.Register(userToCreate, password);

            return StatusCode(201);                     

        }






    }
}