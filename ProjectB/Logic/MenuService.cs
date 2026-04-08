public class MenuService
{
    public List<MenuItem> Starters { get; private set; } = new();
    public List<MenuItem> Mains { get; private set; } = new();
    public List<MenuItem> Desserts { get; private set; } = new();
    public List<MenuItem> Drinks { get; private set; } = new();
    public List<MenuItem> Wines { get; private set; } = new();

    private readonly MenuItemAccess itemAccess;

    public MenuService(DatabaseContext db)
    {
        itemAccess = new MenuItemAccess(db);
        Starters = itemAccess.GetItemsByCategory(1);
        Mains = itemAccess.GetItemsByCategory(2);
        Desserts = itemAccess.GetItemsByCategory(3);
        Wines = itemAccess.GetItemsByCategory(4);
        Drinks = itemAccess.GetItemsByCategory(5);
    }
}
