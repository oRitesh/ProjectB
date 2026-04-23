using Dapper;

public class AdminAccess
{
    private readonly DatabaseContext db;
    private readonly UserAccess userAccess;
    public AdminAccess(DatabaseContext db)
    {
        this.db = db;
        this.userAccess = new UserAccess(db);
    }
    public void CheckAdminAccountExistence()
    {
        string sql = "SELECT COUNT(*) FROM Gebruiker WHERE Rol = 2";
        int count = db.Connection.ExecuteScalar<int>(sql);
        if (count == 0)
        {
            userAccess.AddUser(new Gebruiker(0, 2, "admin", "admin@restaurant_b.nl", "", "boot123"));
        }
    }
}