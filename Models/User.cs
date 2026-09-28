using System.ComponentModel.DataAnnotations;

namespace WebAppBowling.Models
{
    // сотрудник клуба (таблица Users): Admin, Manager, Operator
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int Points { get; set; }
        public bool IsSubscribedToAI { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}
