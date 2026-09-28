using System.ComponentModel.DataAnnotations;

namespace WebAppBowling.Models
{
    // категория дорожки: Стандартная, VIP, Детская (таблица LaneTypes)
    public class LaneType
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public virtual ICollection<Lane> Lanes { get; set; } = new List<Lane>();
    }
}
