using WarehouseApp;
using WarehouseApp.Data;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("Инициализация базы данных склада (SQLite + EF Core)...");

using var db = new WarehouseDbContext();
DbInitializer.Initialize(db);

Console.WriteLine($"База готова: {WarehouseDbContext.DatabaseFileName}");
ConsoleMenu.Run(db);
