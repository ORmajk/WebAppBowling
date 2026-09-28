using Microsoft.EntityFrameworkCore;
using WebAppBowling.Models;

namespace WebAppBowling.Data
{
    // контекст базы данных BowlingClub (таблицы уже созданы SQL-скриптами)
    public class BowlingContext : DbContext
    {
        public BowlingContext(DbContextOptions<BowlingContext> options) : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserAction> UserActions { get; set; }
        public DbSet<Lane> Lanes { get; set; }
        public DbSet<LaneType> LaneTypes { get; set; }
        public DbSet<LaneStatus> LaneStatuses { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
    }
}
