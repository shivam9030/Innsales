
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OrderMicroservice.Domain;
using OrderMicroservice.Common.Enums;

namespace OrderMicroservice.Database
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

        // DbSets (top-level tables)
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ============================
            // Converters (enums, DateOnly)
            // ============================
            var orderSourceConverter      = new EnumToStringConverter<OrderSource>();
            var orderPriorityConverter    = new EnumToStringConverter<OrderPriority>();
            var shippingPriorityConverter = new EnumToStringConverter<ShippingPriority>();
            var itemTypeConverter         = new EnumToStringConverter<ItemType>();

            // Store DateOnly as SQL 'date'
            var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
                to   => to.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                from => DateOnly.FromDateTime(DateTime.SpecifyKind(from, DateTimeKind.Utc))
            );

            // ==========
            // Order
            // ==========
            modelBuilder.Entity<Order>(order =>
            {
                // Primary key
                order.HasKey(o => o.Id);

                // Columns
                order.Property(o => o.Id).ValueGeneratedOnAdd();

                order.Property(o => o.OrderNumber)
                     .IsRequired()
                     .HasMaxLength(64);

                order.Property(o => o.Source)
                     .HasConversion(orderSourceConverter)
                     .IsRequired();

                order.Property(o => o.Channel)
                     .IsRequired()
                     .HasMaxLength(32);

                order.Property(o => o.VendorId)
                     .IsRequired();

                order.Property(o => o.Currency)
                     .IsRequired()
                     .HasMaxLength(8);

                order.Property(o => o.PlacedAt)
                     .IsRequired();

                order.Property(o => o.OrderPriority)
                     .HasConversion(orderPriorityConverter)
                     .IsRequired();

                order.Property(o => o.ShippingPriority)
                     .HasConversion(shippingPriorityConverter)
                     .IsRequired();

                // ✅ Unique composite index: (OrderNumber, VendorId)
                order.HasIndex(o => new { o.OrderNumber, o.VendorId })
                     .IsUnique();

                // Helpful index for time-based querying
                order.HasIndex(o => o.PlacedAt);

                // Owned: ShippingDetail in Orders table
                order.OwnsOne(o => o.Shipping, sd =>
                {
                    sd.Property(s => s.Method)
                      .IsRequired()
                      .HasMaxLength(64);

                    sd.Property(s => s.PromisedDate)
                      .HasConversion(dateOnlyConverter)
                      .HasColumnType("date")
                      .IsRequired();

                    // Keep owned columns in same table
                    sd.ToTable("Orders");
                });

                // Navigation: Order → Items (configured on OrderItem side too)
                order.Navigation(o => o.Items).AutoInclude(false);
            });

            // ==========
            // Customer
            // ==========
            modelBuilder.Entity<Customer>(customer =>
            {
                // Primary key: your model uses CustomerId (string)
                customer.HasKey(c => c.CustomerId);

                customer.Property(c => c.CustomerId)
                        .IsRequired()
                        .HasMaxLength(64);

                customer.Property(c => c.Email)
                        .HasMaxLength(256);

                customer.Property(c => c.Phone)
                        .HasMaxLength(32);

                customer.Property(c => c.FullName)
                        .IsRequired()
                        .HasMaxLength(128);

                // Owned: BillingAddress
                customer.OwnsOne(c => c.BillingAddress, a =>
                {
                    a.Property(p => p.Line1).IsRequired().HasMaxLength(256);
                    a.Property(p => p.City).IsRequired().HasMaxLength(64);
                    a.Property(p => p.State).IsRequired().HasMaxLength(64);
                    a.Property(p => p.PostalCode).IsRequired().HasMaxLength(16);
                    a.Property(p => p.Country).IsRequired().HasMaxLength(16);
                    a.ToTable("Customers");
                });

                // Owned: ShippingAddress
                customer.OwnsOne(c => c.ShippingAddress, a =>
                {
                    a.Property(p => p.Line1).IsRequired().HasMaxLength(256);
                    a.Property(p => p.City).IsRequired().HasMaxLength(64);
                    a.Property(p => p.State).IsRequired().HasMaxLength(64);
                    a.Property(p => p.PostalCode).IsRequired().HasMaxLength(16);
                    a.Property(p => p.Country).IsRequired().HasMaxLength(16);
                    a.ToTable("Customers");
                });

                customer.HasIndex(c => c.Email);
            });

            // ==========
            // OrderItem
            // ==========
            modelBuilder.Entity<OrderItem>(item =>
            {
                // ✅ Composite key: (OrderId, ItemId)
                //   - ItemId is unique within an order
                item.HasKey(i => new { i.OrderId, i.ItemId });

                item.Property(i => i.OrderId)
                    .IsRequired();

                item.Property(i => i.ItemId)
                    .IsRequired()
                    .HasMaxLength(64);

                item.Property(i => i.ItemType)
                    .HasConversion(itemTypeConverter)
                    .IsRequired();

                item.Property(i => i.Sku)
                    .HasMaxLength(64);

                item.Property(i => i.Name)
                    .IsRequired()
                    .HasMaxLength(128);

                item.Property(i => i.Quantity)
                    .IsRequired();

                // ✅ Relationship: Order 1..N OrderItem (FK constraint)
                item.HasOne(i => i.Order)
                    .WithMany(o => o.Items)
                    .HasForeignKey(i => i.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Helpful index for querying all items of an order
                item.HasIndex(i => i.OrderId);
            });
        }
    }
}
