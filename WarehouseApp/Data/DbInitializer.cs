using Microsoft.EntityFrameworkCore;
using WarehouseApp.Models;

namespace WarehouseApp.Data;

/// <summary>
/// Применение миграций и заполнение демо-данными.
/// </summary>
public static class DbInitializer
{
    public static void Initialize(WarehouseDbContext db)
    {
        db.Database.Migrate();

        if (db.Products.Any())
        {
            return;
        }

        var suppliers = new[]
        {
            new Supplier
            {
                Name = "ООО «БелПромСнаб»",
                ContactPerson = "Иванов П. С.",
                Phone = "+375 29 111-22-33",
                Email = "sales@belprom.by",
                Address = "г. Минск, ул. Промышленная, 12"
            },
            new Supplier
            {
                Name = "ЧТУП «ОфисМаркет»",
                ContactPerson = "Сидорова А. В.",
                Phone = "+375 33 444-55-66",
                Email = "order@officemarket.by",
                Address = "г. Брест, пр-т Машерова, 45"
            },
            new Supplier
            {
                Name = "ИП Козлов Д. Н.",
                ContactPerson = "Козлов Д. Н.",
                Phone = "+375 25 777-88-99",
                Email = "kozlov@mail.by",
                Address = "г. Гродно, ул. Советская, 8"
            }
        };

        var products = new[]
        {
            new Product
            {
                Name = "Бумага A4 80 г/м²",
                Article = "PAP-A4-80",
                Unit = "пач",
                Category = "Канцтовары",
                MinStockLevel = 20
            },
            new Product
            {
                Name = "Картридж лазерный HP 85A",
                Article = "CRG-HP-85A",
                Unit = "шт",
                Category = "Расходные материалы",
                MinStockLevel = 5
            },
            new Product
            {
                Name = "Клавиатура USB",
                Article = "KB-USB-01",
                Unit = "шт",
                Category = "Периферия",
                MinStockLevel = 10
            },
            new Product
            {
                Name = "Мышь оптическая",
                Article = "MS-OPT-01",
                Unit = "шт",
                Category = "Периферия",
                MinStockLevel = 10
            },
            new Product
            {
                Name = "Скотч упаковочный 48 мм",
                Article = "TAPE-48",
                Unit = "шт",
                Category = "Упаковка",
                MinStockLevel = 15
            }
        };

        db.Suppliers.AddRange(suppliers);
        db.Products.AddRange(products);
        db.SaveChanges();

        db.ProductSuppliers.AddRange(
            new ProductSupplier { ProductId = products[0].Id, SupplierId = suppliers[1].Id, IsPrimary = true },
            new ProductSupplier { ProductId = products[0].Id, SupplierId = suppliers[0].Id, IsPrimary = false },
            new ProductSupplier { ProductId = products[1].Id, SupplierId = suppliers[0].Id, IsPrimary = true },
            new ProductSupplier { ProductId = products[2].Id, SupplierId = suppliers[2].Id, IsPrimary = true },
            new ProductSupplier { ProductId = products[3].Id, SupplierId = suppliers[2].Id, IsPrimary = true },
            new ProductSupplier { ProductId = products[3].Id, SupplierId = suppliers[0].Id, IsPrimary = false },
            new ProductSupplier { ProductId = products[4].Id, SupplierId = suppliers[1].Id, IsPrimary = true }
        );

        foreach (var product in products)
        {
            db.StockBalances.Add(new StockBalance
            {
                ProductId = product.Id,
                Quantity = 0,
                LastUpdated = DateTime.UtcNow
            });
        }

        var delivery1 = new Delivery
        {
            SupplierId = suppliers[0].Id,
            DeliveryDate = DateTime.Today.AddDays(-10),
            DocumentNumber = "Накл-001/2026",
            Notes = "Первичная поставка расходных материалов",
            Items =
            {
                new DeliveryItem { ProductId = products[0].Id, Quantity = 50, UnitPrice = 12.50m },
                new DeliveryItem { ProductId = products[1].Id, Quantity = 12, UnitPrice = 89.00m }
            }
        };

        var delivery2 = new Delivery
        {
            SupplierId = suppliers[2].Id,
            DeliveryDate = DateTime.Today.AddDays(-3),
            DocumentNumber = "Накл-014/2026",
            Notes = "Периферия для рабочих мест",
            Items =
            {
                new DeliveryItem { ProductId = products[2].Id, Quantity = 20, UnitPrice = 35.00m },
                new DeliveryItem { ProductId = products[3].Id, Quantity = 25, UnitPrice = 18.50m }
            }
        };

        db.Deliveries.AddRange(delivery1, delivery2);
        db.SaveChanges();

        ApplyDeliveryToStock(db, delivery1);
        ApplyDeliveryToStock(db, delivery2);
        db.SaveChanges();
    }

    public static void ApplyDeliveryToStock(WarehouseDbContext db, Delivery delivery)
    {
        foreach (var item in delivery.Items)
        {
            var stock = db.StockBalances.Find(item.ProductId);
            if (stock is null)
            {
                stock = new StockBalance
                {
                    ProductId = item.ProductId,
                    Quantity = 0,
                    LastUpdated = DateTime.UtcNow
                };
                db.StockBalances.Add(stock);
            }

            stock.Quantity += item.Quantity;
            stock.LastUpdated = DateTime.UtcNow;
        }
    }
}
