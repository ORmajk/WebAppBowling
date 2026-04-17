using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppBowling.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Название обязательно")]
        [Display(Name = "Название")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Артикул обязателен")]
        [Display(Name = "Артикул")]
        public string Article { get; set; } = string.Empty;

        [Display(Name = "Изображение")]
        public string? ImageUrl { get; set; }

        [Required]
        [Display(Name = "Единица измерения")]
        public string UnitOfMeasure { get; set; } = "шт.";

        [Display(Name = "Старая цена")]
        [Range(0, double.MaxValue, ErrorMessage = "Цена не может быть отрицательной")]
        public decimal? OldPrice { get; set; }

        [Display(Name = "Скидка %")]
        [Range(0, 100, ErrorMessage = "Скидка должна быть от 0 до 100%")]
        public int DiscountPercent { get; set; } = 0;

        [Required(ErrorMessage = "Цена обязательна")]
        [Display(Name = "Цена")]
        [Range(0, double.MaxValue, ErrorMessage = "Цена не может быть отрицательной")]
        public decimal Price { get; set; }

        [Display(Name = "Остаток")]
        [Range(0, int.MaxValue, ErrorMessage = "Остаток не может быть отрицательным")]
        public int StockQuantity { get; set; } = 0;

        [Display(Name = "Статус наличия")]
        public int AvailabilityStatus { get; set; } = 1; // 1-В наличии, 2-Нет, 3-Ожидается

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Display(Name = "Отображать на сайте")]
        public bool IsVisible { get; set; } = true;

        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Поставщик")]
        public int? SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public virtual Supplier? Supplier { get; set; }

        [Display(Name = "Производитель")]
        public int? ManufacturerId { get; set; }

        [ForeignKey("ManufacturerId")]
        public virtual Manufacturer? Manufacturer { get; set; }

        // Вычисляемое свойство - актуальная цена с учетом скидки
        [NotMapped]
        public decimal ActualPrice => DiscountPercent > 0 ? Price * (1 - DiscountPercent / 100m) : Price;

        // Вычисляемое свойство - текст наличия
        [NotMapped]
        public string AvailabilityText => AvailabilityStatus switch
        {
            1 => "В наличии",
            2 => "Нет в наличии",
            3 => "Ожидается",
            _ => "Неизвестно"
        };

        // Навигационные свойства
        public virtual ICollection<ProductTagMapping> TagMappings { get; set; } = new List<ProductTagMapping>();
        public virtual ICollection<ProductComment> Comments { get; set; } = new List<ProductComment>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}
