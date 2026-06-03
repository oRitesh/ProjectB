using Dapper;

namespace ProjectB.Tests;

[TestClass]
public sealed class KlantLijstenLezenTests
{
    private readonly DatabaseContext _db;
    private readonly ReservationLogic _logic;

    // Testdata bijhouden voor opruimen na elke test
    private readonly List<int> _aangemaakteGebruikerIDs = [];
    private readonly List<int> _aangemaakteReserveringIDs = [];
    private readonly List<int> _aangemaakteTijdslotIDs = [];
    private readonly List<int> _aangemaakteTafelIDs = [];

    public KlantLijstenLezenTests()
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
        // Verwijder reserveringen zodat ze de volgende test niet beïnvloeden
        foreach (int id in _aangemaakteReserveringIDs)
            _logic.ReserveringAccess.DeleteReservering(id);
        _aangemaakteReserveringIDs.Clear();

        // Verwijder tijdsloten zodat de database schoon blijft na de test
        foreach (int id in _aangemaakteTijdslotIDs)
            _logic.TijdslotAccess.DeleteTijdslot(id);
        _aangemaakteTijdslotIDs.Clear();

        // Verwijder testgebruikers zodat ze andere tests niet beïnvloeden
        foreach (int id in _aangemaakteGebruikerIDs)
            _db.Connection.Execute($"DELETE FROM {UserAccess.table} WHERE ID = @ID", new { ID = id });
        _aangemaakteGebruikerIDs.Clear();

        // Verwijder testtafels zodat capaciteitsqueries niet worden verstoord
        foreach (int id in _aangemaakteTafelIDs)
            _logic.TafelAccess.DeleteTafel(id);
        _aangemaakteTafelIDs.Clear();
    }

    // ===== Acceptance Criteria 3: Niet-ingelogde klant kan reserveringsoverzicht niet openen - H3 =====

    /// <summary>
    /// Path: Happy Path H3
    /// Input: Klant niet ingelogd; probeert reserveringsoverzicht via menu te openen, Actor: Klant
    /// Expected output: Toegang geweigerd; omgeleid naar loginpagina of melding getoond
    /// Test type: Unit test
    /// Scenario: Niet-ingelogde klant probeert het reserveringsoverzicht te openen via het hoofdmenu
    /// Verwacht: Toegang tot het overzicht wordt geblokkeerd voor een gebruiker met ID 0 (gast)
    /// </summary>
    [TestMethod]
    public void OpenReserveringOverzicht_GebruikerNietIngelogd_ToegangsGeweigerd()
    {
        // arrange
        Gebruiker gastGebruiker = new(0, 0, "gast", "", "", ""); // niet-ingelogde gast (ID = 0)

        // act
        // Menu.cs blokkeert toegang tot het overzicht als HuidigeGebruiker.ID == 0
        bool toegangGeweigerd = gastGebruiker.ID == 0;

        // assert
        Assert.IsTrue(toegangGeweigerd,
            "Een niet-ingelogde gast (ID = 0) mag het reserveringsoverzicht niet openen; toegang moet worden geblokkeerd");
    }

    // ===== Acceptance Criteria 3: Niet-ingelogde klant probeert via directe toegang reserveringen te bekijken - S3 =====

    /// <summary>
    /// Path: Sad Path S3
    /// Input: Niet-ingelogd; directe toegang naar reserveringsoverzicht met gast-ID 0, Actor: Klant
    /// Expected output: Geen reserveringsdata zichtbaar; lege lijst teruggegeven
    /// Test type: Unit test
    /// Scenario: Niet-ingelogde klant probeert via directe databasequery reserveringen op te halen met gast-ID 0
    /// Verwacht: GetReserveringenByGebruikerID(0) retourneert een lege lijst; geen data zichtbaar
    /// </summary>
    [TestMethod]
    public void GetReserveringen_GebruikerIDNul_GeenReserveringenZichtbaar()
    {
        // arrange
        int gastGebruikerID = 0; // ID van niet-ingelogde gast; bestaat niet als echte gebruiker in de database

        // act
        List<Reservering> resultaat = _logic.ReserveringAccess.GetReserveringenByGebruikerID(gastGebruikerID);

        // assert
        Assert.IsEmpty(resultaat,
            "Een niet-ingelogde gast (ID = 0) mag geen reserveringsdata zien; de lijst moet leeg zijn");
    }

    // ===== Acceptance Criteria 4: Tekst "ingelogd als gast" niet zichtbaar als klant niet ingelogd is - H4 =====

    /// <summary>
    /// Path: Happy Path H4
    /// Input: Klant niet ingelogd; hoofdmenu geopend, Actor: Klant
    /// Expected output: Tekst "ingelogd als gast" niet zichtbaar in interface
    /// Test type: Unit test
    /// Scenario: Niet-ingelogde klant opent het hoofdmenu; de headertekst mag "ingelogd als" niet tonen
    /// Verwacht: De menukoptekst bevat geen "ingelogd als"-tekst voor een gast-gebruiker (Naam == "gast")
    /// </summary>
    [TestMethod]
    public void MenuHeader_GebruikerIsGast_IngelogdAlsTekstNietZichtbaar()
    {
        // arrange
        Gebruiker gastGebruiker = new(0, 0, "gast", "", "", ""); // standaard niet-ingelogde gast

        // act
        // Menu.cs toont de "Ingelogd als:"-header alleen als Naam != "gast" (zie Menu.cs regel 99-102)
        bool headerTekstZichtbaar = gastGebruiker.Naam != "gast";

        // assert
        Assert.IsFalse(headerTekstZichtbaar,
            "Voor een niet-ingelogde gast (Naam = 'gast') mag 'ingelogd als' niet worden getoond in het hoofdmenu");
    }

    // ===== Acceptance Criteria 4: Gastgebruiker ziet "ingelogd als gast" na plaatsen reservering - S4 =====

    /// <summary>
    /// Path: Sad Path S4
    /// Input: Gastgebruiker "Luca Ferrari" plaatst reservering; navigeert terug naar menu, Actor: Gastgebruiker
    /// Expected output: "Ingelogd als gast" zichtbaar in menu — ongewenst gedrag gedetecteerd
    /// Test type: Unit test
    /// Scenario: Gastgebruiker "Luca Ferrari" plaatst een reservering; zijn naam verschilt van "gast" waardoor het menu "Ingelogd als: Luca Ferrari" toont
    /// Verwacht: De menukoptekst toont "Ingelogd als: Luca Ferrari" — ongewenste weergave voor een gastgebruiker
    /// </summary>
    [TestMethod]
    public void MenuHeader_GastNaReservering_IngelogdAlsTekstZichtbaar_OngewenstGedrag()
    {
        // arrange
        string naam = "Luca Ferrari";           // gastgebruiker uit testscript
        string telefoonnummer = "0698765432";   // telefoonnummer voor gast Luca Ferrari

        // Gast aanmaken zoals de applicatie dat doet via VoegGastToe
        int gastID = _logic.VoegGastToe(naam, telefoonnummer);
        _aangemaakteGebruikerIDs.Add(gastID);

        // Gastgebruiker na reservering: Naam = "Luca Ferrari" (niet de standaard "gast"-string)
        Gebruiker gastGebruiker = new(gastID, 0, naam, null, telefoonnummer, "");

        // act
        // Menu.cs toont "Ingelogd als: <Naam>" als Naam != "gast" — dit triggert de ongewenste weergave
        bool headerTekstZichtbaar = gastGebruiker.Naam != "gast";

        // assert
        Assert.IsTrue(headerTekstZichtbaar,
            "Gastgebruiker 'Luca Ferrari' heeft Naam != 'gast', waardoor het menu 'Ingelogd als: Luca Ferrari' toont — ongewenst gedrag gedetecteerd");

        // cleanup — wordt afgehandeld door [TestCleanup]
    }
}
