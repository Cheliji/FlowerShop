using FlowerShop.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Flower> Flowers => Set<Flower>();
    public DbSet<FlowerInventory> FlowerInventories => Set<FlowerInventory>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Flower>().HasQueryFilter(f => !f.IsDeleted);

        modelBuilder.Entity<Flower>()
            .HasIndex(f => f.CategoryId);

        modelBuilder.Entity<Flower>()
            .HasIndex(f => f.Name);

        modelBuilder.Entity<CartItem>()
            .HasIndex(c => new { c.UserId, c.FlowerId, c.SelectedOptionQty })
            .IsUnique();

        modelBuilder.Entity<FlowerInventory>()
            .HasIndex(fi => fi.FlowerId)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Phone)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderNo)
            .IsUnique();

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "玫瑰", Icon = "🌹", SortOrder = 1, IsActive = true },
            new Category { Id = 2, Name = "百合", Icon = "🌷", SortOrder = 2, IsActive = true },
            new Category { Id = 3, Name = "康乃馨", Icon = "🌸", SortOrder = 3, IsActive = true },
            new Category { Id = 4, Name = "向日葵", Icon = "🌻", SortOrder = 4, IsActive = true },
            new Category { Id = 5, Name = "混搭花束", Icon = "💐", SortOrder = 5, IsActive = true },
            new Category { Id = 6, Name = "永生花", Icon = "🌺", SortOrder = 6, IsActive = true },
            new Category { Id = 7, Name = "绿植", Icon = "🌿", SortOrder = 7, IsActive = true }
        );
    }
}
