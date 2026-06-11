using System;
using System.Linq;

public class RegistratieUI
{
    private readonly UserAccess userAccess;
    private readonly UserValidationLogic validationLogic;

    public RegistratieUI(UserAccess userAccess)
    {
        this.userAccess = userAccess;
        this.validationLogic = new UserValidationLogic(userAccess);
    }

    public Gebruiker? Registreer()
    {
        Console.Clear();

        bool userExists = false;

        RegistratieStap stap = RegistratieStap.Naam;
        string? naam = null, email = null, telefoon = null, wachtwoord = null;

        while (true)
        {
            switch (stap)
            {
                case RegistratieStap.Naam:
                    Console.Clear();
                    naam = InputValidatie.ValideerInput(
                        "=== Registreren ===\nDruk op Escape om terug te gaan.\n\nNaam",
                        x => x.Length > 1,
                        "Naam moet minimaal 2 letters zijn."
                    );
                    if (naam == null)
                    {
                        return null;
                    }
                    else stap = RegistratieStap.Email;
                    break;

                case RegistratieStap.Email:
                    Console.Clear();
                    email = VraagUniekEmail(naam!);
                    if (email == null)
                    {
                        stap = RegistratieStap.Naam;
                    }
                    else stap = RegistratieStap.Telefoon;
                    break;

                case RegistratieStap.Telefoon:
                    Console.Clear();
                    telefoon = VraagTelefoonMetCheck(ref userExists, naam!, email!);
                    if (telefoon == null) stap = RegistratieStap.Email;
                    else stap = RegistratieStap.Wachtwoord;
                    break;

                case RegistratieStap.Wachtwoord:
                    Console.Clear();
                    wachtwoord = InputValidatie.ValideerInput(
                        $"=== Registreren ===\nDruk op Escape om terug te gaan.\n\nNaam: {naam}\nE-mailadres: {email}\nTelefoonnummer: {telefoon}\nWachtwoord (min. 8 tekens, 1 hoofdletter, 1 kleine letter)",
                        x => x.Length >= 8 && x.Any(char.IsUpper) && x.Any(char.IsLower),
                        "Wachtwoord moet minimaal 8 tekens bevatten, én minstens 1 hoofdletter en 1 kleine letter."
                    );
                    if (wachtwoord == null) stap = RegistratieStap.Telefoon;
                    else stap = RegistratieStap.Bevestig;
                    break;

                case RegistratieStap.Bevestig:
                    Console.Clear();
                    string sterren = new string('*', wachtwoord!.Length);
                    string? wachtwoordBevestig = InputValidatie.ValideerInput(
                        $"=== Registreren ===\nDruk op Escape om terug te gaan.\n\nNaam: {naam}\nE-mailadres: {email}\nTelefoonnummer: {telefoon}\nWachtwoord (min. 8 tekens, 1 hoofdletter, 1 kleine letter): {sterren}\nBevestig wachtwoord",
                        x => x == wachtwoord,
                        "Wachtwoorden komen niet overeen."
                    );
                    if (wachtwoordBevestig == null) stap = RegistratieStap.Wachtwoord;
                    else goto done;
                    break;
            }
        }
    done:

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
            int id = userAccess.AddUser(nieuweGebruiker);
            nieuweGebruiker.ID = id;
        }

        Console.WriteLine();
        Console.WriteLine("✔ Registratie succesvol!");
        Console.WriteLine($"Welkom {naam}!");
        Console.ReadKey(true);

        return nieuweGebruiker;
    }

    private string? VraagUniekEmail(string naam)
    {
        while (true)
        {
            Console.Clear();
            string? email = InputValidatie.ValideerInput(
                $"=== Registreren ===\nDruk op Escape om terug te gaan.\n\nNaam: {naam}\nE-mailadres",
                x => x.Contains("@") && x.Contains("."),
                "Ongeldig e-mailadres."
            );
            if (email == null) return null;

            if (!validationLogic.IsEmailUnique(email))
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

    private string? VraagTelefoonMetCheck(ref bool userExists, string naam, string email)
    {
        while (true)
        {
            Console.Clear();
            string? telefoon = InputValidatie.ValideerInput(
                $"=== Registreren ===\nDruk op Escape om terug te gaan.\n\nNaam: {naam}\nE-mailadres: {email}\nTelefoonnummer",
                x => x.Length >= 8 && x.All(char.IsDigit),
                "Telefoonnummer moet minimaal 8 cijfers bevatten en mag geen letters bevatten."
            );
            if (telefoon == null) return null;

            if (!validationLogic.IsPhoneNumberAvailable(telefoon))
            {
                Console.WriteLine();
                Console.WriteLine("Dit telefoonnummer is al gekoppeld aan een account.");
                Console.ReadKey(true);
                Console.Clear();
                continue;
            }

            if (validationLogic.IsPhoneNumberForGuest(telefoon))
            {
                userExists = true;
            }

            return telefoon;
        }
    }
}
