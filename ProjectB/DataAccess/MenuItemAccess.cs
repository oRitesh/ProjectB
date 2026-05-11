using System.Collections.Generic;
using System.Linq;
using Dapper;

public class MenuItemAccess
{
    private readonly DatabaseContext db;
    public const string Table = "MenuItem";

    public MenuItemAccess(DatabaseContext db)
    {
        this.db = db;
    }

    public List<MenuItem> GetItemsByCategory(int categoryId)
    {
        string sql = $@"
            SELECT * FROM {Table}
            WHERE MenuCatogorieID = @CategoryID
            ORDER BY Prijs;";

        return db.Connection.Query<MenuItem>(sql, new { CategoryID = categoryId }).ToList();
    }

    public List<MenuItem> GetAllMenuItems()
    {
        string sql = $@"SELECT * FROM {Table} ORDER BY Naam;";
        return db.Connection.Query<MenuItem>(sql).ToList();
    }

    public void AddMenuItem(MenuItem item)
    {
        string sql = $@"
            INSERT INTO {Table} (Naam, Prijs, MenuCatogorieID)
            VALUES (@Naam, @Prijs, @MenuCatogorieID);";

        db.Connection.Execute(sql, item);
    }

    public void UpdateMenuItem(MenuItem item)
    {
        string sql = $@"
            UPDATE {Table}
            SET Naam = @Naam, Prijs = @Prijs, MenuCatogorieID = @MenuCatogorieID,
                Beschrijving = @Beschrijving, Allergeen = @Allergeen
            WHERE ID = @ID;";

        db.Connection.Execute(sql, item);
    }

    public void DeleteMenuItem(int id)
    {
        string sql = $@"
            DELETE FROM {Table}
            WHERE ID = @ID;";

        db.Connection.Execute(sql, new { ID = id });
    }
}
