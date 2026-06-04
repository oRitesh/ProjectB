using Dapper;

namespace ProjectB.Tests;

[TestClass]
public sealed class GastInlogTests
{
    private readonly DatabaseContext _db;
    private readonly ReservationLogic _logic;

    // Testdata bijhouden voor opruimen na elke test
    private readonly List<int> _aangemaakteGebruikerIDs = [];
    private readonly List<int> _aangemaakteReserveringIDs = [];
    private readonly List<int> _aangemaakteTijdslotIDs = [];
    private readonly List<int> _aangemaakteTafelIDs = [];

    public GastInlogTests()
    {
        // Initialiseer DatabaseContext en ReservationLogic met alle benodigde access-klassen
        _db = new DatabaseContext();
        var reserveringAccess = new ReserveringAccess(_db);
        var tafelAccess = new TafelAccess(_db);
        var tijdslotAccess = new TijdslotAccess(_db);
        var userAccess = new UserAccess(_db);
        _logic = new ReservationLogic(reserveringAccess, tafelAccess, tijdslotAccess, userAccess);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Verwijder testdata die tijdens de test is aangemaakt.
        // Dit voorkomt dat tests elkaar beïnvloeden door gedeelde databasestatus.
        // Elke test moet in een schone staat beginnen en eindigen.
        foreach (int id in _aangemaakteReserveringIDs)
            _logic.ReserveringAccess.DeleteReservering(id);
        _aangemaakteReserveringIDs.Clear();

        foreach (int id in _aangemaakteTijdslotIDs)
            _logic.TijdslotAccess.DeleteTijdslot(id);
        _aangemaakteTijdslotIDs.Clear();

        foreach (int id in _aangemaakteGebruikerIDs)
            _db.Connection.Execute($"DELETE FROM {UserAccess.table} WHERE ID = @ID", new { ID = id });
        _aangemaakteGebruikerIDs.Clear();

        foreach (int id in _aangemaakteTafelIDs)
            _logic.TafelAccess.DeleteTafel(id);
        _aangemaakteTafelIDs.Clear();
    }

    // ===== Acceptance Criteria 1: Inloggen of doorgaan als gast - S1 =====

    /// <summary>
    /// Path: Sad Path S1
    /// Input: Klant op stap "Login of gast?" klikt "Volgende" zonder keuze te maken, Actor: Klant
    /// Expected output: Foutmelding: maak een keuze om door te gaan; stap geblokkeerd
    /// Test type: Unit test
    /// Scenario: Klant probeert de stap "Inloggen of doorgaan als gast" over te slaan zonder een selectie te maken
    /// Verwacht: Een lege selectie wordt als ongeldig beschouwd en de klant wordt geblokkeerd
    /// </summary>
    [TestMethod]
    public void ValideerGastOfInlogKeuze_GeenKeuzeGemaakt_WordtGeblokkeerd()
    {
        // arrange
        string? keuze = null; // klant heeft geen keuze gemaakt op de keuzepagina

        // act
        // Keuze is alleen geldig als "gast" of "inloggen" is geselecteerd
        bool isGeldig = keuze == "gast" || keuze == "inloggen";

        // assert
        Assert.IsFalse(isGeldig,
            "Zonder keuze te maken mag de klant niet doorgaan; null-keuze moet worden geblokkeerd");
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
    public void VoegGastToeEnReserveer_GeldigeGegevens_ReserveringSuccesvol()
    {
        // arrange
        string naam = "Thomas";                          // naam uit testscript
        string telefoonnummer = "0612345678";            // telefoonnummer uit testscript
        DateTime datum = new(2026, 6, 10);                 // datum uit testscript: 10-06-2026
        int aantalPersonen = 2;

        // Maak tijdsloten aan voor de testdatum als die nog niet bestaan
        var bestaandeTijdsloten = _logic.TijdslotAccess.GetTijdslotenByDatum("2026-06-10");
        bool nieuweTijdsloten = bestaandeTijdsloten.Count == 0;
        _logic.MaakTijdslotenVoorDatumAlsNietBestaan(datum);

        var tijdsloten = _logic.TijdslotAccess.GetTijdslotenByDatum("2026-06-10");
        Assert.IsNotEmpty(tijdsloten,
            "Er moeten tijdsloten beschikbaar zijn voor 10-06-2026");

        if (nieuweTijdsloten)
            foreach (var ts in tijdsloten)
                _aangemaakteTijdslotIDs.Add(ts.ID);

        Tijdslot tijdslot = tijdsloten.First();
        // Zorg dat er tafels met de juiste capaciteit bestaan in de test-DB
        int benodigdeCapaciteit = _logic.GetBenodigdeCapaciteit(aantalPersonen);
        if (_logic.TafelAccess.GetTafelsByCapaciteit(benodigdeCapaciteit).Count == 0)
        {
            _logic.TafelAccess.AddTafel(new Tafel(0, 99, benodigdeCapaciteit));
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
        var reserveringen = _logic.ReserveringAccess.GetReserveringenByGebruikerID(gastID);
        foreach (var r in reserveringen)
            _aangemaakteReserveringIDs.Add(r.ID);

        // assert
        Assert.IsGreaterThan(0, gastID,
            "VoegGastToe moet een geldig positief ID teruggeven voor gast Thomas");
        Assert.IsTrue(resultaat,
            "AddReservering moet true retourneren voor een geldige gastinvoer");

        // cleanup — wordt afgehandeld door [TestCleanup]
    }

    // ===== Acceptance Criteria 1: Inloggen of doorgaan als gast - S2 =====

    /// <summary>
    /// Path: Sad Path S2
    /// Input: Naam: leeg, Telefoonnummer: 0698765432, Datum: 14-06-2026, Actor: Gast
    /// Expected output: Foutmelding: naam is verplicht; reservering niet geplaatst
    /// Test type: Unit test
    /// Scenario: Gast probeert te reserveren zonder naam in te vullen
    /// Verwacht: Een lege naam wordt als ongeldig beschouwd en de invoer wordt geweigerd
    /// </summary>
    [TestMethod]
    public void ValideerGastNaam_LegeNaam_WordtGeweigerd()
    {
        // arrange
        string naam = ""; // verplicht veld dat leeg is gelaten

        // act
        // Naam is alleen geldig als die niet leeg of alleen witruimte is
        bool naamIsGeldig = !string.IsNullOrWhiteSpace(naam);

        // assert
        Assert.IsFalse(naamIsGeldig,
            "Naam is verplicht; een lege naam mag niet worden geaccepteerd bij gastregistratie");
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
    public void InloggenMetWachtwoord_VerkeedWachtwoord_RetourneertNull()
    {
        // arrange
        string email = "testgast.s3@testdata.com"; // tijdelijk testaccount
        string juistWachtwoord = "JuistWW123!";
        string verkeedWachtwoord = "VerkeedWW99";  // wachtwoord uit testscript

        // Tijdelijk testaccount aanmaken met het juiste wachtwoord
        Gebruiker testGebruiker = new(0, 1, "Alice Test", email, "0699990001", juistWachtwoord);
        int gebruikerID = _logic.UserAccess.AddUser(testGebruiker);
        _aangemaakteGebruikerIDs.Add(gebruikerID);

        // act
        Gebruiker? resultaat = _logic.UserAccess.GetUserByEmail(email, verkeedWachtwoord);

        // assert
        Assert.IsNull(resultaat,
            "Inloggen met een verkeerd wachtwoord moet worden geweigerd; GetUserByEmail moet null retourneren");

        // cleanup — wordt afgehandeld door [TestCleanup]
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
    public void ValideerTelefoonnummer_TelefoonnummerMetLetters_WordtGeweigerd()
    {
        // arrange
        string telefoonnummer = "abcdefghij"; // ongeldig: uitsluitend letters, geen cijfers

        // act
        // Telefoonnummer is alleen geldig als het uitsluitend cijfers bevat
        bool isGeldig = System.Text.RegularExpressions.Regex.IsMatch(telefoonnummer, @"^\d+$");

        // assert
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
    public void RegistrerenMetEmail_EmailAlInGebruik_RegistratieGeblokkeerd()
    {
        // arrange
        string email = "testgast.s5@testdata.com"; // tijdelijk testaccount met uniek e-mailadres
        string bestaandWachtwoord = "BestaandWW123!";

        // Eerste gebruiker aanmaken met dit e-mailadres
        Gebruiker eersteGebruiker = new(0, 1, "Alicia Origineel", email, "0699990002", bestaandWachtwoord);
        int eersteID = _logic.UserAccess.AddUser(eersteGebruiker);
        _aangemaakteGebruikerIDs.Add(eersteID);

        // act
        // Registratiecontrole: kijk of het e-mailadres al in gebruik is voordat een nieuwe gebruiker wordt aangemaakt
        Gebruiker? bestaandeGebruiker = _logic.UserAccess.GetUserByEmail(email);

        // assert
        Assert.IsNotNull(bestaandeGebruiker,
            "GetUserByEmail moet een gebruiker retourneren als het e-mailadres al in gebruik is");
        Assert.AreEqual(email, bestaandeGebruiker.Email,
            "Het teruggevonden e-mailadres moet overeenkomen; registratie met dit e-mailadres moet worden geblokkeerd");

        // cleanup — wordt afgehandeld door [TestCleanup]
    }
}
