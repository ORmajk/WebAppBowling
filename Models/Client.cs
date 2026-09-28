using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppBowling.Models
{
    // клиент клуба (таблица Clients)
    public class Client
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите ФИО")]
        [Display(Name = "ФИО")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Телефон")]
        public string? Phone { get; set; }

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Column(TypeName = "date")]
        public DateTime RegistrationDate { get; set; }

        public int Points { get; set; }

        public bool IsSubscribedToAl { get; set; }

        public string? Login { get; set; }

        public string? Password { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
