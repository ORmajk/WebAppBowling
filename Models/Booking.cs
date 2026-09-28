using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppBowling.Models
{
    // бронирование дорожки (таблица Bookings)
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Номер брони")]
        public string BookingNumber { get; set; } = string.Empty;

        public int ClientId { get; set; }
        public int LaneId { get; set; }

        [Display(Name = "Начало игры")]
        public DateTime StartTime { get; set; }

        [Display(Name = "Длительность, мин")]
        public int DurationMinutes { get; set; }

        [Display(Name = "Сумма")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Способ оплаты")]
        public string PaymentMethod { get; set; } = string.Empty;

        [Display(Name = "Статус")]
        public string Status { get; set; } = string.Empty;

        public virtual Client? Client { get; set; }
        public virtual Lane? Lane { get; set; }

        // время окончания игры (в базе не хранится)
        [NotMapped]
        public DateTime EndTime => StartTime.AddMinutes(DurationMinutes);
    }
}
