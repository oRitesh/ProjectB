using System.Collections.Generic;
using System.Linq;
using Dapper;

public class UserAccess
{
    private readonly DatabaseContext db;
    public const string table = "User";

    public UserAccess(DatabaseContext db)
    {
        this.db = db;
    }

    public void AddUser(Gebruiker gebruiker)
    {
        string sql = $@"
            INSERT INTO {table}
            (Rol, Naam, Email, Telefoonnummer, Wachtwoord)
            VALUES
            (@Rol, @Naam, @Email, @Telefoonnummer, @Wachtwoord);";

        db.Connection.Execute(sql, gebruiker);
    }
}