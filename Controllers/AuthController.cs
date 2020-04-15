using System.Security.Claims;
using System.Threading.Tasks;
using DattingApp.API.Data;
using DattingApp.API.Dtos;
using DattingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using AutoMapper;

namespace DattingApp.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
      
        public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
            _config = config;
        }
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserToRegisterDto userToRegisterDto)
        {

            userToRegisterDto.Username = userToRegisterDto.Username.ToLower();

            if (await _repo.UserExists(userToRegisterDto.Username))
                return BadRequest("Username already exists");

            var userToCreate = new User()
            {
                Username = userToRegisterDto.Username
            };

            var user = await _repo.Register(userToCreate, userToRegisterDto.Password);

            return StatusCode(201);

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserToLoginDto userToLoginDto)
        {
            var userFromRepo = await _repo.Login(userToLoginDto.Username.ToLower(), userToLoginDto.Password);

            if (userFromRepo == null)
                return Unauthorized();

            // build Token for Client

            var claims = new[]
            {
               new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
               new Claim(ClaimTypes.Name, userFromRepo.Username)
           };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var user = _mapper.Map<UserForListDto>(userFromRepo);



            return Ok(new
            {
                token = tokenHandler.WriteToken(token),
                user
            });

        }
    }
}