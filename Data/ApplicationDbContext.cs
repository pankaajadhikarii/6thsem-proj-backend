using Bizkit_backend.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bizkit_backend.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<BusinessType> BusinessTypes => Set<BusinessType>();
    public DbSet<BusinessTypeProduct> BusinessTypeProducts => Set<BusinessTypeProduct>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ResaleListing> ResaleListings => Set<ResaleListing>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --------------------------------------------------
        // BusinessTypeProduct
        // --------------------------------------------------

        modelBuilder.Entity<BusinessTypeProduct>()
            .HasKey(item => new
            {
                item.BusinessTypeId,
                item.ProductId
            });

        modelBuilder.Entity<BusinessTypeProduct>()
            .HasOne(item => item.BusinessType)
            .WithMany(businessType => businessType.BusinessTypeProducts)
            .HasForeignKey(item => item.BusinessTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BusinessTypeProduct>()
            .HasOne(item => item.Product)
            .WithMany(product => product.BusinessTypeProducts)
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Cascade);


        // --------------------------------------------------
        // Cart
        // --------------------------------------------------

        modelBuilder.Entity<Cart>()
            .HasIndex(cart => cart.UserId)
            .IsUnique();

        modelBuilder.Entity<Cart>()
            .HasOne(cart => cart.User)
            .WithOne(user => user.Cart)
            .HasForeignKey<Cart>(cart => cart.UserId)
            .OnDelete(DeleteBehavior.Cascade);


        // --------------------------------------------------
        // CartItem
        // --------------------------------------------------

        modelBuilder.Entity<CartItem>()
            .HasIndex(item => new
            {
                item.CartId,
                item.ProductId
            })
            .IsUnique();

        modelBuilder.Entity<CartItem>()
            .HasOne(item => item.Cart)
            .WithMany(cart => cart.CartItems)
            .HasForeignKey(item => item.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CartItem>()
            .HasOne(item => item.Product)
            .WithMany(product => product.CartItems)
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);


        // --------------------------------------------------
        // Category
        // --------------------------------------------------

        modelBuilder.Entity<Category>()
            .HasIndex(category => category.Name)
            .IsUnique();


        // --------------------------------------------------
        // Product
        // --------------------------------------------------

        modelBuilder.Entity<Product>()
            .Property(product => product.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Product>()
            .HasOne(product => product.Category)
            .WithMany(category => category.Products)
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasOne(product => product.SupersedesProduct)
            .WithMany(product => product.NewerProducts)
            .HasForeignKey(product => product.SupersedesProductId)
            .OnDelete(DeleteBehavior.Restrict);


        // --------------------------------------------------
        // Order
        // --------------------------------------------------

        modelBuilder.Entity<Order>()
            .HasIndex(order => order.OrderNumber)
            .IsUnique();

        modelBuilder.Entity<Order>()
            .Property(order => order.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .HasOne(order => order.Customer)
            .WithMany(user => user.Orders)
            .HasForeignKey(order => order.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);


        // --------------------------------------------------
        // OrderItem
        // --------------------------------------------------

        modelBuilder.Entity<OrderItem>()
            .Property(item => item.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(item => item.Subtotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .HasOne(item => item.Order)
            .WithMany(order => order.OrderItems)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(item => item.Product)
            .WithMany(product => product.OrderItems)
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderItem>()
            .HasOne(item => item.ResaleListing)
            .WithMany(listing => listing.OrderItems)
            .HasForeignKey(item => item.ResaleListingId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderItem>()
            .HasIndex(item => item.ResaleListingId)
            .IsUnique();


        // --------------------------------------------------
        // Payment
        // --------------------------------------------------

        modelBuilder.Entity<Payment>()
            .Property(payment => payment.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Payment>()
            .HasOne(payment => payment.Order)
            .WithMany(order => order.Payments)
            .HasForeignKey(payment => payment.OrderId)
            .OnDelete(DeleteBehavior.Cascade);


        // --------------------------------------------------
        // ResaleListing
        // --------------------------------------------------

        modelBuilder.Entity<ResaleListing>()
            .Property(listing => listing.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ResaleListing>()
            .HasOne(listing => listing.Product)
            .WithMany(product => product.ResaleListings)
            .HasForeignKey(listing => listing.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ResaleListing>()
            .HasOne(listing => listing.Seller)
            .WithMany(user => user.ResaleListings)
            .HasForeignKey(listing => listing.SellerId)
            .OnDelete(DeleteBehavior.Restrict);


        // --------------------------------------------------
        // Notification
        // --------------------------------------------------

        modelBuilder.Entity<Notification>()
            .HasOne(notification => notification.User)
            .WithMany(user => user.Notifications)
            .HasForeignKey(notification => notification.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notification>()
            .HasOne(notification => notification.Product)
            .WithMany(product => product.NewProductNotifications)
            .HasForeignKey(notification => notification.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Notification>()
            .HasOne(notification => notification.OldProduct)
            .WithMany(product => product.OldProductNotifications)
            .HasForeignKey(notification => notification.OldProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}