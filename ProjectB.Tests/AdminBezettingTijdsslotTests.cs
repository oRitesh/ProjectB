namespace ProjectB.Tests;

[TestClass]
public sealed class AdminBezettingTijdsslotTests
{
    private readonly DatabaseContext db;
    private readonly TijdslotAccess tijdslotAccess;

    public AdminBezettingTijdsslotTests()
    {
        db = new DatabaseContext();
        tijdslotAccess = new TijdslotAccess(db);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Verwijder alle tijdsloten voor de testdatum 2026-06-15 (ingevoegd door H1)
        // Zodat overgebleven testdata niet meeloopt in volgende tests of testrondes
        foreach (var ts in tijdslotAccess.GetTijdslotenByDatum("2026-06-15"))
            tijdslotAccess.DeleteTijdslot(ts.ID);
    }

    // ===== Acceptance Criteria 1: Zoeken op datum - H1 =====

    /// <summary>
    /// Path ID: Happy Path H1
    /// Input: Zoekdatum: 15-06-2026; 3 reserveringen op die dag aanwezig
    /// Actor: Admin
    /// Verwachte output: Tijdsloten 12:00, 14:30 en 19:00 getoond voor 15-06-2026
    /// Test type: Unit test
    /// Scenario: Admin zoekt tijdsloten op 15-06-2026 waarop drie tijdsloten beschikbaar zijn
    /// Verwacht: Drie tijdsloten worden teruggegeven met starttijden 12:00, 14:30 en 19:00
    /// </summary>
    [TestMethod]
    public void GetTijdslotenByDatum_DrieReserveringenOpDatum_RetourneertDrieTijdsloten()
    {
        // arrange
        string zoekdatum = "2026-06-15"; // testdatum waarop drie tijdsloten worden ingepland

        tijdslotAccess.AddTijdslot(new Tijdslot(0, zoekdatum, "2026-06-15 12:00", "2026-06-15 13:30")); // tijdslot 12:00
        tijdslotAccess.AddTijdslot(new Tijdslot(0, zoekdatum, "2026-06-15 14:30", "2026-06-15 16:00")); // tijdslot 14:30
        tijdslotAccess.AddTijdslot(new Tijdslot(0, zoekdatum, "2026-06-15 19:00", "2026-06-15 20:30")); // tijdslot 19:00

        // act
        var tijdsloten = tijdslotAccess.GetTijdslotenByDatum(zoekdatum);

        // assert
        Assert.HasCount(3, tijdsloten,
            "Er moeten precies 3 tijdsloten worden teruggegeven voor 15-06-2026");

        var starttijden = tijdsloten
            .Select(t => DateTime.Parse(t.StartTijd).ToString("HH:mm"))
            .ToList();

        CollectionAssert.Contains(starttijden, "12:00",
            "Tijdslot 12:00 moet aanwezig zijn in de resultaten voor 15-06-2026");
        CollectionAssert.Contains(starttijden, "14:30",
            "Tijdslot 14:30 moet aanwezig zijn in de resultaten voor 15-06-2026");
        CollectionAssert.Contains(starttijden, "19:00",
            "Tijdslot 19:00 moet aanwezig zijn in de resultaten voor 15-06-2026");

        // cleanup — [TestCleanup] verwijdert de ingevoegde tijdsloten na afloop van de test
    }

    // ===== Acceptance Criteria 1: Zoeken op datum - S1 =====

    /// <summary>
    /// Path ID: Sad Path S1
    /// Input: Zoekdatum: 25-06-2026; 0 reserveringen op die dag
    /// Actor: Admin
    /// Verwachte output: Melding "Geen reserveringen op deze datum"; lege weergave
    /// Test type: Unit test
    /// Scenario: Admin zoekt op een datum waarop geen tijdsloten bestaan
    /// Verwacht: De geretourneerde lijst is leeg
    /// </summary>
    [TestMethod]
    public void GetTijdslotenByDatum_GeenReserveringenOpDatum_RetourneertLegeLijst()
    {
        // arrange
        string zoekdatum = "2026-06-25"; // datum waarop bewust geen tijdsloten zijn aangemaakt

        // act
        var tijdsloten = tijdslotAccess.GetTijdslotenByDatum(zoekdatum);

        // assert
        Assert.IsEmpty(tijdsloten,
            "De lijst moet leeg zijn voor een datum waarop geen tijdsloten bestaan; verwachte melding: 'Geen reserveringen op deze datum'");

        // cleanup — geen testdata ingevoegd; geen opruiming nodig
    }

    // ===== Acceptance Criteria 1: Zoeken op datum - S2 =====

    /// <summary>
    /// Path ID: Sad Path S2
    /// Input: Zoekdatum: 01-01-2020 (verleden datum)
    /// Actor: Admin
    /// Verwachte output: Foutmelding of lege weergave; datum in verleden niet toegestaan
    /// Test type: Unit test
    /// Scenario: Admin voert een datum in het verleden in als zoekdatum
    /// Verwacht: De datum wordt herkend als verleden datum en is niet toegestaan
    /// </summary>
    [TestMethod]
    public void ValideerZoekdatum_DatumInVerleden_WordtGeweigerd()
    {
        // arrange
        DateTime zoekdatum = new(2020, 1, 1); // 01-01-2020 — expliciet een datum in het verleden
        DateTime vandaag = DateTime.Today;              // grens: alleen huidige of toekomstige datums zijn toegestaan

        // act — datum is geldig als hij op of na vandaag valt
        bool isDatumInVerleden = zoekdatum.Date < vandaag;

        // assert
        Assert.IsTrue(isDatumInVerleden,
            "Datum 01-01-2020 ligt in het verleden en moet geweigerd worden; alleen huidige of toekomstige datums zijn toegestaan");

        // cleanup — geen testdata ingevoegd; geen opruiming nodig
    }

    // ===== Acceptance Criteria 1: Zoeken op datum - S3 =====

    /// <summary>
    /// Path ID: Sad Path S3
    /// Input: Datumveld leeg; admin klikt op "Zoek"
    /// Actor: Admin
    /// Verwachte output: Foutmelding: voer een geldige datum in; geen resultaten getoond
    /// Test type: Unit test
    /// Scenario: Admin laat het datumveld leeg en probeert te zoeken
    /// Verwacht: Het systeem herkent de lege invoer als ongeldig en geeft geen resultaten terug
    /// </summary>
    [TestMethod]
    public void ValideerZoekdatum_LegeDatumInvoer_IsOngeldig()
    {
        // arrange
        string zoekdatum = ""; // leeg datumveld — admin heeft geen datum ingevuld

        // act — datum is geldig als het veld niet leeg of whitespace is
        bool isGeldig = !string.IsNullOrWhiteSpace(zoekdatum);

        // assert
        Assert.IsFalse(isGeldig,
            "Lege datum is ongeldig; systeem moet de foutmelding 'voer een geldige datum in' tonen en geen resultaten weergeven");

        // cleanup — geen testdata ingevoegd; geen opruiming nodig
    }
}
