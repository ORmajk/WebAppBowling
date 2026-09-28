using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppBowling.Models
{
    // товар бара / магазина (таблица Products)
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название")]
        [StringLength(100, ErrorMessage = "Название не длиннее 100 символов")]
        [Display(Name = "Название")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Выберите категорию")]
        [Display(Name = "Категория")]
        public string Category { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Range(1, 1000000, ErrorMessage = "Цена должна быть больше 0")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Цена")]
        public decimal Price { get; set; }

        [Range(0, 100000, ErrorMessage = "Количество не может быть отрицательным")]
        [Display(Name = "Остаток")]
        public int Quantity { get; set; }

        [Display(Name = "Ссылка на картинку")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Показывать на сайте")]
        public bool IsVisible { get; set; } = true;
    }
}
