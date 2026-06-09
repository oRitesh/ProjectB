using Dapper;

public class AdminAccess
{
    private readonly DatabaseContext db;

    public AdminAccess(DatabaseContext db)
    {
        this.db = db;
    }

    public bool AdminExists()
    {
        string sql = "SELECT COUNT(*) FROM Gebruiker WHERE Rol = 2";
        int count = db.Connection.ExecuteScalar<int>(sql);
        return count > 0;
    }
}