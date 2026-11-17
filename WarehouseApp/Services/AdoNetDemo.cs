using Microsoft.Data.Sqlite;
using WarehouseApp.Data;

namespace WarehouseApp.Services;

/// <summary>
/// Демонстрация прямого доступа к SQLite через ADO.NET:
/// параметризованные запросы, DbDataReader, транзакции.
/// </summary>
public static class AdoNetDemo
{
    public static string ConnectionString => $"Data Source={WarehouseDbContext.DatabaseFileName}";

    public static void RunStockReport()
    {
        Console.WriteLine();
        Console.WriteLine("=== ADO.NET: отчёт по остаткам (параметризованный SELECT) ===");

        const string sql = """
            SELECT p.Sku AS Article, p.Name, s.Quantity, p.Unit, p.MinStockLevel
            FROM Products p
            INNER JOIN StockBalances s ON s.ProductId = p.Id
            WHERE s.Quantity >= $minQty
            ORDER BY p.Name;
            """;

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        // Параметр защищает от SQL-инъекций (в отличие от конкатенации строк).
        command.Parameters.AddWithValue("$minQty", 0);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var article = reader.GetString(0);
            var name = reader.GetString(1);
            var qty = reader.GetDecimal(2);
            var unit = reader.GetString(3);
            var min = reader.GetDecimal(4);
            Console.WriteLine($"{article,-12} {name,-30} {qty,8:0.###} {unit} (мин. {min:0.###})");
        }
    }

    public static void RunTransactionalWriteOff()
    {
        Console.WriteLine();
        Console.WriteLine("=== ADO.NET: транзакционное списание остатка ===");
        Console.Write("Артикул товара: ");
        var article = Console.ReadLine()?.Trim() ?? string.Empty;
        Console.Write("Количество к списанию: ");
        if (!decimal.TryParse(Console.ReadLine(), out var amount) || amount <= 0)
        {
            Console.WriteLine("Некорректное количество.");
            return;
        }

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            int productId;
            decimal currentQty;

            using (var findCmd = connection.CreateCommand())
            {
                findCmd.Transaction = transaction;
                findCmd.CommandText = """
                    SELECT p.Id, s.Quantity
                    FROM Products p
                    INNER JOIN StockBalances s ON s.ProductId = p.Id
                    WHERE p.Sku = $sku;
                    """;
                findCmd.Parameters.AddWithValue("$sku", article);

                using var reader = findCmd.ExecuteReader();
                if (!reader.Read())
                {
                    throw new InvalidOperationException("Товар с таким артикулом не найден.");
                }

                productId = reader.GetInt32(0);
                currentQty = reader.GetDecimal(1);
            }

            if (currentQty < amount)
            {
                throw new InvalidOperationException(
                    $"Недостаточно остатка: есть {currentQty:0.###}, требуется {amount:0.###}.");
            }

            using (var updateCmd = connection.CreateCommand())
            {
                updateCmd.Transaction = transaction;
                updateCmd.CommandText = """
                    UPDATE StockBalances
                    SET Quantity = Quantity - $amount,
                        LastUpdated = $updated
                    WHERE ProductId = $productId;
                    """;
                updateCmd.Parameters.AddWithValue("$amount", amount);
                updateCmd.Parameters.AddWithValue("$updated", DateTime.UtcNow.ToString("O"));
                updateCmd.Parameters.AddWithValue("$productId", productId);

                var rows = updateCmd.ExecuteNonQuery();
                if (rows != 1)
                {
                    throw new InvalidOperationException("Не удалось обновить остаток.");
                }
            }

            transaction.Commit();
            Console.WriteLine(
                $"Списано {amount:0.###}. Было {currentQty:0.###}, стало {currentQty - amount:0.###}.");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.WriteLine($"Ошибка ADO.NET, выполнен Rollback: {ex.Message}");
        }
    }

    public static void RunScalarStats()
    {
        Console.WriteLine();
        Console.WriteLine("=== ADO.NET: агрегаты (ExecuteScalar) ===");

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                (SELECT COUNT(*) FROM Products) AS ProductCount,
                (SELECT COUNT(*) FROM Suppliers) AS SupplierCount,
                (SELECT COUNT(*) FROM Deliveries) AS DeliveryCount,
                (SELECT ROUND(COALESCE(SUM(Quantity), 0), 3) FROM StockBalances) AS TotalStock;
            """;

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            Console.WriteLine($"Товаров:     {reader.GetInt64(0)}");
            Console.WriteLine($"Поставщиков: {reader.GetInt64(1)}");
            Console.WriteLine($"Поставок:    {reader.GetInt64(2)}");
            Console.WriteLine($"Суммарный остаток: {reader.GetDecimal(3):0.###}");
        }
    }
}
