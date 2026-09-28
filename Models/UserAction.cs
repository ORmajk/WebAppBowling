using System.ComponentModel.DataAnnotations;

namespace WebAppBowling.Models
{
    // журнал действий сотрудников (таблица UserActions)
    public class UserAction
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; }

        public virtual User? User { get; set; }
    }
}
