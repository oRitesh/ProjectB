namespace ProjectB.Tests;

using Dapper;

/// Test categories:
/// - H1-H3: Happy paths (klant kan reservering maken, ziet beschikbare slots, ziet beschikbare tafels)
/// - S1-S3: Sad paths (boeken op bezette tafel, te korte duratie, verleden datum)
[TestClass]
public sealed class KlantReserveringTests
{
    private readonly DatabaseContext _db;
    private readonly ReserveringAccess _reserveringAccess;
    private readonly TafelAccess _tafelAccess;
    private readonly TijdslotAccess _tijdslotAccess;
    private readonly UserAccess _userAccess;

    private readonly OpeningsTijdenAccess _openingstijdenAccess;
    private readonly OpeningsDagAccess _openingsdagAccess;

    private readonly List<int> _reserveringIDs = [];
    private readonly List<int> _tijdslotIDs = [];
    private readonly List<int> _tafelIDs = [];
    private readonly List<int> _gebruikerIDs = [];

    public KlantReserveringTests()
    {
        _db = DatabaseContext.Instance;

        _reserveringAccess = new ReserveringAccess();
        _tafelAccess = new TafelAccess();
        _tijdslotAccess = new TijdslotAccess();
        _userAccess = new UserAccess();
        _openingstijdenAccess = new OpeningsTijdenAccess();
        _openingsdagAccess = new OpeningsDagAccess();
    }

    [TestCleanup]
    public void Cleanup()
    {
        foreach (int id in _reserveringIDs)
            _reserveringAccess.DeleteReservering(id);
        _reserveringIDs.Clear();

        foreach (int id in _tijdslotIDs)
            _tijdslotAccess.DeleteTijdslot(id);
        _tijdslotIDs.Clear();

        foreach (int id in _tafelIDs)
            _tafelAccess.DeleteTafel(id);
        _tafelIDs.Clear();

        foreach (int id in _gebruikerIDs)
            _db.Connection.Execute($"DELETE FROM {UserAccess.table} WHERE ID = @ID", new { ID = id });
        _gebruikerIDs.Clear();
    }


    /// H1: Klant kan reservering van 2 uur maken
    /// Scenario: Klant maakt een reservering van 2 uur zodat duidelijk is hoelang de reservering duurt
    /// </summary>
    [TestMethod]
    public void CreateReservering_DuurtTweeUur_WordtSuccesvolAangemaakt()
    {
        _tafelAccess.AddTafel(new Tafel(0, 1, 4));
        int tafelID = _db.Connection.QuerySingle<int>("SELECT last_insert_rowid();");
        _tafelIDs.Add(tafelID);

        var tafel = _tafelAccess.GetAllTafels().First();
        int tafelNummer = tafel.TafelNummer;

        _userAccess.AddUser(new Gebruiker(0, 0, "Test", "test@mail.com", "1234", ""));
        int gebruikerId = _db.Connection.QuerySingle<int>("SELECT last_insert_rowid();");
        _gebruikerIDs.Add(gebruikerId);

        string datum = "2026-06-20";
        string start = "2026-06-20 18:00";
        string eind = "2026-06-20 20:00";

        _tijdslotAccess.AddTijdslot(new Tijdslot(0, datum, start, eind));
        int tijdslotID = _db.Connection.QuerySingle<int>("SELECT last_insert_rowid();");
        _tijdslotIDs.Add(tijdslotID);

        var tijdslot = _tijdslotAccess.GetTijdslotenByDatum(datum).First();

        var logic = new ReservationLogic(_reserveringAccess, _tafelAccess, _userAccess, _openingstijdenAccess, _openingsdagAccess);

        bool resultaat = logic.AddReservering(
            gebruikerId,
            4,
            tijdslot,
            tafelNummer,
            ""
        );

        Assert.IsTrue(resultaat, "Reservering moet worden opgeslagen");

        var opgeslagen = _reserveringAccess.GetReserveringenVoorDatum(datum);
        Assert.AreEqual(1, opgeslagen.Count, "Er moet 1 reservering zijn");

        var r = opgeslagen.First();
        _reserveringIDs.Add(r.ID);

        var duur = DateTime.Parse(r.EindTijd) - DateTime.Parse(r.StartTijd);
        Assert.AreEqual(2, duur.TotalHours, "De reservering moet 2 uur duren");
    }



    /// H2: Beschikbare tijdsloten tonen in stappen van 15 minuten
    /// Scenario: Klant wil beschikbare tijdsloten zien in stappen van 15 minuten zodat duidelijk is op welk moment geboekt kan worden
    [TestMethod]
    public void TimeSlotLogic_GenereertTijdslotenVanTweeUur_InStappenVan15Minuten()
    {
        var logic = new TimeSlotLogic();

        var datum = new DateTime(2026, 6, 20);
        var slots = logic.MaakTijdslotenVoorReservering(datum);

        Assert.IsTrue(slots.All(s =>
            (DateTime.Parse(s.EindTijd) - DateTime.Parse(s.StartTijd)).TotalHours == 2),
            "Elk tijdslot moet 2 uur duren");

        for (int i = 1; i < slots.Count; i++)
        {
            var vorige = DateTime.Parse(slots[i - 1].StartTijd);
            var huidige = DateTime.Parse(slots[i].StartTijd);

            Assert.AreEqual(15, (huidige - vorige).TotalMinutes,
                "Starttijden moeten 15 minuten uit elkaar liggen");
        }
    }



    /// H3: Beschikbare tafels tonen per tijdslot
    /// Scenario: Klant wil duidelijk zien welke tafels beschikbaar zijn op welke tijdsloten
    [TestMethod]
    public void GetBeschikbareTafels_TijdensTijdslot_GeeftAlleenVrijeTafelsTerug()
    {
        _tafelAccess.AddTafel(new Tafel(0, 1, 4));
        int tafelID1 = _db.Connection.QuerySingle<int>("SELECT last_insert_rowid();");
        _tafelIDs.Add(tafelID1);

        _tafelAccess.AddTafel(new Tafel(0, 2, 4));
        int tafelID2 = _db.Connection.QuerySingle<int>("SELECT last_insert_rowid();");
        _tafelIDs.Add(tafelID2);

        var tafels = _tafelAccess.GetAllTafels();
        var tafel = tafels.First();
        int tafelNummer = tafel.TafelNummer;

        _userAccess.AddUser(new Gebruiker(0, 0, "Test", "test@mail.com", "1234", ""));
        int gebruikerId = _db.Connection.QuerySingle<int>("SELECT last_insert_rowid();");
        _gebruikerIDs.Add(gebruikerId);

        string datum = "2026-06-20";
        string start = "2026-06-20 18:00";
        string eind = "2026-06-20 20:00";

        _tijdslotAccess.AddTijdslot(new Tijdslot(0, datum, start, eind));
        int tijdslotID = _db.Connection.QuerySingle<int>("SELECT last_insert_rowid();");
        _tijdslotIDs.Add(tijdslotID);

        var tijdslot = _tijdslotAccess.GetTijdslotenByDatum(datum).First();

        var logic = new ReservationLogic(_reserveringAccess, _tafelAccess, _userAccess, _openingstijdenAccess, _openingsdagAccess);

        bool resultaat = logic.AddReservering(
            gebruikerId,
            4,
            tijdslot,
            tafelNummer,
            ""
        );

        Assert.IsTrue(resultaat, "Reservering moet worden opgeslagen");

        var overlappende = _reserveringAccess.GetOverlappendeReserveringenVoorTijdslot(tijdslot);
        foreach (var r in overlappende)
            _reserveringIDs.Add(r.ID);

        var gereserveerdeTafels = overlappende.Select(r => r.TafelID).ToList();
        var beschikbareTafels = tafels.Where(t => !gereserveerdeTafels.Contains(t.ID)).ToList();

        Assert.IsNotEmpty(beschikbareTafels, "Er moet minstens 1 vrije tafel zijn");
    }



    /// S1: Reservering korter dan 2 uur wordt geweigerd
    /// Scenario: Klant probeert een reservering van 1 uur te maken.
    [TestMethod]
    public void AddReservering_MinderDanTweeUur_WordtNietOpgeslagen()
    {
        _tafelAccess.AddTafel(new Tafel(0, 1, 4));
        int tafelID = _db.Connection.QuerySingle<int>("SELECT last_insert_rowid();");
        _tafelIDs.Add(tafelID);

        var tafel = _tafelAccess.GetAllTafels().First();
        int tafelNummer = tafel.ID;

        _userAccess.AddUser(new Gebruiker(0, 0, "Test", "test@mail.com", "1234", ""));
        int gebruikerId = _db.Connection.QuerySingle<int>("SELECT last_insert_rowid();");
        _gebruikerIDs.Add(gebruikerId);

        string datum = "2026-06-20";
        string start = "2026-06-20 18:00";
        string eind = "2026-06-20 19:00";

        _tijdslotAccess.AddTijdslot(new Tijdslot(0, datum, start, eind));
        int tijdslotID = _db.Connection.QuerySingle<int>("SELECT last_insert_rowid();");
        _tijdslotIDs.Add(tijdslotID);

        var tijdslot = _tijdslotAccess.GetTijdslotenByDatum(datum).First();

        var logic = new ReservationLogic(_reserveringAccess, _tafelAccess, _userAccess, _openingstijdenAccess, _openingsdagAccess);

        bool resultaat = logic.AddReservering(
            gebruikerId,
            4,
            tijdslot,
            tafelNummer,
            ""
        );

        Assert.IsFalse(resultaat, "Reservering van minder dan 2 uur moet worden geweigerd");

        var opgeslagen = _reserveringAccess.GetReserveringenVoorDatum("2026-06-20");
        Assert.IsFalse(opgeslagen.Any());
    }



    /// S2: Reservering voor datum in het verleden wordt geweigerd
    /// Scenario: Klant probeert te reserveren voor een datum die al voorbij is.
    [TestMethod]
    public void AddReservering_VerledenDatum_WordtNietOpgeslagen()
    {
        _tafelAccess.AddTafel(new Tafel(0, 1, 4));
        int tafelID = _db.Connection.QuerySingle<int>("SELECT last_insert_rowid();");
        _tafelIDs.Add(tafelID);

        var tafel = _tafelAccess.GetAllTafels().First();
        int tafelNummer = tafel.ID;

        _userAccess.AddUser(new Gebruiker(0, 0, "Test", "test@mail.com", "1234", ""));
        int gebruikerId = _db.Connection.QuerySingle<int>("SELECT last_insert_rowid();");
        _gebruikerIDs.Add(gebruikerId);

        string datum = "2020-01-01";
        string start = "2020-01-01 18:00";
        string eind = "2020-01-01 20:00";

        _tijdslotAccess.AddTijdslot(new Tijdslot(0, datum, start, eind));
        int tijdslotID = _db.Connection.QuerySingle<int>("SELECT last_insert_rowid();");
        _tijdslotIDs.Add(tijdslotID);

        var tijdslot = _tijdslotAccess.GetTijdslotenByDatum(datum).First();

        var logic = new ReservationLogic(_reserveringAccess, _tafelAccess, _userAccess, _openingstijdenAccess, _openingsdagAccess);

        bool resultaat = logic.AddReservering(
            gebruikerId,
            4,
            tijdslot,
            tafelNummer,
            ""
        );

        Assert.IsFalse(resultaat, "Reservering in het verleden moet worden geweigerd");

        var opgeslagen = _reserveringAccess.GetReserveringenVoorDatum(datum);
        Assert.IsFalse(opgeslagen.Any(), "Er mag geen reservering worden opgeslagen");
    }



    /// S3: Geen tijdsloten beschikbaar voor datum
    /// Scenario: Klant vraagt tijdsloten op een dag zonder tijdsloten.
    [TestMethod]
    public void GetTijdslotenByDatum_GeenSlots_GeeftLegeLijst()
    {
        string datum = "2026-06-21";

        var slots = _tijdslotAccess.GetTijdslotenByDatum(datum);

        Assert.IsEmpty(slots,
            "Op een datum zonder tijdsloten kan de klant geen reservering maken lijst moet leeg zijn");
    }
}
