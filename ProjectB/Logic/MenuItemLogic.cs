public class MenuItemLogic
{
    private readonly MenuItemAccess menuItemAccess;

    public MenuItemLogic()
    {
        menuItemAccess = new MenuItemAccess();
    }

    public List<MenuItem> GetAllMenuItems() => menuItemAccess.GetAllMenuItems();

    public List<MenuItem> GetItemsByCategory(int categoryId) => menuItemAccess.GetItemsByCategory(categoryId);

    public void AddMenuItem(MenuItem item) => menuItemAccess.AddMenuItem(item);

    public void UpdateMenuItem(MenuItem item) => menuItemAccess.UpdateMenuItem(item);

    public void DeleteMenuItem(int id) => menuItemAccess.DeleteMenuItem(id);

    public string GetMenuItemNameById(int id) => menuItemAccess.GetMenuItemNameById(id);

    public bool IsGeldigePrijs(decimal prijs) => prijs >= 0;
}
