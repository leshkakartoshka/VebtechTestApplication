using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace VebtechTestApplication.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Имя обязательно")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Неправильный формат Email")]
        public string? Email { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Возраст должен быть положительным числом")]
        public int Age { get; set; }
        public List<Role>? Roles { get; set; }
    }
}
