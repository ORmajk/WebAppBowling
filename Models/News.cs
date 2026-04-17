using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppBowling.Models
{
    public class News
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Заголовок обязателен")]
        [Display(Name = "Заголовок")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Содержание обязательно")]
        [Display(Name = "Содержание")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Изображение")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Дата публикации")]
        public DateTime PublishedAt { get; set; } = DateTime.Now;

        [Display(Name = "В архиве")]
        public bool IsArchived { get; set; } = false;

        [Display(Name = "Автор")]
        public int AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public virtual User? Author { get; set; }
    }
}
