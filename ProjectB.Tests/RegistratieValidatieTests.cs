using Dapper;

namespace ProjectB.Tests;

[TestClass]
public sealed class RegistratieValidatieTests
{
    private readonly DatabaseContext dataBaseInstance = DatabaseContext.Instance;
    private readonly UserLogic _userLogic;
    private readonly List<int> _aangemaakteGebruikerIDs = [];

    public RegistratieValidatieTests()
    {
        _userLogic = new UserLogic();
    }

    [TestCleanup]
    public void Cleanup()
    {
        foreach (int id in _aangemaakteGebruikerIDs)
            dataBaseInstance.Connection.Execute($"DELETE FROM {UserAccess.table} WHERE ID = @ID", new { ID = id });
        _aangemaakteGebruikerIDs.Clear();
    }

    /// <summary>
    /// Path: Happy Path H2
    /// Input: Wachtwoord: Sterk1234!
    /// Scenario: Klant registreert met een geldig wachtwoord dat aan alle eisen voldoet
    /// </summary>
    [TestMethod]
    public void IsGeldigWachtwoord_GeldigWachtwoord_RetourneertTrue()
    {
        // Arrange
        UserLogic userLogic = new();
        string wachtwoord = "Sterk1234!";

        // Act
        bool isGeldig = userLogic.IsGeldigWachtwoord(wachtwoord);

        // Assert
        Assert.IsTrue(isGeldig,
            "Wachtwoord 'Sterk1234!' voldoet aan alle eisen en moet worden geaccepteerd");
    }

    /// <summary>
    /// Path: Happy Path H3
    /// Input: E-mail: info@restauranttest.nl
    /// Scenario: Klant registreert met een geldig e-mailadres
    /// </summary>
    [TestMethod]
    public void IsGeldigEmail_GeldigEmail_RetourneertTrue()
    {
        // Arrange
        UserLogic userLogic = new();
        string email = "info@restauranttest.nl";

        // Act
        bool isGeldig = userLogic.IsGeldigEmail(email);

        // Assert
        Assert.IsTrue(isGeldig,
            "E-mailadres 'info@restauranttest.nl' is geldig en moet worden geaccepteerd");
    }

    /// <summary>
    /// Path: Happy Path H4
    /// Input: Telefoonnummer: 0612345679
    /// Scenario: Klant registreert met een geldig telefoonnummer
    /// </summary>
    [TestMethod]
    public void IsGeldigTelefoonnummer_GeldigNummer_RetourneertTrue()
    {
        // Arrange
        string telefoonnummer = "0612345679";
        UserLogic userLogic = new();

        // Act
        bool isGeldig = userLogic.IsGeldigTelefoonnummer(telefoonnummer);

        // Assert
        Assert.IsTrue(isGeldig,
            "Telefoonnummer '0612345679' is geldig en moet worden geaccepteerd");
    }

    /// <summary>
    /// Path: Happy Path H5
    /// Input: Naam: Jo (2 letters)
    /// Scenario: Klant registreert met een naam van precies 2 letters
    /// </summary>
    [TestMethod]
    public void AddUser_NaamVanTweeLetters_WordtOpgeslagen()
    {
        // Arrange
        var gebruiker = new Gebruiker(0, 1, "Jo", "testvalidnaam@testdata.com", "0612345670", "Test1234!");

        // Act
        int id = _userLogic.AddUser(gebruiker);
        _aangemaakteGebruikerIDs.Add(id);
        var opgeslagen = dataBaseInstance.Connection.QueryFirstOrDefault<Gebruiker>(
            $"SELECT * FROM {UserAccess.table} WHERE ID = @ID", new { ID = id });

        // Assert
        Assert.IsNotNull(opgeslagen,
            "Gebruiker met naam 'Jo' moet succesvol worden opgeslagen");
        Assert.AreEqual("Jo", opgeslagen.Naam,
            "Opgeslagen naam moet exact 'Jo' zijn");
    }

    // ===== Acceptance Criteria 1: Verplicht veld leeg - S1 =====

    /// <summary>
    /// Path: Sad Path S1
    /// Input: Telefoonnummer: leeg
    /// Scenario: Klant laat het verplichte telefoonnummerveld leeg
    /// </summary>
    [TestMethod]
    public void IsGeldigTelefoonnummer_LeegNummer_RetourneertFalse()
    {
        UserLogic userLogic = new();
        // Arrange
        string telefoonnummer = "";

        // Act
        bool isGeldig = userLogic.IsGeldigTelefoonnummer(telefoonnummer);

        // Assert
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
        int id = _userLogic.AddUser(bestaandeGebruiker);

        // act
        bool isUniek = _userLogic.IsEmailUnique(email);

        // cleanup
        dataBaseInstance.Connection.Execute($"DELETE FROM {UserAccess.table} WHERE ID = @ID", new { ID = id });

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
    public void IsGeldigWachtwoord_TeKortWachtwoord_RetourneertFalse()
    {
        // Arrange
        UserLogic userLogic = new();
        string wachtwoord = "abc";

        // Act
        bool isGeldig = userLogic.IsGeldigWachtwoord(wachtwoord);

        // Assert
        Assert.IsFalse(isGeldig,
            "Wachtwoord 'abc' is te kort en heeft geen hoofdletter; moet worden geweigerd");
    }

    /// <summary>
    /// Path: Sad Path S4
    /// Input: E-mail: gebruikerzonderpunt@domein
    /// Scenario: Klant vult een e-mailadres in zonder punt na de @
    /// </summary>
    [TestMethod]
    public void IsGeldigEmail_ZonderPuntNaAt_RetourneertFalse()
    {
        // Arrange
        string email = "gebruikerzonderpunt@domein";
        UserLogic userLogic = new();

        // Act
        bool isGeldig = userLogic.IsGeldigEmail(email);

        // Assert
        Assert.IsFalse(isGeldig,
            "E-mailadres zonder punt na @ mag niet worden geaccepteerd");
    }

    /// <summary>
    /// Path: Sad Path S5
    /// Input: Telefoonnummer: 06ABCDEFG
    /// Scenario: Klant vult een telefoonnummer in dat letters bevat
    /// </summary>
    [TestMethod]
    public void IsGeldigTelefoonnummer_MetLetters_RetourneertFalse()
    {
        // Arrange
        string telefoonnummer = "06ABCDEFG";
        UserLogic userLogic = new();

        // Act
        bool isGeldig = userLogic.IsGeldigTelefoonnummer(telefoonnummer);

        // Assert
        Assert.IsFalse(isGeldig,
            "Telefoonnummer met letters mag niet worden geaccepteerd");
    }

    /// <summary>
    /// Path: Sad Path S6
    /// Input: Naam: X (1 letter)
    /// Scenario: Klant vult een naam in van slechts 1 letter
    /// Verwacht: Applicatie slaat gebruiker op met naam "X"; naamvalidatie ontbreekt in de logic-laag
    /// </summary>
    [TestMethod]
    public void AddUser_EenLetterNaam_NaamWordtOpgeslagen()
    {
        // Arrange
        var gebruiker = new Gebruiker(0, 1, "X", "testkortenaam@testdata.com", "0612345671", "Test1234!");

        // Act
        int id = _userLogic.AddUser(gebruiker);
        _aangemaakteGebruikerIDs.Add(id);
        var opgeslagen = dataBaseInstance.Connection.QueryFirstOrDefault<Gebruiker>(
            $"SELECT * FROM {UserAccess.table} WHERE ID = @ID", new { ID = id });

        // Assert
        Assert.IsNotNull(opgeslagen,
            "Gebruiker met naam 'X' wordt opgeslagen; de logic-laag heeft geen naamlengte-validatie");
        Assert.AreEqual("X", opgeslagen.Naam,
            "De opgeslagen naam moet exact 'X' zijn");
    }
}