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

    public Gebruiker? GetUserByEmail(string email, string password)
    {
        string sql = $"SELECT * FROM {table} WHERE Email = @Email";
        var user = db.Connection.QuerySingleOrDefault<Gebruiker>(sql, new { Email = email });

        if (user != null && user.Wachtwoord == password)
        {
            return user;
        }

        return null;
    }

    public Gebruiker? GetAdminAccount()
    {
        string sql = $"SELECT * FROM {table} WHERE Rol = 2 LIMIT 1";
        return db.Connection.QueryFirstOrDefault<Gebruiker>(sql);
    }

    public void EnsureAdminExists()
    {
        var admin = GetAdminAccount();
        if (admin == null)
        {
            var defaultAdmin = new Gebruiker
            {
                Rol           = 2,
                Naam          = "Admin",
                Email         = "admin@restaurantb.nl",
                Telefoonnummer = "0000000000",
                Wachtwoord    = "admin123"
            };
            AddUser(defaultAdmin);
        }
    }
}