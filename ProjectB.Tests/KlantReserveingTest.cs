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

    private readonly OpeningsTijdenAccess openingstijdenAccess;

    public KlantReserveringTests()
    {
        db = new DatabaseContext();
        reserveringAccess = new ReserveringAccess(db);
        tafelAccess = new TafelAccess(db);
        tijdslotAccess = new TijdslotAccess(db);
        userAccess = new UserAccess(db);
        openingstijdenAccess = new OpeningsTijdenAccess(db);
    }

    [TestCleanup]
    public void Cleanup()
    {
        db.Connection.Execute("DELETE FROM Reservering;");
        db.Connection.Execute("DELETE FROM Tijdslot;");
        db.Connection.Execute("DELETE FROM Tafel;");
        db.Connection.Execute("DELETE FROM Gebruiker;");
        db.Connection.Execute("DELETE FROM OpeningsTijden;");
        db.Connection.Execute("DELETE FROM sqlite_sequence;");

    }


    /// H1: Klant kan reservering van 2 uur maken
    /// Scenario: Klant maakt een reservering van 2 uur zodat duidelijk is hoelang de reservering duurt
    [TestMethod]
    public void CreateReservering_DuurtTweeUur_WordtSuccesvolAangemaakt()
    {
        // Arrange
        tafelAccess.AddTafel(new Tafel(0, 1, 4));
        var tafel = tafelAccess.GetAllTafels().First();
        int tafelNummer = tafel.TafelNummer;

        userAccess.AddUser(new Gebruiker(0, 0, "Test", "test@mail.com", "1234", ""));
        var gebruiker = userAccess.GetAllUsers().First();
        int gebruikerId = gebruiker.ID;

        string datum = "2026-06-20";
        string start = "2026-06-20 18:00";
        string eind = "2026-06-20 20:00";

        //act   
        tijdslotAccess.AddTijdslot(new Tijdslot(0, datum, start, eind));
        var tijdslot = tijdslotAccess.GetTijdslotenByDatum(datum).First();

        var logic = new ReservationLogic(reserveringAccess, tafelAccess, userAccess);

        bool resultaat = logic.AddReservering(
            gebruikerId,
            4,
            tijdslot,
            tafelNummer,
            ""
        );

        //assert
        Assert.IsTrue(resultaat, "Reservering moet worden opgeslagen");

        var opgeslagen = reserveringAccess.GetReserveringenVoorDatum(datum);
        Assert.AreEqual(1, opgeslagen.Count, "Er moet 1 reservering zijn");

        var r = opgeslagen.First();
        var duur = DateTime.Parse(r.EindTijd) - DateTime.Parse(r.StartTijd);

        Assert.AreEqual(2, duur.TotalHours, "De reservering moet 2 uur duren");
    }


    /// H2: Beschikbare tijdsloten tonen in stappen van 15 minuten
    /// Scenario: Klant wil beschikbare tijdsloten zien in stappen van 15 minuten zodat duidelijk is op welk moment geboekt kan worden
    [TestMethod]
    public void TimeSlotLogic_GenereertTijdslotenVanTweeUur_InStappenVan15Minuten()
    {
        //arrange
        var logic = new TimeSlotLogic(db);

        //act
        var datum = new DateTime(2026, 6, 20);
        var slots = logic.MaakTijdslotenVoorReservering(datum);

        //assert
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
        //arrange
        tafelAccess.AddTafel(new Tafel(0, 1, 4));
        tafelAccess.AddTafel(new Tafel(0, 2, 4));

        var tafels = tafelAccess.GetAllTafels();
        var tafel = tafels.First();
        int tafelNummer = tafel.TafelNummer;

        userAccess.AddUser(new Gebruiker(0, 0, "Test", "test@mail.com", "1234", ""));
        var user = userAccess.GetAllUsers().First();
        int gebruikerId = user.ID;

        string datum = "2026-06-20";
        string start = "2026-06-20 18:00";
        string eind = "2026-06-20 20:00";

        tijdslotAccess.AddTijdslot(new Tijdslot(0, datum, start, eind));
        var tijdslot = tijdslotAccess.GetTijdslotenByDatum(datum).First();

        //act
        var logic = new ReservationLogic(reserveringAccess, tafelAccess, userAccess);

        bool resultaat = logic.AddReservering(
            gebruikerId,
            4,
            tijdslot,
            tafelNummer,
            ""
        );

        //assert
        Assert.IsTrue(resultaat, "Reservering moet worden opgeslagen");

        var overlappende = reserveringAccess.GetOverlappendeReserveringenVoorTijdslot(tijdslot);
        var gereserveerdeTafels = overlappende.Select(r => r.TafelID).ToList();

        var beschikbareTafels = tafels.Where(t => !gereserveerdeTafels.Contains(t.ID)).ToList();

        Assert.IsNotEmpty(beschikbareTafels, "Er moet minstens 1vrije tafel zijn");
    }



    /// <summary>
    /// S1: Reservering korter dan 2 uur wordt geweigerd
    /// Scenario: Klant probeert een reservering van 1 uur te maken.
    /// </summary>
    [TestMethod]
    public void AddReservering_MinderDanTweeUur_WordtNietOpgeslagen()
    {
        //arrange
        tafelAccess.AddTafel(new Tafel(0, 1, 4));
        var tafel = tafelAccess.GetAllTafels().First();
        int tafelNummer = tafel.ID;

        userAccess.AddUser(new Gebruiker(0, 0,  "Test", "test@mail.com", "1234", ""));
        var user = userAccess.GetAllUsers().First();
        int gebruikerId = user.ID;

        string datum = "2026-06-20";
        string start = "2026-06-20 18:00";
        string eind = "2026-06-20 19:00";

        tijdslotAccess.AddTijdslot(new Tijdslot(
            0,
            datum,
            start,
            eind
        ));

        //act
        var tijdslot = tijdslotAccess.GetTijdslotenByDatum(datum).First();


        var logic = new ReservationLogic(reserveringAccess, tafelAccess, userAccess);

        bool resultaat = logic.AddReservering(
            gebruikerId,
            4,
            tijdslot,
            tafelNummer,
            ""
        );

        //assert
        Assert.IsFalse(resultaat, "Reservering van minder dan 2 uur moet worden geweigerd");

        var opgeslagen = reserveringAccess.GetReserveringenVoorDatum("2026-06-20");
        Assert.IsFalse(opgeslagen.Any());
    }




    /// <summary>
    /// S2: Reservering voor datum in het verleden wordt geweigerd
    /// Scenario: Klant probeert te reserveren voor een datum die al voorbij is.
    /// </summary>
    [TestMethod]
    public void AddReservering_VerledenDatum_WordtNietOpgeslagen()
    {
        //arrange
        tafelAccess.AddTafel(new Tafel(0, 1, 4));
        var tafel = tafelAccess.GetAllTafels().First();
        int tafelNummer = tafel.TafelNummer;

        userAccess.AddUser(new Gebruiker(0, 0, "Test", "test@mail.com", "1234", ""));
        var user = userAccess.GetAllUsers().First();
        int gebruikerId = user.ID;

        string datum = "2020-01-01";
        string start = "2020-01-01 18:00";
        string eind = "2020-01-01 20:00";

        tijdslotAccess.AddTijdslot(new Tijdslot(0, datum, start, eind));

        //act
        var tijdslot = tijdslotAccess.GetTijdslotenByDatum(datum).First();

        var logic = new ReservationLogic(reserveringAccess, tafelAccess, userAccess);

        bool resultaat = logic.AddReservering(
            gebruikerId,
            4,
            tijdslot,
            tafelNummer,
            ""
        );

        //assert
        Assert.IsFalse(resultaat, "Reservering in het verleden moet worden geweigerd");

        var opgeslagen = reserveringAccess.GetReserveringenVoorDatum(datum);
        Assert.IsFalse(opgeslagen.Any(), "Er mag geen reservering worden opgeslagen");
    }




    /// <summary>
    /// S3: Geen tijdsloten beschikbaar voor datum
    /// Scenario: Klant vraagt tijdsloten op een dag zonder tijdsloten.
    /// </summary>
    [TestMethod]
    public void GetTijdslotenByDatum_GeenSlots_GeeftLegeLijst()
    {
        //arrange
        string datum = "2026-06-21";

        //act
        var slots = tijdslotAccess.GetTijdslotenByDatum(datum);

        //assert
        Assert.IsEmpty(slots,
            "Op een datum zonder tijdsloten kan de klant geen reservering maken lijst moet leeg zijn");
    }

}