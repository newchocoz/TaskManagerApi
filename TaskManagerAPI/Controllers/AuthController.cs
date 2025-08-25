using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManagerAPI.Data;
using TaskManagerAPI.Dtos;
using TaskManagerAPI.Models;
using TaskManagerAPI.Utils;

namespace TaskManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto req)
        {
            if (await _context.Users.AnyAsync(u => u.Username == req.Username)) 
            {
                return BadRequest(new { message = "Username already exists" });
            } 
            if(await _context.Users.AnyAsync(u => u.Email == req.Email))
            {
                return BadRequest(new { message = "Email already registered" });
            }

            var hashedPassword = PasswordHasher.HashPassword(req.Password);

            var user = new User
            {
                Email = req.Email,
                Username = req.Username,
                PasswordHash = hashedPassword,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new {message = $"User {req.Username} registered"});
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto req)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
            if (user == null) { return BadRequest(new { message = "User not found" }); }

            if(!PasswordHasher.VerifyPassword(req.Password, user.PasswordHash))
            {
                return BadRequest(new { message = "Wrong Password" });
            }

            var token = GenerateJwtToken(user);

            return Ok(new { message = "Login Success", token });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("userId", user.Id.ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
