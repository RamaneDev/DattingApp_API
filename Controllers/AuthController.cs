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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace DattingApp.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        public AuthController(IConfiguration config, IMapper mapper,
        UserManager<User> useManager, SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
            _useManager = useManager;
            _mapper = mapper;  
            _config = config;
        }

        private readonly UserManager<User> _useManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserToRegisterDto userToRegisterDto)
        {           

            var userToCreate = _mapper.Map<User>(userToRegisterDto);

            var result = await _useManager.CreateAsync(userToCreate, userToRegisterDto.Password);          

            var userToReturn = _mapper.Map<UserForDetailedDto>(userToCreate);

            if (result.Succeeded)
            {
               return CreatedAtRoute("GetUser", new { controller = "Users", id = userToCreate.Id }, userToReturn);
            }

            return BadRequest(result.Errors);
        
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserToLoginDto userToLoginDto)
        {
            var user = await _useManager.FindByNameAsync(userToLoginDto.Username);

            var result = await _signInManager.CheckPasswordSignInAsync(user, userToLoginDto.Password, false);        
            
            
            if (result.Succeeded)
            {
                var appUser = _mapper.Map<UserForListDto>(user);

                return Ok(new
                {
                    token = GenerateJwtToken(user).Result,
                    user = appUser
                });

            }
                
            return Unauthorized();           

        }


        private async Task<string> GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
               new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
               new Claim(ClaimTypes.Name, user.UserName)
           };

            var roles = await _useManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

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

            return tokenHandler.WriteToken(token);
        }
    }
}