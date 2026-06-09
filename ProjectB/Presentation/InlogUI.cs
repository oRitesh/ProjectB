using System;

public class InlogUI
{
    private readonly UserLogic userLogic;

    public InlogUI(UserLogic userLogic)
    {
        this.userLogic = userLogic;
    }

    public Gebruiker? Login()
    {
        Console.Clear();
        string? email = null;
        string? wachtwoord = null;

        while (true)
        {
            Console.Clear();
            email = InputValidatie.ValideerInput(
            "=== Inloggen ===\nDruk op Escape om terug te gaan.\n\nE-mailadres",
            x => userLogic.IsGeldigEmail(x),
            "Ongeldig e-mailadres."
        );
            if (email == null) return null;

            Console.Clear();
            wachtwoord = InputValidatie.ValideerInput(
                $"=== Inloggen ===\nDruk op Escape om terug te gaan.\n\nE-mailadres: {email}\nWachtwoord",
                x => x.Length > 0,
                "Wachtwoord mag niet leeg zijn."
            );
            if (wachtwoord != null) break;
        }


        var user = userLogic.Login(email, wachtwoord);

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
