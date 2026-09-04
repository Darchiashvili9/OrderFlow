using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.Data
{
    public class OrderFlowDbContext : DbContext
    {
        public OrderFlowDbContext(DbContextOptions<OrderFlowDbContext> options) : base(options) { }


        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>().Property(p => p.ProductPrice).HasPrecision(18, 2);
            modelBuilder.Entity<Product>().Property(p => p.ProductName).HasMaxLength(200);
            modelBuilder.Entity<Product>().Property(p => p.RowVersion).IsRowVersion();

            modelBuilder.Entity<Order>()
                .HasMany(item => item.OrderItems)
                .WithOne(order => order.Order)
                .HasForeignKey(item => item.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>().Property(o => o.UnitPrice).HasPrecision(18, 2);
            modelBuilder.Entity<OrderItem>()
                .HasOne(p => p.Product)
                .WithMany(ord => ord.OrderItems)
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>().Property(cust => cust.Name).HasMaxLength(150);
            modelBuilder.Entity<Customer>().Property(cust => cust.Address).HasMaxLength(200);
            modelBuilder.Entity<Customer>().HasIndex(c => c.Email).IsUnique();
            modelBuilder.Entity<Customer>().Property(cust => cust.Email).HasMaxLength(200);
        }
    }
}
