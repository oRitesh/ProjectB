public class MenuCategorieLogic
{
    private readonly MenuCategorieAccess menuCategorieAccess;

    public MenuCategorieLogic()
    {
        menuCategorieAccess = new MenuCategorieAccess();
    }

    public List<MenuCategorie> GetAllCategories() => menuCategorieAccess.GetAllCategories();
}
