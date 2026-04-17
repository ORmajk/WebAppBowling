using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppBowling.Models
{
    public class ProductComment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Required(ErrorMessage = "Текст отзыва обязателен")]
        [Display(Name = "Отзыв")]
        public string CommentText { get; set; } = string.Empty;

        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Статус модерации")]
        public int ModerationStatus { get; set; } = 0; // 0-На модерации, 1-Одобрен, 2-Отклонен

        public bool IsApproved => ModerationStatus == 1;
    }
}
