using System.ComponentModel.DataAnnotations;

namespace WebAppBowling.Models
{
    // данные формы бронирования
    public class BookingForm
    {
        [Required(ErrorMessage = "Выберите дорожку")]
        [Display(Name = "Дорожка")]
        public int? LaneId { get; set; }

        [Required(ErrorMessage = "Выберите дату")]
        [DataType(DataType.Date)]
        [Display(Name = "Дата")]
        public DateTime? Date { get; set; }

        [Required(ErrorMessage = "Выберите время")]
        [Display(Name = "Время начала")]
        public string Time { get; set; } = "18:00";

        [Display(Name = "Длительность")]
        public int DurationMinutes { get; set; } = 60;

        [Display(Name = "Способ оплаты")]
        public string PaymentMethod { get; set; } = "Наличные";
    }
}
