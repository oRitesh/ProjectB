using System.Collections.Generic;
using System.Linq;
using Dapper;

public class MenuCategorieAccess
{
    private readonly DatabaseContext db;
    public const string Table = "MenuCatogorie";

    public MenuCategorieAccess()
    {
        this.db = DatabaseContext.Instance;
    }

    public List<MenuCategorie> GetAllCategories()
    {
        string sql = $@"SELECT * FROM {Table};";
        return db.Connection.Query<MenuCategorie>(sql).ToList();
    }
}
