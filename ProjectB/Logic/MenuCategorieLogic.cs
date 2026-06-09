public class MenuCategorieLogic
{
    private readonly MenuCategorieAccess menuCategorieAccess;

    public MenuCategorieLogic(DatabaseContext db)
    {
        menuCategorieAccess = new MenuCategorieAccess(db);
    }

    public List<MenuCategorie> GetAllCategories() => menuCategorieAccess.GetAllCategories();
}
