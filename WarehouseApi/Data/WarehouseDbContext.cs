using Microsoft.EntityFrameworkCore;
using WarehouseApi.Domain.Entities;

namespace WarehouseApi.Data;

public class WarehouseDbContext : DbContext
{
    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<DeliveryItem> DeliveryItems => Set<DeliveryItem>();
    public DbSet<StockBalance> StockBalances => Set<StockBalance>();
    public DbSet<ProductSupplier> ProductSuppliers => Set<ProductSupplier>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasIndex(x => x.UserName).IsUnique();
        });

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

        modelBuilder.Entity<StockBalance>(entity =>
        {
            entity.HasOne(x => x.Product)
                .WithOne(p => p.StockBalance)
                .HasForeignKey<StockBalance>(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.HasOne(x => x.Supplier)
                .WithMany(s => s.Deliveries)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DocumentNumber).IsUnique();
        });

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
