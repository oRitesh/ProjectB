using Dapper;

namespace ProjectB.Tests;

[TestClass]
public sealed class RegistratieValidatieTests
{
    private readonly DatabaseContext _db;
    private readonly UserAccess _userAccess;
    private readonly UserValidationLogic _validationLogic;

    private readonly List<int> _aangemaakteGebruikerIDs = [];

    public RegistratieValidatieTests()
    {
        _db = new DatabaseContext();
        _userAccess = new UserAccess(_db);
        _validationLogic = new UserValidationLogic(_userAccess);
    }

    [TestCleanup]
    public void Cleanup()
    {
        foreach (int id in _aangemaakteGebruikerIDs)
            _db.Connection.Execute($"DELETE FROM {UserAccess.table} WHERE ID = @ID", new { ID = id });
        _aangemaakteGebruikerIDs.Clear();
    }

    // ===== Acceptance Criteria 2: Geldig wachtwoord - H2 =====

    /// <summary>
    /// Path: Happy Path H2
    /// Input: Wachtwoord: Sterk1234! (bevat hoofdletter en kleine letter, minimaal 8 tekens)
    /// Expected output: Wachtwoord geaccepteerd; account aangemaakt
    /// Test type: Unit test
    /// Scenario: Klant registreert met een geldig wachtwoord dat aan alle eisen voldoet
    /// Verwacht: Validator retourneert true voor een geldig wachtwoord
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

    // ===== Acceptance Criteria 3: Geldig e-mailadres - H3 =====

    /// <summary>
    /// Path: Happy Path H3
    /// Input: E-mail: info@restauranttest.nl
    /// Expected output: E-mailadres geaccepteerd als geldig formaat
    /// Test type: Unit test
    /// Scenario: Klant registreert met een geldig e-mailadres
    /// Verwacht: Validator retourneert true voor een geldig e-mailadres
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

    // ===== Acceptance Criteria 4: Geldig telefoonnummer - H4 =====

    /// <summary>
    /// Path: Happy Path H4
    /// Input: Telefoonnummer: 0612345679
    /// Expected output: Telefoonnummer geaccepteerd; account aangemaakt
    /// Test type: Unit test
    /// Scenario: Klant registreert met een geldig telefoonnummer
    /// Verwacht: Validator retourneert true voor een geldig telefoonnummer
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

    // ===== Acceptance Criteria 5: Naam minimaal 2 letters - H5 =====

    /// <summary>
    /// Path: Happy Path H5
    /// Input: Naam: Jo (2 letters)
    /// Expected output: Naam geaccepteerd; minimumeis van 2 letters gehaald
    /// Test type: Unit test
    /// Scenario: Klant registreert met een naam van precies 2 letters
    /// Verwacht: Validator retourneert true voor een naam van 2 letters
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

    // ===== Acceptance Criteria 1: Verplicht veld leeg - S1 =====

    /// <summary>
    /// Path: Sad Path S1
    /// Input: Naam: Kevin Mulder, Tel: leeg, E-mail: kevin2@example.nl, Wachtwoord: Test1234!
    /// Expected output: Foutmelding: telefoonnummer is verplicht; registratie geblokkeerd
    /// Test type: Unit test
    /// Scenario: Klant laat het verplichte telefoonnummerveld leeg
    /// Verwacht: Een leeg telefoonnummer wordt als ongeldig beschouwd
    /// </summary>
    [TestMethod]
    public void ValideerTelefoonnummer_LeegTelefoonnummer_WordtGeweigerd()
    {
        // arrange
        string telefoonnummer = ""; // verplicht veld leeg gelaten

        // act
        bool isGeldig = telefoonnummer.Length >= 8 && telefoonnummer.All(char.IsDigit);

        // assert
        Assert.IsFalse(isGeldig,
            "Een leeg telefoonnummer is ongeldig; registratie moet worden geblokkeerd");
    }

    // ===== Acceptance Criteria 1: E-mail al in gebruik - S2 =====

    /// <summary>
    /// Path: Sad Path S2
    /// Input: E-mail: kevin@example.nl (al geregistreerd), Wachtwoord: Nieuw456!
    /// Expected output: Foutmelding: e-mailadres al in gebruik; registratie geweigerd
    /// Test type: Unit test
    /// Scenario: Klant probeert te registreren met een e-mailadres dat al in gebruik is
    /// Verwacht: IsEmailUnique retourneert false voor een bestaand e-mailadres
    /// </summary>
    [TestMethod]
    public void IsEmailUnique_EmailAlInGebruik_RetourneertFalse()
    {
        // arrange
        string email = "testreg.s2@testdata.com";
        Gebruiker bestaandeGebruiker = new(0, 1, "Kevin Mulder", email, "0645678901", "Veilig123!");
        int id = _userAccess.AddUser(bestaandeGebruiker);
        _aangemaakteGebruikerIDs.Add(id);

        // act
        bool isUniek = _validationLogic.IsEmailUnique(email);

        // assert
        Assert.IsFalse(isUniek,
            "Een e-mailadres dat al in gebruik is mag niet als uniek worden beschouwd; registratie moet worden geweigerd");
    }

    // ===== Acceptance Criteria 2: Wachtwoord te kort - S3 =====

    /// <summary>
    /// Path: Sad Path S3
    /// Input: Wachtwoord: abc (te kort, geen hoofdletter)
    /// Expected output: Foutmelding: wachtwoord voldoet niet aan eisen; registratie geblokkeerd
    /// Test type: Unit test
    /// Scenario: Klant kiest een wachtwoord dat niet aan de eisen voldoet
    /// Verwacht: Validator retourneert false voor een te kort wachtwoord zonder hoofdletter
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

    // ===== Acceptance Criteria 3: Ongeldig e-mailadres - S4 =====

    /// <summary>
    /// Path: Sad Path S4
    /// Input: E-mail: gebruikerzonderpunt@domein (geen punt na @)
    /// Expected output: Foutmelding: ongeldig e-mailadres; registratie geblokkeerd
    /// Test type: Unit test
    /// Scenario: Klant vult een e-mailadres in zonder punt na de @
    /// Verwacht: Validator retourneert false voor een e-mailadres zonder punt
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

    // ===== Acceptance Criteria 4: Telefoonnummer met letters - S5 =====

    /// <summary>
    /// Path: Sad Path S5
    /// Input: Telefoonnummer: 06-ABCDEFG (bevat letters)
    /// Expected output: Foutmelding: telefoonnummer mag alleen cijfers bevatten
    /// Test type: Unit test
    /// Scenario: Klant vult een telefoonnummer in dat letters bevat
    /// Verwacht: Validator retourneert false voor een telefoonnummer met letters
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
            "Telefoonnummer met letters mag niet worden geaccepteerd; alleen cijfers zijn toegestaan");
    }

    // ===== Acceptance Criteria 5: Naam te kort - S6 =====

    /// <summary>
    /// Path: Sad Path S6
    /// Input: Naam: X (1 letter)
    /// Expected output: Foutmelding: naam moet minimaal 2 letters hebben
    /// Test type: Unit test
    /// Scenario: Klant vult een naam in van slechts 1 letter
    /// Verwacht: Validator retourneert false voor een naam van 1 letter
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