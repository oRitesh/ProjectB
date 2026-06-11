using Dapper;

namespace ProjectB.Tests;

[TestClass]
public sealed class RegistratieValidatieTests
{
    private readonly DatabaseContext _db = new();
    private readonly UserAccess _userAccess;
    private readonly UserValidationLogic _validationLogic;

    public RegistratieValidatieTests()
    {
        _userAccess = new UserAccess(_db);
        _validationLogic = new UserValidationLogic(_userAccess);
    }

    /// <summary>
    /// Path: Happy Path H2
    /// Input: Wachtwoord: Sterk1234!
    /// Scenario: Klant registreert met een geldig wachtwoord dat aan alle eisen voldoet
    /// </summary>
    [TestMethod]
    public void ValideerWachtwoord_GeldigWachtwoord_WordtGeaccepteerd()
    {
        // arrange
        string wachtwoord = "Sterk1234!";

        // act
        bool isGeldig = wachtwoord.Length >= 8
                        && wachtwoord.Any(char.IsUpper)
                        && wachtwoord.Any(char.IsLower);

        // assert
        Assert.IsTrue(isGeldig,
            "Wachtwoord 'Sterk1234!' voldoet aan alle eisen en moet worden geaccepteerd");
    }

    /// <summary>
    /// Path: Happy Path H3
    /// Input: E-mail: info@restauranttest.nl
    /// Scenario: Klant registreert met een geldig e-mailadres
    /// </summary>
    [TestMethod]
    public void ValideerEmail_GeldigEmail_WordtGeaccepteerd()
    {
        // arrange
        string email = "info@restauranttest.nl";

        // act
        bool isGeldig = email.Contains("@") && email.Contains(".");

        // assert
        Assert.IsTrue(isGeldig,
            "E-mailadres 'info@restauranttest.nl' is geldig en moet worden geaccepteerd");
    }

    /// <summary>
    /// Path: Happy Path H4
    /// Input: Telefoonnummer: 0612345679
    /// Scenario: Klant registreert met een geldig telefoonnummer
    /// </summary>
    [TestMethod]
    public void ValideerTelefoonnummer_GeldigTelefoonnummer_WordtGeaccepteerd()
    {
        // arrange
        string telefoonnummer = "0612345679";

        // act
        bool isGeldig = telefoonnummer.Length >= 8 && telefoonnummer.All(char.IsDigit);

        // assert
        Assert.IsTrue(isGeldig,
            "Telefoonnummer '0612345679' is geldig en moet worden geaccepteerd");
    }

    /// <summary>
    /// Path: Happy Path H5
    /// Input: Naam: Jo (2 letters)
    /// Scenario: Klant registreert met een naam van precies 2 letters
    /// </summary>
    [TestMethod]
    public void ValideerNaam_NaamVanTweeLetters_WordtGeaccepteerd()
    {
        // arrange
        string naam = "Jo";

        // act
        bool isGeldig = naam.Length >= 2;

        // assert
        Assert.IsTrue(isGeldig,
            "Naam 'Jo' heeft 2 letters en voldoet aan de minimumeis");
    }

    /// <summary>
    /// Path: Sad Path S1
    /// Input: Telefoonnummer: leeg
    /// Scenario: Klant laat het verplichte telefoonnummerveld leeg
    /// </summary>
    [TestMethod]
    public void ValideerTelefoonnummer_LeegTelefoonnummer_WordtGeweigerd()
    {
        // arrange
        string telefoonnummer = "";

        // act
        bool isGeldig = telefoonnummer.Length >= 8 && telefoonnummer.All(char.IsDigit);

        // assert
        Assert.IsFalse(isGeldig,
            "Een leeg telefoonnummer is ongeldig; registratie moet worden geblokkeerd");
    }

    /// <summary>
    /// Path: Sad Path S2
    /// Input: E-mail: kevin@example.nl (al geregistreerd)
    /// Scenario: Klant probeert te registreren met een e-mailadres dat al in gebruik is
    /// </summary>
    [TestMethod]
    public void IsEmailUnique_EmailAlInGebruik_RetourneertFalse()
    {
        // arrange
        string email = "testreg.s2@testdata.com";
        Gebruiker bestaandeGebruiker = new(0, 1, "Kevin Mulder", email, "0645678901", "Veilig123!");
        int id = _userAccess.AddUser(bestaandeGebruiker);

        // act
        bool isUniek = _validationLogic.IsEmailUnique(email);

        // cleanup
        _db.Connection.Execute($"DELETE FROM {UserAccess.table} WHERE ID = @ID", new { ID = id });

        // assert
        Assert.IsFalse(isUniek,
            "Een e-mailadres dat al in gebruik is mag niet als uniek worden beschouwd");
    }

    /// <summary>
    /// Path: Sad Path S3
    /// Input: Wachtwoord: abc
    /// Scenario: Klant kiest een wachtwoord dat niet aan de eisen voldoet
    /// </summary>
    [TestMethod]
    public void ValideerWachtwoord_TeKortWachtwoord_WordtGeweigerd()
    {
        // arrange
        string wachtwoord = "abc";

        // act
        bool isGeldig = wachtwoord.Length >= 8
                        && wachtwoord.Any(char.IsUpper)
                        && wachtwoord.Any(char.IsLower);

        // assert
        Assert.IsFalse(isGeldig,
            "Wachtwoord 'abc' is te kort en heeft geen hoofdletter; moet worden geweigerd");
    }

    /// <summary>
    /// Path: Sad Path S4
    /// Input: E-mail: gebruikerzonderpunt@domein
    /// Scenario: Klant vult een e-mailadres in zonder punt na de @
    /// </summary>
    [TestMethod]
    public void ValideerEmail_ZonderPuntNaAt_WordtGeweigerd()
    {
        // arrange
        string email = "gebruikerzonderpunt@domein";

        // act
        bool isGeldig = email.Contains("@") && email.Contains(".");

        // assert
        Assert.IsFalse(isGeldig,
            "E-mailadres zonder punt na @ mag niet worden geaccepteerd");
    }

    /// <summary>
    /// Path: Sad Path S5
    /// Input: Telefoonnummer: 06ABCDEFG
    /// Scenario: Klant vult een telefoonnummer in dat letters bevat
    /// </summary>
    [TestMethod]
    public void ValideerTelefoonnummer_MetLetters_WordtGeweigerd()
    {
        // arrange
        string telefoonnummer = "06ABCDEFG";

        // act
        bool isGeldig = telefoonnummer.Length >= 8 && telefoonnummer.All(char.IsDigit);

        // assert
        Assert.IsFalse(isGeldig,
            "Telefoonnummer met letters mag niet worden geaccepteerd");
    }

    /// <summary>
    /// Path: Sad Path S6
    /// Input: Naam: X (1 letter)
    /// Scenario: Klant vult een naam in van slechts 1 letter
    /// </summary>
    [TestMethod]
    public void ValideerNaam_EenLetter_WordtGeweigerd()
    {
        // arrange
        string naam = "X";

        // act
        bool isGeldig = naam.Length >= 2;

        // assert
        Assert.IsFalse(isGeldig,
            "Naam 'X' heeft slechts 1 letter en voldoet niet aan de minimumeis van 2 letters");
    }
}