using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace WebAppBowling.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Фамилия обязательна")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Имя обязательно")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Отчество")]
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Телефон обязателен")]
        [Phone(ErrorMessage = "Некорректный формат телефона")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Дата регистрации")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Роль")]
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; }

        [Display(Name = "Статус")]
        public int Status { get; set; } = 1; // 1 - Активен, 0 - Заблокирован

        [Range(0, 100, ErrorMessage = "Скидка должна быть от 0 до 100%")]
        [Display(Name = "Персональная скидка")]
        public int Discount { get; set; } = 0;

        // Навигационные свойства
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<ProductComment> Comments { get; set; } = new List<ProductComment>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}
