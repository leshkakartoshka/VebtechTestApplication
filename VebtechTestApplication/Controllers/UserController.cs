using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VebtechTestApplication.Data;
using VebtechTestApplication.Models;

namespace VebtechTestApplication.Controllers
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

        // Получаем список пользователей для сортировки и пагинации
        [HttpGet]
        public IActionResult GetUsers(int page = 1, int pageSize = 10, string sortField = "Id", string sortOrder = "ascending", string filter = "")
        {
            var query = _context.Users.AsQueryable();

            // Примените фильтрацию, сортировку и пагинацию на основе входных параметров.
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(u => u.Name!.Contains(filter) || u.Email!.Contains(filter));
            }

            // Сортировка.
            query = sortOrder.ToLower() == "ascending"
                ? query.OrderBy(user => EF.Property<object>(user, sortField))
                : query.OrderByDescending(user => EF.Property<object>(user, sortField));

            // Пагинация.
            var users = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(users);
        }

        // Поиск пользователя по ID
        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            var user = _context.Users.Include(u => u.Roles).FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // Создание нового пользователя
        [HttpPost]
        public IActionResult CreateUser([FromForm] string name, [FromForm] int age, [FromForm] string email)
        {
            // Создание нового пользователя без явного указания Id
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
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // Обновление данных существуещего пользователя по ID
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromForm] string name, [FromForm] int age, [FromForm] string email)
        {
            // Получите существующего пользователя по Id
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            // Обновление свойств пользователя на основе параметров запроса
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

            _context.SaveChanges();

            return NoContent();
        }

        // Удаление данных существуещего пользователя по ID
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var roles = _context.Users.Include(u => u.Roles).FirstOrDefault(u => u.Id == id);

            if (roles == null)
            {
                return NotFound("Пользователь не найден");
            }


            // Удалите записи ролей.
            foreach (var role in roles!.Roles!.ToList())
            {
                _context.Roles.Remove(role);
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            return NoContent();
        }

        // Присвоение роли существуещему пользователю
        [HttpPost("{userId}/roles/{roleId}")]
        public IActionResult AddRoleToUser(int userId, int roleId)
        {
            var user = _context.Users.Include(u => u.Roles).FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            var role = _context.Roles.FirstOrDefault(r => r.RoleId == roleId);
            if (role == null)
            {
                return NotFound("Роль не найдена");
            }

            // Проверьте, не имеет ли пользователь уже эту роль.
            if (user.Roles!.Any(r => r.RoleId == roleId))
            {
                return BadRequest("Пользователь уже имеет эту роль");
            }

            user.Roles!.Add(role);
            _context.SaveChanges();

            return Ok("Роль успешно присвоена пользователю");
        }


        // Удаление роли существуещему пользователю
        [HttpDelete("{userId}/roles/{roleId}")]
        public IActionResult RemoveRoleFromUser(int userId, int roleId)
        {
            var user = _context.Users.Include(u => u.Roles).FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            var role = _context.Roles.FirstOrDefault(r => r.RoleId == roleId);
            if (role == null)
            {
                return NotFound("Роль не найдена");
            }

            // Проверьте, имеет ли пользователь эту роль, прежде чем ее удалить.
            var userRole = user.Roles!.FirstOrDefault(r => r.RoleId == roleId);
            if (userRole == null)
            {
                return BadRequest("Пользователь не имеет эту роль");
            }

            user.Roles!.Remove(userRole);
            _context.SaveChanges();

            return Ok("Роль успешно удалена у пользователя");
        }

    }
}
