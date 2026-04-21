using System.Collections.Generic;
using System.Linq;
using Dapper;

public class UserAccess
{
    private readonly DatabaseContext db;
    public const string table = "Gebruiker";

    public UserAccess(DatabaseContext db)
    {
        this.db = db;
    }

    public int AddUser(Gebruiker gebruiker)
    {
        string sql = $@"
            INSERT INTO {table}
            (Rol, Naam, Email, Telefoonnummer, Wachtwoord)
            VALUES
            (@Rol, @Naam, @Email, @Telefoonnummer, @Wachtwoord);
            SELECT last_insert_rowid();";

        //db.Connection.Execute(sql, gebruiker);
        return db.Connection.QuerySingle<int>(sql, gebruiker);
    }
}