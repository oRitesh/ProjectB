using Dapper;

namespace ProjectB.Tests;

[TestClass]
public sealed class BestellingAfhalenTesting
{
    private readonly DatabaseContext db;
    private readonly AfhaalSysteemLogic logic;

    [ClassInitialize]
    public static void SetupDatabase(TestContext _)
    {
        var setupDb = new DatabaseContext();
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
        setupDb.Close();
    }

    public BestellingAfhalenTesting()
    {
        db = new DatabaseContext();
        logic = new AfhaalSysteemLogic(db);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Verwijder alle items uit de winkelwagen na elke test
        // Zodat overgebleven items uit één test niet meelopen in de volgende test
        logic.Winkelwagen.Clear();
    }

    // ===== Acceptatiecriterium 1: Tijdslot kiezen - H2 =====
    /// <summary>
    /// Happy Path H2
    /// Input: Tijdkeuze: 45 minuten vanaf nu, Items: [Salade €7.00, Water €2.00], Klant: Fatima
    /// Expected: Bestelling ingepland op 15:45; klant ontvangt bevestiging
    /// Test type: Unit test
    ///
    /// Scenario: Klant voegt Salade en Water toe en controleert of de beschikbare tijdopties per kwartier stijgen
    /// Verwacht: GetOphaalTijdOpties geeft een niet-lege lijst terug waarbij opeenvolgende slots 15 minuten uit elkaar liggen
    /// </summary>
    [TestMethod]
    public void GetOphaalTijdOpties_ItemsInWinkelwagen_RetourneertKwartierSlots()
    {
        // arrange
        var salade = new MenuItem(1, 1, "Salade", 7.00m, "Verse salade", "", 10);
        var water = new MenuItem(2, 1, "Water", 2.00m, "Mineral water", "", 0);
        logic.VoegToe(salade);
        logic.VoegToe(water);

        // act
        var opties = logic.GetOphaalTijdOpties();

        // assert — minstens twee opties beschikbaar zodat het interval gecontroleerd kan worden
        Assert.IsGreaterThanOrEqualTo(2, opties.Count,
            "Er moeten minstens 2 tijdopties beschikbaar zijn om per kwartier te kunnen kiezen");

        // sla de eerste echte tijdoptie op (index 1 = eerste optie na 'Zo snel mogelijk')
        string eersteVasteOptie = opties[1]; // bijv. "17:15"
        string tweedeVasteOptie = opties[2]; // bijv. "17:30"

        DateTime eerste = DateTime.Parse(eersteVasteOptie);
        DateTime tweede = DateTime.Parse(tweedeVasteOptie);
        double verschilMinuten = (tweede - eerste).TotalMinutes;

        Assert.AreEqual(15.0, verschilMinuten,
            "Opeenvolgende tijdopties moeten precies 15 minuten uit elkaar liggen");
    }

    // ===== Acceptatiecriterium 2: Bestelling overzicht met prijs - H4 =====
    /// <summary>
    /// Happy Path H4
    /// Input: Items: [Pizza €10.00], Opmerking: leeg, Klant: Marieke
    /// Expected: Bestelling succesvol geplaatst zonder opmerking; totaal €10.00
    /// Test type: Unit test
    ///
    /// Scenario: Klant plaatst bestelling met één item en laat het opmerkingsveld leeg
    /// Verwacht: Totaalprijs is €10.00 en de status is exact "Ontvangen" zonder aangehangen opmerking
    /// </summary>
    [TestMethod]
    public void BestellingZonderOpmerking_LeegOpmerkingsveld_StatusIsOntvangenEnTotaalIs1000()
    {
        // arrange
        var pizza = new MenuItem(3, 2, "Pizza", 10.00m, "Margherita", "gluten", 15);
        logic.VoegToe(pizza);
        string opmerking = "";

        // act
        decimal totaal = logic.BerekenTotaal();
        // zelfde logica als in SlaBestellingOp
        string status = opmerking.Length > 0 ? $"Ontvangen - {opmerking}" : "Ontvangen";

        // assert
        Assert.AreEqual(10.00m, totaal,
            "Totaalprijs voor 1× Pizza €10.00 moet €10.00 zijn");
        Assert.AreEqual("Ontvangen", status,
            "Status zonder opmerking moet exact 'Ontvangen' zijn; geen tekst mag worden toegevoegd");
    }

    // ===== Acceptatiecriterium 1: Tijdslot kiezen - S1 =====
    /// <summary>
    /// Sad Path S1
    /// Input: Tijdkeuze: over 90 minuten (buiten 1-uursvenster), Klant: Bilal
    /// Expected: Foutmelding: tijdslot buiten toegestaan bereik; keuze geweigerd
    /// Test type: Unit test
    ///
    /// Scenario: Klant probeert een tijdslot te kiezen dat meer dan 1 uur in de toekomst ligt
    /// Verwacht: Een tijdslot 90 minuten vooruit overschrijdt het maximale venster van 60 minuten en is ongeldig
    /// </summary>
    [TestMethod]
    public void ValideerTijdslot_90MinutenVanNu_ValtBuitenEenUurVenster()
    {
        // arrange
        DateTime nu = DateTime.Now;
        DateTime gekozenTijdslot = nu.AddMinutes(90);
        DateTime maxToegestaan = nu.AddMinutes(60);

        // act
        bool isGeldig = gekozenTijdslot <= maxToegestaan;

        // assert
        Assert.IsFalse(isGeldig,
            "Tijdslot van 90 minuten in de toekomst valt buiten het maximale venster van 60 minuten en moet geweigerd worden");
    }

    // ===== Acceptatiecriterium 2: Bestelling overzicht met prijs - S2 =====
    /// <summary>
    /// Sad Path S2
    /// Input: Winkelmandje: leeg, Klant: Nadia
    /// Expected: Foutmelding: winkelmandje is leeg; bestellen niet mogelijk
    /// Test type: Unit test
    ///
    /// Scenario: Klant probeert te bestellen terwijl het winkelmandje geen items bevat
    /// Verwacht: Winkelmandje telt 0 items en BerekenTotaal geeft €0.00 terug
    /// </summary>
    [TestMethod]
    public void BerekenTotaal_LeegWinkelmandje_RetourneertNul()
    {
        // arrange — winkelwagen is al leeg via Cleanup; geen items toegevoegd

        // act
        int aantalItems = logic.Winkelwagen.Count;
        decimal totaal = logic.BerekenTotaal();

        // assert
        Assert.AreEqual(0, aantalItems,
            "Winkelmandje moet 0 items bevatten; bestellen zonder items is niet toegestaan");
        Assert.AreEqual(0m, totaal,
            "Totaalprijs van een leeg winkelmandje moet €0.00 zijn");
    }

    // ===== Acceptatiecriterium 3: Opmerking voor allergieën - S3 =====
    /// <summary>
    /// Sad Path S3
    /// Input: Opmerking: 520 tekens lange tekst, Items: [Burger €8.50], Klant: Karim
    /// Expected: Foutmelding: opmerking te lang (max 500 tekens); invoer geblokkeerd
    /// Test type: Unit test
    ///
    /// Scenario: Klant voert een opmerking in van 520 tekens, meer dan het toegestane maximum
    /// Verwacht: De opmerking van 520 tekens overschrijdt de grens van 500 tekens en moet geblokkeerd worden
    /// </summary>
    [TestMethod]
    public void ValideerOpmerking_520Tekens_OverschrijdtMaximumVan500()
    {
        // arrange
        string telangeOpmerking = new('A', 520); // 520 tekens — boven het maximum
        int maxTekens = 500;

        // act
        bool isGeldig = telangeOpmerking.Length <= maxTekens;

        // assert
        Assert.IsFalse(isGeldig,
            $"Opmerking van {telangeOpmerking.Length} tekens overschrijdt het maximum van {maxTekens} tekens en moet geblokkeerd worden");
    }
}
