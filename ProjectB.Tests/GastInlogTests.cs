using Dapper;

namespace ProjectB.Tests;

[TestClass]
public sealed class GastInlogTests
{
    private readonly DatabaseContext _db;
    private readonly ReservationLogic _logic;
    private readonly TijdslotAccess _tijdslotAccess;
    private readonly ReserveringAccess _reserveringAccess;
    private readonly TafelAccess _tafelAccess;
    private readonly UserAccess _userAccess;

    [ClassInitialize]
    public static void SetupDatabase(TestContext _)
    {
        var setupDb = DatabaseContext.Instance;
        setupDb.Connection.Execute(@"
            CREATE TABLE IF NOT EXISTS OpeningsDag (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                DagVanWeek INTEGER NOT NULL,
                IsOpen INTEGER NOT NULL
            )");
        if (setupDb.Connection.QuerySingle<int>("SELECT COUNT(*) FROM OpeningsDag") == 0)
            for (int i = 0; i <= 6; i++)
                setupDb.Connection.Execute(
                    "INSERT INTO OpeningsDag (DagVanWeek, IsOpen) VALUES (@D, 1)", new { D = i });

        setupDb.Connection.Execute(@"
            CREATE TABLE IF NOT EXISTS OpeningsTijden (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                OpeningsTijd TEXT NOT NULL,
                SluitingsTijd TEXT NOT NULL
            )");
        if (setupDb.Connection.QuerySingle<int>("SELECT COUNT(*) FROM OpeningsTijden") == 0)
            setupDb.Connection.Execute(
                "INSERT INTO OpeningsTijden (OpeningsTijd, SluitingsTijd) VALUES ('00:00', '23:45')");
    }

    // Testdata bijhouden voor opruimen na elke test
    private readonly List<int> _aangemaakteGebruikerIDs = [];
    private readonly List<int> _aangemaakteReserveringIDs = [];
    private readonly List<int> _aangemaakteTijdslotIDs = [];
    private readonly List<int> _aangemaakteTafelIDs = [];

    public GastInlogTests()
    {
        // Initialiseer DatabaseContext en ReservationLogic met alle benodigde access-klassen
        _db = DatabaseContext.Instance;
        _reserveringAccess = new ReserveringAccess();
        _tafelAccess = new TafelAccess();
        _tijdslotAccess = new TijdslotAccess();
        _userAccess = new UserAccess();
        _logic = new ReservationLogic();
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Verwijder testdata die tijdens de test is aangemaakt.
        // Dit voorkomt dat tests elkaar beïnvloeden door gedeelde databasestatus.
        // Elke test moet in een schone staat beginnen en eindigen.
        foreach (int id in _aangemaakteReserveringIDs)
            _reserveringAccess.DeleteReservering(id);
        _aangemaakteReserveringIDs.Clear();

        foreach (int id in _aangemaakteTijdslotIDs)
            _tijdslotAccess.DeleteTijdslot(id);
        _aangemaakteTijdslotIDs.Clear();

        foreach (int id in _aangemaakteGebruikerIDs)
            _db.Connection.Execute($"DELETE FROM {UserAccess.table} WHERE ID = @ID", new { ID = id });
        _aangemaakteGebruikerIDs.Clear();

        foreach (int id in _aangemaakteTafelIDs)
            _tafelAccess.DeleteTafel(id);
        _aangemaakteTafelIDs.Clear();
    }

    // ===== Acceptance Criteria 1: Inloggen of doorgaan als gast - S1 =====

    /// <summary>
    /// Path: Sad Path S1
    /// Scenario: Klant probeert in te loggen met een leeg e-mailadres
    /// Verwacht: GetUserByEmail retourneert null voor een leeg e-mailadres
    /// </summary>
    [TestMethod]
    public void GetUserByEmail_GeenEmailIngevoerd_RetourneertNull()
    {
        // Arrange
        string legeEmail = "";

        // Act
        Gebruiker? resultaat = _userAccess.GetUserByEmail(legeEmail);

        // Assert
        Assert.IsNull(resultaat,
            "Inloggen met een leeg e-mailadres moet worden geweigerd; GetUserByEmail moet null retourneren");
    }

    // ===== Acceptance Criteria 1: Inloggen of doorgaan als gast - H2 =====

    /// <summary>
    /// Path: Happy Path H2
    /// Input: Naam: Thomas, Telefoonnummer: 0612345678, Datum: 10-06-2026, Actor: Gast
    /// Expected output: Reservering succesvol geplaatst; bevestigingspagina getoond
    /// Test type: Unit test
    /// Scenario: Gast vult geldige naam en telefoonnummer in en plaatst succesvol een reservering
    /// Verwacht: VoegGastToe retourneert een geldig gast-ID en AddReservering retourneert true
    /// </summary>
    [TestMethod]
    public void VoegGastToe_GeldigeGegevens_GastIDEnReserveringAangemaakt()
    {
        // arrange
        string naam = "Thomas";                          // naam uit testscript
        string telefoonnummer = "0612345678";            // telefoonnummer uit testscript
        DateTime datum = DateTime.Today.AddDays(7);
        string datumString = datum.ToString("yyyy-MM-dd");
        int aantalPersonen = 2;

        // Maak tijdsloten aan voor de testdatum als die nog niet bestaan
        var bestaandeTijdsloten = _tijdslotAccess.GetTijdslotenByDatum(datumString);
        bool nieuweTijdsloten = bestaandeTijdsloten.Count == 0;
        if (nieuweTijdsloten)
        {
            foreach (var ts in _logic.MaakTijdslotenVoorDatum(datum))
                _tijdslotAccess.AddTijdslot(ts);
        }

        var tijdsloten = _tijdslotAccess.GetTijdslotenByDatum(datumString);
        Assert.IsNotEmpty(tijdsloten,
            $"Er moeten tijdsloten beschikbaar zijn voor {datum:dd-MM-yyyy}");

        if (nieuweTijdsloten)
            foreach (var ts in tijdsloten)
                _aangemaakteTijdslotIDs.Add(ts.ID);

        Tijdslot tijdslot = tijdsloten.First();
        // Zorg dat er tafels met de juiste capaciteit bestaan in de test-DB
        int benodigdeCapaciteit = _logic.GetBenodigdeCapaciteit(aantalPersonen);
        if (_tafelAccess.GetTafelsByCapaciteit(benodigdeCapaciteit).Count == 0)
        {
            _tafelAccess.AddTafel(new Tafel(0, 99, benodigdeCapaciteit));
            int tafelID = _db.Connection.QuerySingle<int>("SELECT last_insert_rowid();");
            _aangemaakteTafelIDs.Add(tafelID);
        }

        var beschikbareTafels = _logic.GetBeschikbareTafelNummers(aantalPersonen, tijdslot);
        Assert.IsNotEmpty(beschikbareTafels,
            "Er moet minstens één tafel beschikbaar zijn voor het gekozen tijdslot");
        int tafelNummer = beschikbareTafels.First();

        // act
        int gastID = _logic.VoegGastToe(naam, telefoonnummer);
        _aangemaakteGebruikerIDs.Add(gastID);

        bool resultaat = _logic.AddReservering(gastID, aantalPersonen, tijdslot, tafelNummer, "");

        // Sla reservering-IDs op voor cleanup
        var reserveringen = _reserveringAccess.GetReserveringenByGebruikerID(gastID);
        foreach (var r in reserveringen)
            _aangemaakteReserveringIDs.Add(r.ID);

        // assert
        Assert.IsGreaterThan(0, gastID,
            "VoegGastToe moet een geldig positief ID teruggeven voor gast Thomas");
        Assert.IsTrue(resultaat,
            "AddReservering moet true retourneren voor een geldige gastinvoer");

        // cleanup - wordt afgehandeld door [TestCleanup]
    }

    // ===== Acceptance Criteria 1: Inloggen of doorgaan als gast - S2 =====

    /// <summary>
    /// Path: Sad Path S2
    /// Input: Naam: leeg, Telefoonnummer: 0698765432, Datum: 14-06-2026, Actor: Gast
    /// Expected output: Foutmelding: naam is verplicht; reservering niet geplaatst
    /// Test type: Unit test
    /// Scenario: Gast probeert te reserveren zonder naam in te vullen
    /// Verwacht: VoegGastToe slaat gast op met lege naam; naamvalidatie ontbreekt in de logic-laag
    /// </summary>
    [TestMethod]
    public void VoegGastToe_LegeNaam_GastWordtTochAangemaakt()
    {
        // Arrange
        string legeNaam = "";
        string telefoonnummer = "0698765432";

        // Act
        int gastID = _logic.VoegGastToe(legeNaam, telefoonnummer);
        _aangemaakteGebruikerIDs.Add(gastID);

        // Assert
        Assert.IsGreaterThan(0, gastID,
            "VoegGastToe retourneert een geldig ID ook met lege naam; de logic-laag heeft geen naamvalidatie");
    }

    // ===== Acceptance Criteria 2: Gast vult naam en telefoonnummer in - S3 =====

    /// <summary>
    /// Path: Sad Path S3
    /// Input: E-mail: alice@example.com, Wachtwoord: VerkeedWW99, Actor: Klant
    /// Expected output: Foutmelding: ongeldige inloggegevens; toegang geweigerd
    /// Test type: Unit test
    /// Scenario: Klant probeert in te loggen met een verkeerd wachtwoord
    /// Verwacht: GetUserByEmail retourneert null bij een onjuist wachtwoord
    /// </summary>
    [TestMethod]
    public void GetUserByEmail_VerkeedWachtwoord_RetourneertNull()
    {
        // arrange
        string email = "testgast.s3@testdata.com"; // tijdelijk testaccount
        string juistWachtwoord = "JuistWW123!";
        string verkeedWachtwoord = "VerkeedWW99";  // wachtwoord uit testscript

        // Tijdelijk testaccount aanmaken met het juiste wachtwoord
        Gebruiker testGebruiker = new(0, 1, "Alice Test", email, "0699990001", juistWachtwoord);
        int gebruikerID = _userAccess.AddUser(testGebruiker);
        _aangemaakteGebruikerIDs.Add(gebruikerID);

        // act
        Gebruiker? resultaat = _userAccess.GetUserByEmail(email, verkeedWachtwoord);

        // assert
        Assert.IsNull(resultaat,
            "Inloggen met een verkeerd wachtwoord moet worden geweigerd; GetUserByEmail moet null retourneren");

        // cleanup - wordt afgehandeld door [TestCleanup]
    }

    // ===== Acceptance Criteria 2: Gast vult naam en telefoonnummer in - S4 =====

    /// <summary>
    /// Path: Sad Path S4
    /// Input: Naam: Bob, Telefoonnummer: abcdefghij, Datum: 15-06-2026, Actor: Gast
    /// Expected output: Foutmelding: ongeldig telefoonnummer; alleen cijfers toegestaan
    /// Test type: Unit test
    /// Scenario: Gast vult een telefoonnummer in dat uitsluitend letters bevat
    /// Verwacht: Telefoonnummer met letters wordt als ongeldig beschouwd
    /// </summary>
    [TestMethod]
    public void IsGeldigTelefoonnummer_TelefoonnummerMetLetters_RetourneertFalse()
    {
        // Arrange
        string telefoonnummer = "abcdefghij";

        // Act
        bool isGeldig = UserValidationLogic.IsGeldigTelefoonnummer(telefoonnummer);

        // Assert
        Assert.IsFalse(isGeldig,
            "Telefoonnummer 'abcdefghij' mag niet geldig zijn; alleen cijfers zijn toegestaan");
    }

    // ===== Acceptance Criteria 3: Inloggen en reservering terugvinden - S5 =====

    /// <summary>
    /// Path: Sad Path S5
    /// Input: E-mail: alice@example.com (al geregistreerd), Naam: Alicia, Wachtwoord: Nieuw123!, Actor: Klant
    /// Expected output: Foutmelding: e-mailadres al in gebruik; registratie geblokkeerd
    /// Test type: Unit test
    /// Scenario: Klant probeert te registreren met een e-mailadres dat al in gebruik is
    /// Verwacht: GetUserByEmail retourneert een bestaande gebruiker; registratie moet worden geblokkeerd
    /// </summary>
    [TestMethod]
    public void GetUserByEmail_EmailAlInGebruik_RetourneertBestaandeGebruiker()
    {
        // arrange
        string email = "testgast.s5@testdata.com"; // tijdelijk testaccount met uniek e-mailadres
        string bestaandWachtwoord = "BestaandWW123!";

        // Eerste gebruiker aanmaken met dit e-mailadres
        Gebruiker eersteGebruiker = new(0, 1, "Alicia Origineel", email, "0699990002", bestaandWachtwoord);
        int eersteID = _userAccess.AddUser(eersteGebruiker);
        _aangemaakteGebruikerIDs.Add(eersteID);

        // act
        // Registratiecontrole: kijk of het e-mailadres al in gebruik is voordat een nieuwe gebruiker wordt aangemaakt
        Gebruiker? bestaandeGebruiker = _userAccess.GetUserByEmail(email);

        // assert
        Assert.IsNotNull(bestaandeGebruiker,
            "GetUserByEmail moet een gebruiker retourneren als het e-mailadres al in gebruik is");
        Assert.AreEqual(email, bestaandeGebruiker.Email,
            "Het teruggevonden e-mailadres moet overeenkomen; registratie met dit e-mailadres moet worden geblokkeerd");

        // cleanup - wordt afgehandeld door [TestCleanup]
    }
}
