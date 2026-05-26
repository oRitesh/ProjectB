using System.Collections.Generic;
using System.Linq;
using Dapper;

public class BestellingMenuItemAccess
{
    private readonly DatabaseContext db;
    public const string Table = "BestellingMenuItem";

    public BestellingMenuItemAccess(DatabaseContext db)
    {
        this.db = db;
    }

    public List<BestellingMenuItem> GetAllBestellingMenuItems()
    {
        string sql = $@"SELECT * FROM {Table};";
        return db.Connection.Query<BestellingMenuItem>(sql).ToList();
    }
}