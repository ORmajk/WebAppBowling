using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppBowling.Models
{
    // дорожка (таблица Lanes)
    public class Lane
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Номер дорожки")]
        public int LaneNumber { get; set; }

        public int LaneTypeId { get; set; }
        public int StatusId { get; set; }

        [Display(Name = "Вместимость")]
        public int Capacity { get; set; }

        [Display(Name = "Цена за час")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerHour { get; set; }

        public string? Photo { get; set; }

        public virtual LaneType? LaneType { get; set; }
        public virtual LaneStatus? Status { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
