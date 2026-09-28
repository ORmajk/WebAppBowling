using System.ComponentModel.DataAnnotations;

namespace WebAppBowling.Models
{
    // состояние дорожки: Доступна, Занята, Требует обслуживания (таблица LaneStatuses)
    public class LaneStatus
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Color { get; set; }
    }
}
