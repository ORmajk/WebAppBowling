using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Emit;
using System.Transactions;
using WebAppBowling.Models;

namespace WebAppBowling.Data
{
    public class BowlingClubContext : DbContext
    {
        public BowlingClubContext(DbContextOptions<BowlingClubContext> options)
            : base(options)
        {
        }

        // Таблицы базы данных
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<ProductTag> ProductTags { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductTagMapping> ProductTagMappings { get; set; }
        public DbSet<ProductComment> ProductComments { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Lane> Lanes { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<WorkShift> WorkShifts { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ======================================================
            // НАСТРОЙКА ТАБЛИЦ И СВЯЗЕЙ
            // ======================================================

            // Таблица Roles
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Таблица Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MiddleName).HasMaxLength(100);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Status).HasDefaultValue(1);
                entity.Property(e => e.Discount).HasDefaultValue(0);

                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Phone).IsUnique();

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Таблица Suppliers
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.ToTable("Suppliers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ContactInfo).HasMaxLength(500);
            });

            // Таблица Manufacturers
            modelBuilder.Entity<Manufacturer>(entity =>
            {
                entity.ToTable("Manufacturers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Country).HasMaxLength(100);
            });

            // Таблица ProductTags
            modelBuilder.Entity<ProductTag>(entity =>
            {
                entity.ToTable("ProductTags");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Таблица Products
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Article).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.UnitOfMeasure).IsRequired().HasMaxLength(20).HasDefaultValue("шт.");
                entity.Property(e => e.OldPrice).HasColumnType("decimal(10,2)");
                entity.Property(e => e.DiscountPercent).HasDefaultValue(0);
                entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(10,2)");
                entity.Property(e => e.StockQuantity).HasDefaultValue(0);
                entity.Property(e => e.AvailabilityStatus).HasDefaultValue(1);
                entity.Property(e => e.IsVisible).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.Article).IsUnique();
                entity.HasIndex(e => new { e.AvailabilityStatus, e.IsVisible });

                entity.HasOne(e => e.Supplier)
                    .WithMany()
                    .HasForeignKey(e => e.SupplierId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Manufacturer)
                    .WithMany()
                    .HasForeignKey(e => e.ManufacturerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Таблица ProductTagMapping (связь многие-ко-многим)
            modelBuilder.Entity<ProductTagMapping>(entity =>
            {
                entity.ToTable("ProductTagMapping");
                entity.HasKey(e => new { e.ProductId, e.TagId });

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.TagMappings)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Tag)
                    .WithMany(t => t.ProductMappings)
                    .HasForeignKey(e => e.TagId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Таблица ProductComments
            modelBuilder.Entity<ProductComment>(entity =>
            {
                entity.ToTable("ProductComments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CommentText).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.ModerationStatus).HasDefaultValue(0);

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Таблица Orders
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).HasDefaultValue(1);
                entity.Property(e => e.DeliveryMethod).HasDefaultValue(1);
                entity.Property(e => e.PaymentMethod).HasDefaultValue(1);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(10,2)").HasDefaultValue(0);

                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.HasIndex(e => e.ClientId);
                entity.HasIndex(e => new { e.Status, e.CreatedAt });

                entity.HasOne(e => e.Client)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Таблица OrderItems
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.UnitPrice).IsRequired().HasColumnType("decimal(10,2)");

                entity.HasIndex(e => e.ProductId);

                entity.HasOne(e => e.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Таблица Lanes
            modelBuilder.Entity<Lane>(entity =>
            {
                entity.ToTable("Lanes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LaneNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Status).HasDefaultValue(1);
                entity.Property(e => e.Capacity).HasDefaultValue(6);
                entity.Property(e => e.HourlyRate).IsRequired().HasColumnType("decimal(10,2)");

                entity.HasIndex(e => e.LaneNumber).IsUnique();
            });

            // Таблица Bookings
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.ToTable("Bookings");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StartTime).IsRequired();
                entity.Property(e => e.EndTime).IsRequired();
                entity.Property(e => e.PlayersCount).HasDefaultValue(1);
                entity.Property(e => e.Comment).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Status).HasDefaultValue(1);

                entity.HasIndex(e => new { e.LaneId, e.StartTime, e.EndTime });

                entity.HasOne(e => e.Lane)
                    .WithMany()
                    .HasForeignKey(e => e.LaneId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Client)
                    .WithMany(u => u.Bookings)
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Таблица Transactions
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transactions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).IsRequired().HasColumnType("decimal(10,2)");
                entity.Property(e => e.PaymentMethod).IsRequired();
                entity.Property(e => e.Status).HasDefaultValue(1);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Order)
                    .WithMany(o => o.Transactions)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Таблица Positions
            modelBuilder.Entity<Position>(entity =>
            {
                entity.ToTable("Positions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.BaseSalary).IsRequired().HasColumnType("decimal(10,2)");

                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Таблица Employees
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employees");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MiddleName).HasMaxLength(100);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Login).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.HireDate).IsRequired();

                entity.HasIndex(e => e.Phone).IsUnique();
                entity.HasIndex(e => e.Login).IsUnique();

                entity.HasOne(e => e.Position)
                    .WithMany()
                    .HasForeignKey(e => e.PositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Role)
                    .WithMany()
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Таблица WorkShifts
            modelBuilder.Entity<WorkShift>(entity =>
            {
                entity.ToTable("WorkShifts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StartTime).IsRequired();
                entity.Property(e => e.EndTime).IsRequired();

                entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Таблица Favorites (Избранное)
            modelBuilder.Entity<Favorite>(entity =>
            {
                entity.ToTable("Favorites");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AddedAt).HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Favorites)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.Favorites)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Таблица News (Новости)
            modelBuilder.Entity<News>(entity =>
            {
                entity.ToTable("News");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.PublishedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsArchived).HasDefaultValue(false);

                entity.HasOne(e => e.Author)
                    .WithMany()
                    .HasForeignKey(e => e.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Таблица Promotions (Акции)
            modelBuilder.Entity<Promotion>(entity =>
            {
                entity.ToTable("Promotions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();
                entity.Property(e => e.IsArchived).HasDefaultValue(false);
                entity.Property(e => e.DiscountPercent).HasDefaultValue(null);
            });

            // Таблица Feedback (Обратная связь)
            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.ToTable("Feedbacks");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Status).HasDefaultValue(1);
            });
        }

        // ======================================================
        // ПЕРЕОПРЕДЕЛЕНИЕ SAVECHANGES ДЛЯ АВТОМАТИЧЕСКОГО ЗАПОЛНЕНИЯ ДАТ
        // ======================================================
        public override int SaveChanges()
        {
            SetDefaultValues();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetDefaultValues();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SetDefaultValues()
        {
            foreach (var entry in ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added))
            {
                // Установка даты создания для сущностей с полем CreatedAt
                if (entry.Entity.GetType().GetProperty("CreatedAt") != null)
                {
                    var createdAtProp = entry.Property("CreatedAt");
                    if (createdAtProp.CurrentValue == null ||
                        (DateTime)createdAtProp.CurrentValue == default)
                    {
                        createdAtProp.CurrentValue = DateTime.Now;
                    }
                }

                // Установка даты публикации для новостей
                if (entry.Entity is News news && news.PublishedAt == default)
                {
                    news.PublishedAt = DateTime.Now;
                }
            }
        }
    }
}
