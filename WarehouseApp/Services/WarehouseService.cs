using Microsoft.EntityFrameworkCore;
using WarehouseApp.Data;
using WarehouseApp.Models;

namespace WarehouseApp.Services;

/// <summary>
/// CRUD и демонстрационные сценарии через EF Core.
/// </summary>
public class WarehouseService
{
    private readonly WarehouseDbContext _db;

    public WarehouseService(WarehouseDbContext db)
    {
        _db = db;
    }

    public void ListProducts()
    {
        var products = _db.Products
            .AsNoTracking()
            .Include(p => p.StockBalance)
            .OrderBy(p => p.Name)
            .ToList();

        Console.WriteLine();
        Console.WriteLine("=== Товары и остатки (AsNoTracking) ===");
        foreach (var p in products)
        {
            var qty = p.StockBalance?.Quantity ?? 0;
            var mark = qty < p.MinStockLevel ? " [НИЗКИЙ ОСТАТОК]" : string.Empty;
            Console.WriteLine(
                $"#{p.Id} {p.Article,-12} {p.Name,-30} {qty,8:0.###} {p.Unit}{mark}");
        }
    }

    public void ListSuppliers()
    {
        var suppliers = _db.Suppliers
            .AsNoTracking()
            .Include(s => s.ProductSuppliers)
            .ThenInclude(ps => ps.Product)
            .OrderBy(s => s.Name)
            .ToList();

        Console.WriteLine();
        Console.WriteLine("=== Поставщики (many-to-many с товарами) ===");
        foreach (var s in suppliers)
        {
            Console.WriteLine($"#{s.Id} {s.Name} | {s.ContactPerson} | {s.Phone}");
            foreach (var link in s.ProductSuppliers)
            {
                var primary = link.IsPrimary ? "основной" : "доп.";
                Console.WriteLine($"    - {link.Product?.Name} ({primary})");
            }
        }
    }

    public void ListDeliveries()
    {
        var deliveries = _db.Deliveries
            .AsNoTracking()
            .Include(d => d.Supplier)
            .Include(d => d.Items)
            .ThenInclude(i => i.Product)
            .OrderByDescending(d => d.DeliveryDate)
            .ToList();

        Console.WriteLine();
        Console.WriteLine("=== Поставки (Include) ===");
        foreach (var d in deliveries)
        {
            Console.WriteLine(
                $"#{d.Id} {d.DocumentNumber} от {d.DeliveryDate:dd.MM.yyyy} | {d.Supplier?.Name}");
            foreach (var item in d.Items)
            {
                Console.WriteLine(
                    $"    {item.Product?.Article}: {item.Quantity:0.###} × {item.UnitPrice:0.00} BYN");
            }
        }
    }

    public void AddProductInteractive()
    {
        Console.Write("Название: ");
        var name = Console.ReadLine()?.Trim() ?? string.Empty;
        Console.Write("Артикул: ");
        var article = Console.ReadLine()?.Trim() ?? string.Empty;
        Console.Write("Ед. изм. [шт]: ");
        var unit = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(unit)) unit = "шт";
        Console.Write("Категория: ");
        var category = Console.ReadLine()?.Trim();
        Console.Write("Мин. остаток: ");
        decimal.TryParse(Console.ReadLine(), out var minStock);

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(article))
        {
            Console.WriteLine("Название и артикул обязательны.");
            return;
        }

        var product = new Product
        {
            Name = name,
            Article = article,
            Unit = unit,
            Category = category,
            MinStockLevel = minStock,
            StockBalance = new StockBalance
            {
                Quantity = 0,
                LastUpdated = DateTime.UtcNow
            }
        };

        _db.Products.Add(product);
        ShowChangeTracker("после Add");
        _db.SaveChanges();
        Console.WriteLine($"Товар добавлен, Id = {product.Id}");
    }

    public void UpdateProductStockInteractive()
    {
        Console.Write("Id товара: ");
        if (!int.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Некорректный Id.");
            return;
        }

        var product = _db.Products
            .Include(p => p.StockBalance)
            .FirstOrDefault(p => p.Id == id);

        if (product is null)
        {
            Console.WriteLine("Товар не найден.");
            return;
        }

        Console.WriteLine($"Текущий остаток: {product.StockBalance?.Quantity:0.###}");
        Console.Write("Новый остаток: ");
        if (!decimal.TryParse(Console.ReadLine(), out var qty))
        {
            Console.WriteLine("Некорректное число.");
            return;
        }

        if (product.StockBalance is null)
        {
            product.StockBalance = new StockBalance
            {
                ProductId = product.Id,
                Quantity = qty,
                LastUpdated = DateTime.UtcNow
            };
        }
        else
        {
            product.StockBalance.Quantity = qty;
            product.StockBalance.LastUpdated = DateTime.UtcNow;
        }

        ShowChangeTracker("после изменения остатка");
        _db.SaveChanges();
        Console.WriteLine("Остаток обновлён.");
    }

    public void RegisterDeliveryInteractive()
    {
        var suppliers = _db.Suppliers.AsNoTracking().ToList();
        Console.WriteLine("Поставщики:");
        foreach (var s in suppliers)
        {
            Console.WriteLine($"  {s.Id}. {s.Name}");
        }

        Console.Write("Id поставщика: ");
        if (!int.TryParse(Console.ReadLine(), out var supplierId))
        {
            Console.WriteLine("Некорректный Id.");
            return;
        }

        Console.Write("Номер документа: ");
        var doc = Console.ReadLine()?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(doc))
        {
            Console.WriteLine("Номер документа обязателен.");
            return;
        }

        var delivery = new Delivery
        {
            SupplierId = supplierId,
            DeliveryDate = DateTime.Today,
            DocumentNumber = doc,
            Notes = "Создано из меню"
        };

        while (true)
        {
            Console.Write("Id товара (пустая строка — завершить): ");
            var raw = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(raw)) break;
            if (!int.TryParse(raw, out var productId)) continue;

            Console.Write("Количество: ");
            if (!decimal.TryParse(Console.ReadLine(), out var quantity) || quantity <= 0) continue;
            Console.Write("Цена за ед.: ");
            if (!decimal.TryParse(Console.ReadLine(), out var price) || price < 0) continue;

            delivery.Items.Add(new DeliveryItem
            {
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = price
            });
        }

        if (delivery.Items.Count == 0)
        {
            Console.WriteLine("Нет позиций — поставка не создана.");
            return;
        }

        using var tx = _db.Database.BeginTransaction();
        try
        {
            _db.Deliveries.Add(delivery);
            _db.SaveChanges();
            DbInitializer.ApplyDeliveryToStock(_db, delivery);
            _db.SaveChanges();
            tx.Commit();
            Console.WriteLine($"Поставка зарегистрирована, Id = {delivery.Id}");
        }
        catch (Exception ex)
        {
            tx.Rollback();
            Console.WriteLine($"Ошибка, откат транзакции EF: {ex.Message}");
        }
    }

    public void DeleteProductInteractive()
    {
        Console.Write("Id товара для удаления: ");
        if (!int.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Некорректный Id.");
            return;
        }

        var product = _db.Products.Find(id);
        if (product is null)
        {
            Console.WriteLine("Товар не найден.");
            return;
        }

        // Нельзя удалить товар, если он есть в поставках (Restrict на DeliveryItem).
        var used = _db.DeliveryItems.Any(i => i.ProductId == id);
        if (used)
        {
            Console.WriteLine("Товар участвует в поставках — удаление запрещено (Restrict).");
            return;
        }

        _db.Products.Remove(product);
        ShowChangeTracker("после Remove");
        _db.SaveChanges();
        Console.WriteLine("Товар и связанный остаток удалены (Cascade).");
    }

    public void BulkRaiseMinStock()
    {
        var affected = _db.Products
            .Where(p => p.Category == "Периферия")
            .ExecuteUpdate(setters => setters
                .SetProperty(p => p.MinStockLevel, p => p.MinStockLevel + 2));

        Console.WriteLine($"ExecuteUpdate: обновлено записей = {affected}");
    }

    public void DemonstrateNPlusOne()
    {
        Console.WriteLine();
        Console.WriteLine("=== Демонстрация N+1 (плохо) ===");
        // Ленивая загрузка отключена по умолчанию в EF Core —
        // имитируем N+1 отдельными запросами в цикле.
        var deliveryIds = _db.Deliveries.AsNoTracking().Select(d => d.Id).ToList();
        var queryCount = 1; // уже один запрос на список Id
        foreach (var id in deliveryIds)
        {
            var items = _db.DeliveryItems
                .AsNoTracking()
                .Where(i => i.DeliveryId == id)
                .ToList();
            queryCount++;
            Console.WriteLine($"  Поставка #{id}: позиций {items.Count}");
        }

        Console.WriteLine($"Всего SQL-запросов (N+1): {queryCount}");

        Console.WriteLine();
        Console.WriteLine("=== Исправление через Include (один/два запроса) ===");
        var good = _db.Deliveries
            .AsNoTracking()
            .Include(d => d.Items)
            .ToList();
        Console.WriteLine($"Загружено поставок: {good.Count}, позиций суммарно: {good.Sum(d => d.Items.Count)}");
        Console.WriteLine("Запросы выполнены через JOIN/split-query, без цикла N+1.");
    }

    public void ShowChangeTracker(string stage)
    {
        Console.WriteLine($"--- ChangeTracker ({stage}) ---");
        foreach (var entry in _db.ChangeTracker.Entries())
        {
            Console.WriteLine($"  {entry.Entity.GetType().Name}: {entry.State}");
        }
    }

    public void ShowLowStock()
    {
        var low = _db.Products
            .AsNoTracking()
            .Include(p => p.StockBalance)
            .Where(p => p.StockBalance != null && p.StockBalance.Quantity < p.MinStockLevel)
            .ToList();

        Console.WriteLine();
        Console.WriteLine("=== Товары ниже минимального остатка ===");
        if (low.Count == 0)
        {
            Console.WriteLine("Дефицита нет.");
            return;
        }

        foreach (var p in low)
        {
            Console.WriteLine(
                $"{p.Article}: {p.StockBalance!.Quantity:0.###} / мин. {p.MinStockLevel:0.###}");
        }
    }
}
