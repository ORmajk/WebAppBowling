using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Transactions;

namespace WebAppBowling.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Номер заказа")]
        public string OrderNumber { get; set; } = string.Empty;

        [Display(Name = "Статус заказа")]
        public int Status { get; set; } = 1; // 1-Новый, 2-Оплачен, 3-В обработке, 4-Завершен, 5-Отменен

        [Display(Name = "Клиент")]
        public int ClientId { get; set; }

        [ForeignKey("ClientId")]
        public virtual User? Client { get; set; }

        [Display(Name = "Способ доставки")]
        public int DeliveryMethod { get; set; } = 1; // 1-На месте, 2-С собой, 3-Доставка

        [Display(Name = "Способ оплаты")]
        public int PaymentMethod { get; set; } = 1; // 1-Наличные, 2-Карта, 3-Онлайн

        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Дата завершения")]
        public DateTime? CompletedAt { get; set; }

        [Display(Name = "Общая стоимость")]
        public decimal TotalAmount { get; set; }

        // Вычисляемое свойство - текст статуса
        [NotMapped]
        public string StatusText => Status switch
        {
            1 => "Новый",
            2 => "Оплачен",
            3 => "В обработке",
            4 => "Завершён",
            5 => "Отменён",
            _ => "Неизвестно"
        };

        // Навигационные свойства
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
