using System;
using System.Linq;

public class RegistratieUI
{
    private readonly UserAccess userAccess;

    public RegistratieUI(UserAccess userAccess)
    {
        this.userAccess = userAccess;
    }

    public Gebruiker? Registreer()
    {
        Console.Clear();
        Console.WriteLine("=== Registreren ===");
        Console.WriteLine();

        bool userExists = false;

        string naam = InputValidatie.ValideerInput(
            "Naam",
            x => x.Length > 0,
            "Naam mag niet leeg zijn."
        );

        string email = VraagUniekEmail();

        string telefoon = VraagTelefoonMetCheck(ref userExists);

        string wachtwoord = InputValidatie.ValideerInput(
            "Wachtwoord (min. 8 tekens, 1 hoofdletter, 1 kleine letter)",
            x => x.Length >= 8
                 && x.Any(char.IsUpper)
                 && x.Any(char.IsLower),
            "Wachtwoord moet minimaal 8 tekens bevatten, én minstens 1 hoofdletter en 1 kleine letter."
        );

        Gebruiker nieuweGebruiker;

        if (userExists)
        {
            Console.WriteLine();
            Console.WriteLine("We hebben uw gegevens gevonden van een eerdere reservering.");
            Console.WriteLine("Uw gast-account wordt omgezet naar een officieel account...");

            var result = userAccess.ChangeRole(telefoon, naam, 1, email, wachtwoord);

            if (result == null)
            {
                Console.WriteLine("Kon gast-account niet omzetten naar een echt account.");
                return null;
            }

            nieuweGebruiker = result;


            Console.WriteLine("Account succesvol bijgewerkt!");
        }
        else
        {
            nieuweGebruiker = new Gebruiker(0, 1, naam, email, telefoon, wachtwoord);
            userAccess.AddUser(nieuweGebruiker);
        }

        Console.WriteLine();
        Console.WriteLine("✔ Registratie succesvol!");
        Console.WriteLine($"Welkom {naam}!");
        Console.ReadKey(true);

        return nieuweGebruiker;
    }

    private string VraagUniekEmail()
    {
        while (true)
        {
            string email = InputValidatie.ValideerInput(
                "E-mailadres",
                x => x.Contains("@") && x.Contains("."),
                "Ongeldig e-mailadres."
            );

            var checkUser = userAccess.GetUserByEmail(email);
            if (checkUser != null)
            {
                Console.WriteLine();
                Console.WriteLine("E-mailadres is al in gebruik. Probeer het opnieuw.");
                Console.ReadKey(true);
                Console.Clear();
                continue;
            }

            return email;
        }
    }

    private string VraagTelefoonMetCheck(ref bool userExists)
    {
        while (true)
        {
            string telefoon = InputValidatie.ValideerInput(
                "Telefoonnummer",
                x => x.Length >= 8 && x.All(char.IsDigit),
                "Telefoonnummer moet minimaal 8 cijfers bevatten en mag geen letters bevatten."
            );

            var checkUser = userAccess.GetUserByPhoneNumber(telefoon);

            if (checkUser != null && checkUser.Rol == 1)
            {
                Console.WriteLine();
                Console.WriteLine("Dit telefoonnummer is al gekoppeld aan een account.");
                Console.ReadKey(true);
                Console.Clear();
                continue;
            }
            else if (checkUser != null && checkUser.Rol == 0)
            {
                userExists = true;
            }

            return telefoon;
        }
    }
}
