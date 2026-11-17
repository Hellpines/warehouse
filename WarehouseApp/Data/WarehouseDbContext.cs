using Microsoft.EntityFrameworkCore;
using WarehouseApp.Models;

namespace WarehouseApp.Data;

/// <summary>
/// Контекст EF Core для предметной области «Склад».
/// </summary>
public class WarehouseDbContext : DbContext
{
    public const string DatabaseFileName = "warehouse.db";

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<DeliveryItem> DeliveryItems => Set<DeliveryItem>();
    public DbSet<StockBalance> StockBalances => Set<StockBalance>();
    public DbSet<ProductSupplier> ProductSuppliers => Set<ProductSupplier>();

    public WarehouseDbContext()
    {
    }

    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite($"Data Source={DatabaseFileName}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Many-to-many через явную промежуточную сущность ProductSupplier.
        modelBuilder.Entity<ProductSupplier>(entity =>
        {
            entity.HasKey(x => new { x.ProductId, x.SupplierId });

            entity.HasOne(x => x.Product)
                .WithMany(p => p.ProductSuppliers)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Supplier)
                .WithMany(s => s.ProductSuppliers)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // One-to-one: Product ↔ StockBalance, каскадное удаление остатка вместе с товаром.
        modelBuilder.Entity<StockBalance>(entity =>
        {
            entity.HasOne(x => x.Product)
                .WithOne(p => p.StockBalance)
                .HasForeignKey<StockBalance>(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // One-to-many: Supplier → Deliveries.
        // Restrict: нельзя удалить поставщика, пока есть его поставки.
        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.HasOne(x => x.Supplier)
                .WithMany(s => s.Deliveries)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DocumentNumber).IsUnique();
        });

        // One-to-many: Delivery → DeliveryItems (каскад).
        modelBuilder.Entity<DeliveryItem>(entity =>
        {
            entity.HasOne(x => x.Delivery)
                .WithMany(d => d.Items)
                .HasForeignKey(x => x.DeliveryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Product)
                .WithMany(p => p.DeliveryItems)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(x => x.Article).IsUnique();
        });
    }
}
