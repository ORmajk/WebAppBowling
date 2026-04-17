using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppBowling.Models
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Имя обязательно")]
        [Display(Name = "Имя")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Телефон обязателен")]
        [Phone(ErrorMessage = "Некорректный формат телефона")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Сообщение обязательно")]
        [Display(Name = "Сообщение")]
        public string Message { get; set; } = string.Empty;

        [Display(Name = "Дата отправки")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Статус")]
        public int Status { get; set; } = 1; // 1-Новое, 2-В обработке, 3-Закрыто

        [NotMapped]
        public string StatusText => Status switch
        {
            1 => "Новое",
            2 => "В обработке",
            3 => "Закрыто",
            _ => "Неизвестно"
        };
    }
}
