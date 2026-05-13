using System;

public class InlogUI
{
    private readonly UserAccess userAccess;

    public InlogUI(UserAccess userAccess)
    {
        this.userAccess = userAccess;
    }

    public Gebruiker? Login()
    {
        Console.Clear();
        Console.WriteLine("=== Inloggen ===");
        Console.WriteLine();

        string email = InputValidatie.ValideerInput(
            "E-mailadres",
            x => x.Contains("@") && x.Contains("."),
            "Ongeldig e-mailadres."
        );

        string wachtwoord = InputValidatie.ValideerInput(
            "Wachtwoord",
            x => x.Length > 0,
            "Wachtwoord mag niet leeg zijn."
        );

        var user = userAccess.GetUserByEmail(email, wachtwoord);

        if (user != null)
        {
            Console.WriteLine();
            Console.WriteLine($"✔ Succesvol ingelogd! Welkom terug, {user.Naam}.");
            Console.ReadKey(true);
            return user;
        }

        Console.WriteLine();
        Console.WriteLine("❌ Onjuiste inloggegevens.");
        Console.ReadKey(true);
        return null;
    }
}
