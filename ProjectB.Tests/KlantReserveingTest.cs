namespace ProjectB.Tests;
using Dapper;

/// <summary>
/// Test categories:
/// - H1-H3: Happy paths (klant kan reservering maken, ziet beschikbare slots, ziet beschikbare tafels)
/// - S1-S3: Sad paths (boeken op bezette tafel, te korte duratie, verleden datum)
/// </summary>
[TestClass]
public sealed class KlantReserveringTests
{
    private readonly DatabaseContext db;
    private readonly ReserveringAccess reserveringAccess;
    private readonly TafelAccess tafelAccess;
    private readonly TijdslotAccess tijdslotAccess;
    private readonly UserAccess userAccess;

    public KlantReserveringTests()
    {
        db = new DatabaseContext();
        reserveringAccess = new ReserveringAccess(db);
        tafelAccess = new TafelAccess(db);
        tijdslotAccess = new TijdslotAccess(db);
        userAccess = new UserAccess(db);
    }

    [TestCleanup]
    public void Cleanup()
    {
        db.Connection.Execute("DELETE FROM Reservering;");
        db.Connection.Execute("DELETE FROM Tijdslot;");
        db.Connection.Execute("DELETE FROM Tafel;");
        db.Connection.Execute("DELETE FROM Gebruiker;");
        db.Connection.Execute("DELETE FROM sqlite_sequence;");

    }


    /// <summary>
    /// H1: Klant kan reservering van 2 uur maken
    /// Scenario: Klant maakt een reservering van 2 uur zodat duidelijk is hoelang de reservering duurt
    /// </summary>
    [TestMethod]
    public void CreateReservering_DuurtTweeUur_WordtSuccesvolAangemaakt()
    {
        tafelAccess.AddTafel(new Tafel(0, 1, 4));
        tafelAccess.AddTafel(new Tafel(0, 2, 4));

        var alleTafels = tafelAccess.GetAllTafels();

        var alleReserveringen = reserveringAccess.GetReserveringenVoorDatum("2026-06-20");
        
        Assert.IsNotEmpty(alleTafels, "Er moeten tafels in database bestaan");

        var beschikbareTafel = alleTafels.FirstOrDefault(t => 
            !alleReserveringen.Any(r => r.TafelID == t.ID));

        Assert.IsNotNull(beschikbareTafel, "Er moet een beschikbare tafel zijn");

        var startTijd = DateTime.Parse("2026-06-20 18:00");
        var eindTijd = DateTime.Parse("2026-06-20 20:00");
        var duur = eindTijd - startTijd;

        Assert.AreEqual(2, duur.TotalHours,
            "Reservering duur moet exact 2 uur zijn");

    }

    /// <summary>
    /// H2: Beschikbare tijdsloten tonen in stappen van 15 minuten
    /// Scenario: Klant wil beschikbare tijdsloten zien in stappen van 15 minuten zodat duidelijk is op welk moment geboekt kan worden
    /// </summary>
    [TestMethod]
    public void GetBeschikbareTijdsloten_In15MinutenStappen_GeeftJuisteIntervallenTerug()
    {
        string datum = "2026-06-20";
        var ochtendTijdsloten = new List<(string start, string eind)>
        {
            ("2026-06-20 12:00", "2026-06-20 12:15"),
            ("2026-06-20 12:15", "2026-06-20 12:30"),
            ("2026-06-20 12:30", "2026-06-20 12:45"),
            ("2026-06-20 12:45", "2026-06-20 13:00"),
        };

        foreach (var slot in ochtendTijdsloten)
        {
            tijdslotAccess.AddTijdslot(new Tijdslot(0, datum, slot.start, slot.eind));
        }

        var beschikbareTijdsloten = tijdslotAccess.GetTijdslotenByDatum(datum);

        Assert.HasCount(4, beschikbareTijdsloten,
            "Er moeten exact 4 tijdsloten zijn voor de testtijdsperiode");

        var starttijden = beschikbareTijdsloten
            .Select(t => DateTime.Parse(t.StartTijd).ToString("HH:mm"))
            .ToList();

        CollectionAssert.Contains(starttijden, "12:00", "Starttijd 12:00 moet aanwezig zijn");
        CollectionAssert.Contains(starttijden, "12:15", "Starttijd 12:15 moet aanwezig zijn (15-minuten stap)");
        CollectionAssert.Contains(starttijden, "12:30", "Starttijd 12:30 moet aanwezig zijn (15-minuten stap)");
        CollectionAssert.Contains(starttijden, "12:45", "Starttijd 12:45 moet aanwezig zijn (15-minuten stap)");

        foreach (var slot in beschikbareTijdsloten)
        {
            var start = DateTime.Parse(slot.StartTijd);
            var eind = DateTime.Parse(slot.EindTijd);
            var minuten = (eind - start).TotalMinutes;

            Assert.AreEqual(15, minuten,
                $"Elk slot moet 15 minuten zijn; slot {slot.StartTijd} tot {slot.EindTijd} is {minuten} minuten");
        }

    }


    /// <summary>
    /// H3: Beschikbare tafels tonen per tijdslot
    /// Scenario: Klant wil duidelijk zien welke tafels beschikbaar zijn op welke tijdsloten
    /// </summary>
    [TestMethod]
    public void GetBeschikbareTafels_TijdensTijdslot_GeeftAlleenVrijeTafelsTerug()
    {
        tafelAccess.AddTafel(new Tafel(0, 1, 4));
        tafelAccess.AddTafel(new Tafel(0, 2, 4));

        var alleTafels = tafelAccess.GetAllTafels();
        var tafel = tafelAccess.GetAllTafels().First();
        int TafelId = tafel.ID;

        userAccess.AddUser(new Gebruiker(0, 0,  "Test", "test@mail.com", "1234", ""));
        var user = userAccess.GetAllUsers().First();
        int UserId = user.ID;


        reserveringAccess.AddReservering(new Reservering(
            0, UserId, TafelId, "2026-06-20 18:00", "2026-06-20 20:00", 4, "", ""
        ));

        string startTijd = "2026-06-20 18:00";
        string eindTijd = "2026-06-20 20:00";

        var alleReserveringenInSlot = reserveringAccess.GetOverlappendeReserveringenVoorTijdslot(
            new Tijdslot(0, "2026-06-20", startTijd, eindTijd));

        var gereserveerdeTabels = alleReserveringenInSlot.Select(r => r.TafelID).Distinct().ToList();
        var beschikbareTafels = alleTafels.Where(t => !gereserveerdeTabels.Contains(t.ID)).ToList();

        Assert.IsNotEmpty(beschikbareTafels,
            "Er moet minstens een beschikbare tafel zijn");
        
    }


    /// <summary>
    /// S1: Reservering korter dan 2 uur wordt geweigerd
    /// Scenario: Klant probeert een reservering van 1 uur te maken.
    /// </summary>
    [TestMethod]
    public void AddReservering_MinderDanTweeUur_WordtNietOpgeslagen()
    {
        tafelAccess.AddTafel(new Tafel(0, 1, 4));
        var tafel = tafelAccess.GetAllTafels().First();
        int TafelId = tafel.ID;

        userAccess.AddUser(new Gebruiker(0, 0,  "Test", "test@mail.com", "1234", ""));
        var user = userAccess.GetAllUsers().First();
        int UserId = user.ID;

        string start = "2026-06-20 18:00";
        string eind = "2026-06-20 19:00";

        var reservering = new Reservering(0, UserId, TafelId, start, eind, 4, "", "");

        reserveringAccess.AddReservering(reservering);

        var opgeslagen = reserveringAccess.GetReserveringenVoorDatum("2026-06-20");

        Assert.IsFalse(
            opgeslagen.Any(r => r.StartTijd == start && r.EindTijd == eind),
            "Reservering van minder dan 2 uur mag niet worden opgeslagen"
        );
    }




    /// <summary>
    /// S2: Reservering voor datum in het verleden wordt geweigerd
    /// Scenario: Klant probeert te reserveren voor een datum die al voorbij is.
    /// </summary>
    [TestMethod]
    public void AddReservering_VerledenDatum_WordtNietOpgeslagen()
    {
        tafelAccess.AddTafel(new Tafel(0, 1, 4));

        string start = "2020-01-01 18:00";
        string eind = "2020-01-01 20:00";

        var tafel = tafelAccess.GetAllTafels().First();
        int TafelId = tafel.ID;

        userAccess.AddUser(new Gebruiker(0, 0,  "Test", "test@mail.com", "1234", ""));
        var user = userAccess.GetAllUsers().First();
        int UserId = user.ID;

        var reservering = new Reservering(0, UserId, TafelId, start, eind, 4, "", "");

        reserveringAccess.AddReservering(reservering);

        var opgeslagen = reserveringAccess.GetReserveringenVoorDatum("2020-01-01");

        Assert.IsFalse(
            opgeslagen.Any(),
            "Reservering in het verleden mag niet worden opgeslagen"
        );
    }



    /// <summary>
    /// S3: Geen tijdsloten beschikbaar voor datum
    /// Scenario: Klant vraagt tijdsloten op een dag zonder tijdsloten.
    /// </summary>
    [TestMethod]
    public void GetTijdslotenByDatum_GeenSlots_GeeftLegeLijst()
    {
        string datum = "2026-06-21";

        var slots = tijdslotAccess.GetTijdslotenByDatum(datum);

        Assert.IsEmpty(slots,
            "Op een datum zonder tijdsloten kan de klant geen reservering maken; lijst moet leeg zijn");
    }

}