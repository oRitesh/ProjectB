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

    public void DisplayAllItems()
    {
        string sql = $@"
            SELECT * FROM {Table}
            ORDER BY Prijs;";

        var items = db.Connection.Query<MenuItem>(sql).ToList();
        foreach (var item in items)
        {
            Console.WriteLine($"ID: {item.ID}, Naam: {item.Naam}, Prijs: {item.Prijs}, CategorieID: {item.MenuCatogorieID}");
        }
    }

    public void AddMenuItem(MenuItem item)
    {
        string sql = $@"
            INSERT INTO {Table} (Naam, Prijs, MenuCatogorieID, Beschrijving, allergeen)
            VALUES (@Naam, @Prijs, @MenuCatogorieID, @Beschrijving, @Allergeen);";

        db.Connection.Execute(sql, item);
    }

    public void UpdateMenuItem(MenuItem item)
    {
        string sql = $@"
            UPDATE {Table}
            SET Naam = @Naam, Prijs = @Prijs, MenuCatogorieID = @MenuCatogorieID
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
