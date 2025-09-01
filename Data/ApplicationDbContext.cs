using SunTech.Models;
using Microsoft.EntityFrameworkCore;
using static SunTech.Models.DatabaseModel;
namespace SunTech.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Inflow> Inflows { get; set; }
        public DbSet<Storage> Storages { get; set; }
        public DbSet<Dispatch> Dispatches { get; set; }
        public DbSet<Waste> Wastes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure Product entity
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.Supplier).HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ExpiryDate).HasColumnType("timestamp without time zone");
            });

            // Configure Report entity
            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasKey(e => e.ReportId);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Date).HasColumnType("timestamp without time zone");
                entity.HasOne(r => r.User)
                      .WithMany(u => u.Reports)
                      .HasForeignKey(r => r.GeneratedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Inflow entity
            modelBuilder.Entity<Inflow>(entity =>
            {
                entity.HasKey(e => e.InflowId);
                entity.Property(e => e.DateReceived).HasColumnType("timestamp without time zone");
                entity.HasOne(i => i.Product)
                      .WithMany(p => p.Inflows)
                      .HasForeignKey(i => i.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Storage entity
            modelBuilder.Entity<Storage>(entity =>
            {
                entity.HasKey(e => e.StorageId);
                entity.Property(e => e.Location).IsRequired();
                entity.HasOne(s => s.Product)
                      .WithMany(p => p.Storages)
                      .HasForeignKey(s => s.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Dispatch entity
            modelBuilder.Entity<Dispatch>(entity =>
            {
                entity.HasKey(e => e.DispatchId);
                entity.Property(e => e.DateDispatched).HasColumnType("timestamp without time zone");
                entity.HasOne(d => d.Product)
                      .WithMany(p => p.Dispatches)
                      .HasForeignKey(d => d.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Waste entity
            modelBuilder.Entity<Waste>(entity =>
            {
                entity.HasKey(e => e.WasteId);
                entity.Property(e => e.Reason).IsRequired();
                entity.HasOne(w => w.Product)
                      .WithMany(p => p.Wastes)
                      .HasForeignKey(w => w.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed default admin user
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Name = "System Administrator",
                    Email = "admin@suntech.com",
                    Role = "Admin",
                    PasswordHash = HashPassword("Admin@123") // Default password: Admin@123
                }
            );

            // Seed sample products
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    ProductId = 1,
                    Name = "Solar Panel 300W",
                    Category = "Solar Equipment",
                    Quantity = 50,
                    Supplier = "SolarTech Industries",
                    Price = 299.99m,
                    ExpiryDate = new DateTime(2030, 1, 1)
                },
                new Product
                {
                    ProductId = 2,
                    Name = "Battery Pack 12V",
                    Category = "Energy Storage",
                    Quantity = 25,
                    Supplier = "PowerCell Corp",
                    Price = 149.99m,
                    ExpiryDate = new DateTime(2030, 1, 1)
                }
            );
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var salt = "SunTechSalt2024"; // In production, use a random salt per user
                var saltedPassword = password + salt;
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(hashedBytes) + ":" + salt;
            }
        }
    }
}
