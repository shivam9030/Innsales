using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using InnSales.Domain.Entities;
using InnSales.Domain.UserManagement.Entities;
using InnSales.Common.Enums;


namespace InnSales.DataBase
{
    public class InnSalesDbContext : IdentityDbContext<ApplicationUser>
    {
        public InnSalesDbContext(DbContextOptions<InnSalesDbContext> options) : base(options) { }

        // Domain Entities
        public DbSet<Client> Clients { get; set; }
        public DbSet<DocTemplate> DocumentTemplates { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<BasketItem> BasketItems { get; set; }
        public DbSet<FailedTransaction> FailedTransactions { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<PromoCode> PromoCodes { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Identity Table Mappings
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

            // Order Configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.HasOne(o => o.Customer)
                    .WithMany()
                    .HasForeignKey(o => o.CustomerId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(o => o.OrderStatus)
                    .HasConversion<string>();

                entity.Property(o => o.PaymentStatus)
                    .HasConversion<int>();

                entity.Property(o => o.Tax)
                    .HasPrecision(18, 2);

                entity.Property(o => o.TotalAmount)
                    .HasPrecision(18, 2);
            });

            // OrderItem Configuration
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.Id);

                entity.HasOne(oi => oi.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(oi => oi.OrderId);

                entity.HasOne(oi => oi.Product)
                    .WithMany()
                    .HasForeignKey(oi => oi.ProductId);


               
   entity.HasOne(oi => oi.PromoCode)
        .WithMany() // PromoCode does not have a collection of OrderItems
        .HasForeignKey(oi => oi.PromoCodeId)
        .OnDelete(DeleteBehavior.SetNull);


                entity.Property(oi => oi.UnitPrice)
                    .HasPrecision(18, 2);

            });
    //         modelBuilder.Entity<BasketItem>()
    // .HasOne(b => b.PromoCode)
    // .WithMany()
    // .HasForeignKey(b => b.PromoCodeId)
    // .OnDelete(DeleteBehavior.Restrict);

            // Product Configuration
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            // Payment Configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.HasOne(p => p.Order)
                    .WithMany()
                    .HasForeignKey(p => p.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(p => p.PaymentStatus)
                    .HasConversion<string>();

                entity.Property(p => p.AmountPaid)
                    .HasPrecision(18, 2);
            });

            // FailedTransaction Configuration
            modelBuilder.Entity<FailedTransaction>()
                .Property(ft => ft.Amount)
                .HasPrecision(18, 2);


            // Promotion Table Config
            modelBuilder.Entity<Promotion>(entity =>
            {
                entity.ToTable("InnSales_Promotion");
                entity.HasKey(p => p.PromotionId);
                entity.Property(p => p.Name).HasMaxLength(100).IsRequired();
                entity.Property(p => p.Description).HasColumnType("TEXT");
                entity.Property(p => p.minimumOrderValue).HasColumnType("decimal(10,2)");
                entity.Property(p => p.PromotionValue).HasColumnType("decimal(10,2)");
                entity.Property(p => p.DiscountType).HasConversion<string>(); // Store enum as string
                entity.Property(p => p.Status).HasConversion<string>(); // Store enum as string
            });

            // PromoCode Table Config
            modelBuilder.Entity<PromoCode>(entity =>
            {
                entity.ToTable("InnSales_Promocode");
                entity.HasKey(pc => pc.PromoCodeId);
                entity.Property(pc => pc.Code).HasMaxLength(50).IsRequired();
                entity.HasIndex(pc => pc.Code).IsUnique();
                entity.HasOne(pc => pc.Promotion)
                      .WithMany(p => p.PromoCodes)
                      .HasForeignKey(pc => pc.PromotionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

        }
    }
}
