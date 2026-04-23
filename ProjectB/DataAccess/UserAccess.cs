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

    // Voeg dit toe aan UserAccess.cs
    public Gebruiker? GetUserByEmail(string email, string password)
    {
        // We zoeken de gebruiker op basis van email
        string sql = $"SELECT * FROM {table} WHERE Email = @Email";

        var user = db.Connection.QuerySingleOrDefault<Gebruiker>(sql, new { Email = email });

        if (user != null && user.Wachtwoord == password)
        {
            return user;
        }

        return null;
    }

    public Gebruiker? GetUserByEmail(string email)
    {
        string sql = "SELECT * FROM Gebruiker WHERE Email = @Email";

        return db.Connection.QueryFirstOrDefault<Gebruiker>(sql, new { Email = email });
    }

    public Gebruiker? GetUserByPhoneNumber(string phoneNumber)
    {
        string sql = "SELECT * FROM Gebruiker WHERE Telefoonnummer = @Phone";

        return db.Connection.QueryFirstOrDefault<Gebruiker>(sql, new { Phone = phoneNumber });
    }

    public Gebruiker? ChangeRole(string telefoonnummer, string newName, int newRole, string email, string password)
    {
        string sql = $"UPDATE {table} SET Rol = @NewRole, Naam = @NewName, Email = @Email, Wachtwoord = @Password WHERE Telefoonnummer = @PhoneNumber";

        int rowsAffected = db.Connection.Execute(sql, new
        {
            NewRole = newRole,
            NewName = newName,
            Email = email,
            Password = password,
            PhoneNumber = telefoonnummer
        });

        if (rowsAffected > 0)
        {
            // Als de update succesvol was, haal dan de bijgewerkte gebruiker op
            return GetUserByPhoneNumber(telefoonnummer);
        }

        return null;
    }
}