using _VebtechApplication.Data;
using _VebtechApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VebtechTestApplication.Data;
using VebtechTestApplication.Models;

namespace VebtechApplication.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(int page = 1, int pageSize = 10, string sortField = "Id", string sortOrder = "ascending", string filter = "")
        {
            var query = _context.Users
                .Include(u => u.Roles)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(u => u.Name!.Contains(filter) || u.Email!.Contains(filter));
            }

            query = sortOrder.ToLower() == "ascending"
                ? query.OrderBy(user => EF.Property<object>(user, sortField))
                : query.OrderByDescending(user => EF.Property<object>(user, sortField));

            var users = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromForm] string name, [FromForm] int age, [FromForm] string email)
        {
            var user = new User
            {
                Name = name,
                Age = age,
                Email = email
            };

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromForm] string name, [FromForm] int age, [FromForm] string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            user.Name = name;
            user.Age = age;
            user.Email = email;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (age < 0)
            {
                return BadRequest("Возраст должен быть положительным числом");
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            foreach (var role in user.Roles!.ToList())
            {
                _context.Roles.Remove(role);
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{userId}/roles/{roleId}")]
        public async Task<IActionResult> AddRoleToUser(int userId, int roleId, string email)
        {
            var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            if (_context.Users.Any(u => u.Email == email))
            {
                return BadRequest("Пользователь с таким email уже существует");
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
            if (role == null)
            {
                return NotFound("Роль не найдена");
            }

            if (user.Roles!.Any(r => r.RoleId == roleId))
            {
                return BadRequest("Пользователь уже имеет эту роль");
            }

            user.Roles!.Add(role);
            await _context.SaveChangesAsync();

            return Ok("Роль успешно присвоена пользователю");
        }

        [HttpDelete("{userId}/roles/{roleId}")]
        public async Task<IActionResult> RemoveRoleFromUser(int userId, int roleId)
        {
            var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
            if (role == null)
            {
                return NotFound("Роль не найдена");
            }

            var userRole = user.Roles!.FirstOrDefault(r => r.RoleId == roleId);
            if (userRole == null)
            {
                return BadRequest("Пользователь не имеет эту роль");
            }

            user.Roles!.Remove(userRole);
            await _context.SaveChangesAsync();

            return Ok("Роль успешно удалена у пользователя");
        }
    }
}
