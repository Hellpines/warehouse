using WarehouseApp.Data;
using WarehouseApp.Services;

namespace WarehouseApp;

public static class ConsoleMenu
{
    public static void Run(WarehouseDbContext db)
    {
        var service = new WarehouseService(db);

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("========== СКЛАД (ЛР 5-6: ADO.NET + EF Core) ==========");
            Console.WriteLine("1. Список товаров и остатков (EF, AsNoTracking)");
            Console.WriteLine("2. Список поставщиков (many-to-many)");
            Console.WriteLine("3. Список поставок (Include)");
            Console.WriteLine("4. Добавить товар (Add + ChangeTracker)");
            Console.WriteLine("5. Изменить остаток товара");
            Console.WriteLine("6. Зарегистрировать поставку (транзакция EF)");
            Console.WriteLine("7. Удалить товар (Cascade/Restrict)");
            Console.WriteLine("8. Товары ниже минимального остатка");
            Console.WriteLine("9. ExecuteUpdate: поднять мин. остаток периферии");
            Console.WriteLine("10. Демонстрация N+1 и Include");
            Console.WriteLine("11. ADO.NET: отчёт по остаткам");
            Console.WriteLine("12. ADO.NET: транзакционное списание");
            Console.WriteLine("13. ADO.NET: агрегаты");
            Console.WriteLine("0. Выход");
            Console.Write("Выбор: ");

            var choice = Console.ReadLine()?.Trim();
            Console.WriteLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        service.ListProducts();
                        break;
                    case "2":
                        service.ListSuppliers();
                        break;
                    case "3":
                        service.ListDeliveries();
                        break;
                    case "4":
                        service.AddProductInteractive();
                        break;
                    case "5":
                        service.UpdateProductStockInteractive();
                        break;
                    case "6":
                        service.RegisterDeliveryInteractive();
                        break;
                    case "7":
                        service.DeleteProductInteractive();
                        break;
                    case "8":
                        service.ShowLowStock();
                        break;
                    case "9":
                        service.BulkRaiseMinStock();
                        break;
                    case "10":
                        service.DemonstrateNPlusOne();
                        break;
                    case "11":
                        AdoNetDemo.RunStockReport();
                        break;
                    case "12":
                        AdoNetDemo.RunTransactionalWriteOff();
                        break;
                    case "13":
                        AdoNetDemo.RunScalarStats();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Неизвестный пункт меню.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
}
