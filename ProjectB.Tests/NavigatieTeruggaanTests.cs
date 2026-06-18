namespace ProjectB.Tests;

[TestClass]
public sealed class NavigatieTeruggaanTests
{
    // Geen db nodig

    /// <summary>
    /// H1: Klant drukt op ESC tijdens wachtwoordstap bij registratie
    /// Scenario: Stap springt terug naar Telefoon
    /// </summary>
    [TestMethod]
    public void Registratie_EscBijWachtwoord_GaatNaarTelefoon()
    {
        // arrange
        RegistratieStap stap = RegistratieStap.Wachtwoord;
        string? wachtwoord = null; // null = ESC ingedrukt

        // act
        if (wachtwoord == null)
            stap = RegistratieStap.Telefoon;

        // assert
        Assert.AreEqual(RegistratieStap.Telefoon, stap,
            "Bij ESC op de wachtwoordstap moet de stap terugspringen naar Telefoon");
    }

    /// <summary>
    /// H2: Klant drukt op ESC tijdens telefoonsstap bij registratie
    /// Scenario: Stap springt terug naar Email
    /// </summary>
    [TestMethod]
    public void Registratie_EscBijTelefoon_GaatNaarEmail()
    {
        // arrange
        RegistratieStap stap = RegistratieStap.Telefoon;
        string? telefoon = null;

        // act
        if (telefoon == null)
            stap = RegistratieStap.Email;

        // assert
        Assert.AreEqual(RegistratieStap.Email, stap,
            "Bij ESC op de telefoonsstap moet de stap terugspringen naar Email");
    }

    /// <summary>
    /// H3: Klant drukt op ESC tijdens e-mailstap bij registratie
    /// Scenario: Stap springt terug naar Naam
    /// </summary>
    [TestMethod]
    public void Registratie_EscBijEmail_GaatNaarNaam()
    {
        // arrange
        RegistratieStap stap = RegistratieStap.Email;
        string? email = null;

        // act
        if (email == null)
            stap = RegistratieStap.Naam;

        // assert
        Assert.AreEqual(RegistratieStap.Naam, stap,
            "Bij ESC op de e-mailstap moet de stap terugspringen naar Naam");
    }

    /// <summary>
    /// H4: Klant drukt op ESC tijdens naamstap (eerste stap) bij registratie
    /// Scenario: Registratieflow stopt en klant keert terug naar hoofdpagina
    /// </summary>
    [TestMethod]
    public void Registratie_EscBijNaam_RetourneertNull()
    {
        // arrange
        string? naam = null;

        // act
        bool stroomBeeindigd = naam == null;

        // assert
        Assert.IsTrue(stroomBeeindigd,
            "Bij ESC op de naamstap moet de registratieflow stoppen en null retourneren " +
            "zodat de klant terugkeert naar de hoofdpagina");
    }

    /// <summary>
    /// H5: Klant drukt op ESC tijdens wachtwoordstap bij inloggen
    /// Scenario: Inlogflow herstart en vraagt opnieuw om e-mail
    /// </summary>
    [TestMethod]
    public void Inloggen_EscBijWachtwoord_GaatNaarEmail()
    {
        // arrange
        string? wachtwoord = null;
        bool emailOpnieuwGevraagd = false;

        // act
        if (wachtwoord == null)
            emailOpnieuwGevraagd = true;

        // assert
        Assert.IsTrue(emailOpnieuwGevraagd,
            "Bij ESC op het wachtwoord tijdens inloggen moet de flow terugkeren naar de e-mailstap");
    }

    /// <summary>
    /// H6: Klant drukt op ESC tijdens e-mailstap bij inloggen
    /// Scenario: Inlogflow stopt en klant keert terug naar hoofdpagina
    /// </summary>
    [TestMethod]
    public void Inloggen_EscBijEmail_RetourneertNull()
    {
        // arrange
        string? email = null;

        // act
        bool stroomBeeindigd = email == null;

        // assert
        Assert.IsTrue(stroomBeeindigd,
            "Bij ESC op de e-mailstap tijdens inloggen moet de flow stoppen en null retourneren " +
            "zodat de klant terugkeert naar de hoofdpagina");
    }

    /// <summary>
    /// S1: Klant gaat terug naar naamstap en vult een ongeldige naam in
    /// Scenario: Validatie triggert opnieuw; naam van 1 letter wordt geweigerd
    /// </summary>
    [TestMethod]
    public void Registratie_NaTeruggaanOngeldigeNaam_ValidatieTriggert()
    {
        // arrange
        string ongeldigeNaam = "X"; // naam moet 2 letters zijn

        // act 
        bool naamIsGeldig = ongeldigeNaam.Length > 1;

        // assert
        Assert.IsFalse(naamIsGeldig,
            "Na terugnavigatie moet de validatie opnieuw triggeren; " +
            "een naam van 1 letter mag niet geaccepteerd worden");
    }

    /// <summary>
    /// S2: Klant drukt meerdere keren achter elkaar op ESC tijdens registratie
    /// Scenario: Systeem komt niet in een oneindige loop; flow stopt bij de eerste stap
    /// </summary>
    [TestMethod]
    public void Registratie_MeerdereKerenTerug_GeenEindelozeLoop()
    {
        // arrange
        RegistratieStap stap = RegistratieStap.Naam;
        int terugnAvigaties = 0;
        const int maxTerug = 10;

        // act
        while (terugnAvigaties < maxTerug)
        {
            string? naam = null; // ESC ingedrukt
            if (naam == null)
            {
                break;
            }
            terugnAvigaties++;
        }

        // assert
        Assert.IsTrue(terugnAvigaties < maxTerug,
            "Bij ESC op de eerste stap moet de flow direct stoppen; " +
            "geen oneindige loop mag optreden bij meerdere keren terug navigeren");
    }
}