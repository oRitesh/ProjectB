public class AdminLogic
{
    private readonly UserAccess userAccess;
    private readonly AdminAccess adminAccess;

    public AdminLogic()
    {
        this.userAccess = new UserAccess();
        this.adminAccess = new AdminAccess();
    }

    public Gebruiker? GetUserIfAuthorized(int userId)
    {
        var user = userAccess.GetRoleByUser(userId);
        if (user == null)
        {
            return null;
        }

        if (user.Rol == 2 || user.Rol == 3)
        {
            return user;
        }

        return null;
    }

    public bool IsUserAdmin(Gebruiker user)
    {
        return user.Rol == 2 || user.Rol == 3;
    }

    public string? EnsureAdminExists()
    {
        if (adminAccess.AdminExists()) return null;

        string tempPassword = GenerateSecurePassword();

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
        return tempPassword;
    }

    private static string GenerateSecurePassword()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 16)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}