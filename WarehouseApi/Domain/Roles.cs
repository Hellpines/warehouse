namespace WarehouseApi.Domain;

/// <summary>
/// Роли варианта № 6: кладовщик и менеджер.
/// </summary>
public static class Roles
{
    public const string Manager = "Manager";
    public const string Storekeeper = "Storekeeper";

    public const string ManagerOrStorekeeper = Manager + "," + Storekeeper;
}
