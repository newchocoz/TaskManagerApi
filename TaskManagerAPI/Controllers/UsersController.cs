using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaskManagerAPI.Data;
using TaskManagerAPI.Models;

namespace TaskManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetUsers()
        {
            return Ok(_context.Users.ToList());
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUsers(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }

        [HttpPost]
        [Authorize]
        public IActionResult CreateUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(int id, User updateUser) 
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) 
            {
                return NotFound(new { message = "User not found" });
            }

            if (!string.IsNullOrEmpty(updateUser.Username))
            {
                user.Username = updateUser.Username;
            }
            if (!string.IsNullOrEmpty(updateUser.Email))
            {
                user.Email = updateUser.Email;
            }
            if (!string.IsNullOrEmpty(updateUser.PasswordHash))
            {
                user.PasswordHash = updateUser.PasswordHash;
            }

            await _context.SaveChangesAsync();

            return Ok(user);
        }
        
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUserAsync(int id) 
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return Ok(new { message = $"User {user.Username} deleted successfully" });
        }
        
    }
}
