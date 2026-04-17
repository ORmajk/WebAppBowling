using System.ComponentModel.DataAnnotations;

namespace WebAppBowling.Models
{
    public class Promotion
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Название акции обязательно")]
        [Display(Name = "Название")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Описание обязательно")]
        [Display(Name = "Описание")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Изображение")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Дата начала")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Дата окончания")]
        public DateTime EndDate { get; set; }

        [Display(Name = "В архиве")]
        public bool IsArchived { get; set; } = false;

        [Display(Name = "Скидка %")]
        [Range(0, 100)]
        public int? DiscountPercent { get; set; }
    }
}
