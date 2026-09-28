using System.ComponentModel.DataAnnotations;

namespace WebAppBowling.Models
{
    // данные формы регистрации клиента
    public class RegisterForm
    {
        [Required(ErrorMessage = "Введите ФИО")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "ФИО от 3 до 200 символов")]
        [Display(Name = "ФИО")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите телефон")]
        [RegularExpression(@"^[\d\+\-\(\)\s]{10,20}$", ErrorMessage = "Телефон введён неправильно")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email введён неправильно")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Придумайте логин")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Логин от 3 до 50 символов")]
        [Display(Name = "Логин")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Придумайте пароль")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль не короче 6 символов")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        [Display(Name = "Повторите пароль")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
