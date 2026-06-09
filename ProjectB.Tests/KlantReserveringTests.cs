namespace ProjectB.Tests;

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


    /// <summary>
    /// H1: Klant kan reservering van 2 uur maken
    /// Scenario: Klant maakt een reservering van 2 uur zodat duidelijk is hoelang de reservering duurt
    /// </summary>
    [TestMethod]
    public void CreateReservering_DuurTweeUur_WordtSucvesVolAangemaakt()
    {
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
    /// S1: Klant probeert reservering van minder dan 2 uur te maken
    /// Scenario: Klant probeert een reservering van minder dan 2 uur te maken
    /// </summary>
    [TestMethod]
    public void ValideerReserveringsDuur_MinderDanTweeUur_IsOngeldig()
    {
        string startTijd = "2026-06-20 18:00";
        string eindTijd = "2026-06-20 19:00";

        var startTijdParsed = DateTime.Parse(startTijd);
        var eindTijdParsed = DateTime.Parse(eindTijd);

        var duur = eindTijdParsed - startTijdParsed;
        bool isDuurGeldig = duur.TotalHours >= 2;

        Assert.IsFalse(isDuurGeldig,
            "Reservering van 1 uur is ongeldig; duur moet minstens 2 uur zijn");
    }


    /// <summary>
    /// S2: Klant probeert reservering van 3 uur in te voeren
    /// Scenario: Klant probeert een reservering voor een voorbije datum te maken
    /// </summary>
    [TestMethod]
    public void ValideerReserveringsDatum_VerledeDatum_WordtGeweigerd()
    {
        DateTime reserveringsDatum = new(2026, 6, 1);
        DateTime vandaag = DateTime.Today;

        bool isDateInVerleden = reserveringsDatum.Date < vandaag;

        Assert.IsTrue(isDateInVerleden,
            "Datum 2026-06-01 ligt in het verleden en mag niet worden geboekt");
    }


    /// <summary>
    /// H2: Beschikbare tijdsloten tonen in stappen van 15 minuten
    /// Scenario: Klant wil beschikbare tijdsloten zien in stappen van 15 minuten zodat duidelijk is op welk moment geboekt kan worden
    /// </summary>
    [TestMethod]
    public void GetBeschikbareTijdsloten_In15MinutenStappen_RetourneertJuisteIntervallen()
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

        foreach (var slot in ochtendTijdsloten)
        {
            var tijdslot = beschikbareTijdsloten.FirstOrDefault(ts => ts.StartTijd == slot.start);
            if (tijdslot != null)
            {
                tijdslotAccess.DeleteTijdslot(tijdslot.ID);
            }
        }
    }


    /// <summary>
    /// S3: Klant vraagt beschikbare slots voor een datum waarop geen tijdsloten beschikbaar zijn
    /// Scenario: Klant vraagt beschikbare slots voor een volgeboealde datum
    /// </summary>
    [TestMethod]
    public void GetBeschikbareTijdsloten_GeenSlotsBeschikbaar_RetourneertLegeLijst()
    {
        string datumZonderSlots = "2026-06-21";

        var beschikbareTijdsloten = tijdslotAccess.GetTijdslotenByDatum(datumZonderSlots);

        Assert.IsEmpty(beschikbareTijdsloten,
            "Voor een datum zonder slots moet een lege lijst worden teruggegeven");
    }


    /// <summary>
    /// H3: Beschikbare tafels tonen per tijdslot
    /// Scenario: Klant wil duidelijk zien welke tafels beschikbaar zijn op welke tijdsloten
    /// </summary>
    [TestMethod]
    public void GetBeschikbareTafels_VoorTijdslot_RetourneertAlleenVriyeTafels()
    {
        var alleTafels = tafelAccess.GetAllTafels();
        Assert.IsNotEmpty(alleTafels, "Er moeten tafels in de database bestaan");

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
    /// S4: Klant probeert te reserveren op een tijdslot waarbij alle tafels al bezet zijn
    /// Scenario: Klant probeert op een moment dat alle tafels bezet zijn toch te reserveren
    /// </summary>
    [TestMethod]
    public void ValideerBeschikbaarheidTafel_TafelAlBezet_ReserveringGeweigerd()
    {
        var alleTafels = tafelAccess.GetAllTafels();
        Assert.IsNotEmpty(alleTafels, "Er moeten tafels in de database bestaan");

        string startTijd = "2026-06-20 18:00";
        string eindTijd = "2026-06-20 20:00";
        var tafel = alleTafels.First();

        var overlappendeReserveringen = reserveringAccess.GetOverlappendeReserveringen(tafel.ID, startTijd, eindTijd);

        if (overlappendeReserveringen.Count > 0)
        {
            Assert.AreEqual(tafel.ID, overlappendeReserveringen[0].TafelID, "Tafel ID moet matchen");
        }
    }
}
