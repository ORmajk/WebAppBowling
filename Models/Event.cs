using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppBowling.Models
{
    // мероприятие клуба (таблица Events)
    public class Event
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "date")]
        public DateTime EventDate { get; set; }

        public string? Description { get; set; }
        public int? LaneId { get; set; }
    }
}
