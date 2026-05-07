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
            string tempPassword = GenerateSecurePassword();
            Console.WriteLine("=== EERSTE OPSTART ===");
            Console.WriteLine($"Admin aangemaakt. Wachtwoord: {tempPassword}");
            Console.WriteLine("Bewaar dit wachtwoord veilig!");
            Console.WriteLine("======================");
            Console.WriteLine("Druk op een toets om door te gaan...");
            Console.ReadKey(true);

            var admin = new Gebruiker
            {
                ID = 0,
                Rol = 2,
                Naam = "admin",
                Email = "admin@restaurant_b.nl",
                Telefoonnummer = "",
                Wachtwoord = tempPassword
            };

            userAccess.AddUser(admin);
        }
    }

    private string GenerateSecurePassword()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 16)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}