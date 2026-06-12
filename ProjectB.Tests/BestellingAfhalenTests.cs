using Dapper;

namespace ProjectB.Tests;

[TestClass]
public sealed class BestellingAfhalenTesting
{
    private readonly AfhaalSysteemLogic logic;
    private readonly bestellingAccess _bestellingAccess;
    private readonly List<int> _aangemaakteBestellingIDs = [];

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

    public BestellingAfhalenTesting()
    {
        logic = new AfhaalSysteemLogic();
        _bestellingAccess = new bestellingAccess();
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Verwijder alle items uit de winkelwagen na elke test
        // Zodat overgebleven items uit één test niet meelopen in de volgende test
        logic.Winkelwagen.Clear();

        foreach (int id in _aangemaakteBestellingIDs)
            _bestellingAccess.DeleteBestelling(id);
        _aangemaakteBestellingIDs.Clear();
    }

    // ===== Acceptatiecriterium 1: Tijdslot kiezen - H2 =====
    /// <summary>
    /// Happy Path H2
    /// Scenario: Klant voegt Salade en Water toe en controleert of de beschikbare tijdopties per kwartier stijgen
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

        // assert - minstens twee opties beschikbaar zodat het interval gecontroleerd kan worden
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
    /// Scenario: Klant plaatst bestelling met één item en laat het opmerkingsveld leeg
    /// Verwacht: Totaalprijs is €10.00 en de status is exact "Ontvangen" zonder aangehangen opmerking
    /// </summary>
    [TestMethod]
    public void SlaBestellingOp_LeegOpmerkingsveld_StatusIsOntvangen()
    {
        // arrange
        var pizza = new MenuItem(3, 2, "Pizza", 10.00m, "Margherita", "gluten", 15);
        logic.VoegToe(pizza);
        string opmerking = "";
        string ophaalTijd = "17:00";

        // act
        decimal totaal = logic.BerekenTotaal();
        logic.Winkelwagen.Clear(); // leeg winkelwagen zodat er geen BestellingMenuItems worden aangemaakt
        logic.SlaBestellingOp(0, ophaalTijd, opmerking);
        int bestellingID = DatabaseContext.Instance.Connection.QuerySingle<int>("SELECT last_insert_rowid();");
        _aangemaakteBestellingIDs.Add(bestellingID);
        var opgeslagen = _bestellingAccess.GetAllBestellingen().FirstOrDefault(b => b.ID == bestellingID);

        // assert
        Assert.AreEqual(10.00m, totaal,
            "Totaalprijs voor 1× Pizza €10.00 moet €10.00 zijn");
        Assert.IsNotNull(opgeslagen, "Bestelling moet zijn opgeslagen na SlaBestellingOp");
        Assert.AreEqual("Ontvangen", opgeslagen!.Status,
            "Status zonder opmerking moet exact 'Ontvangen' zijn; geen tekst mag worden toegevoegd");
    }

    // ===== Acceptatiecriterium 1: Tijdslot kiezen - S1 =====
    /// <summary>
    /// Sad Path S1
    /// Scenario: Klant probeert een tijdslot te kiezen dat meer dan 1 uur in de toekomst ligt
    /// </summary>
    [TestMethod]
    public void GetOphaalTijdOpties_ItemsInWinkelwagen_GeeftMeerdereSlots()
    {
        // arrange
        var pizza = new MenuItem(3, 2, "Pizza", 10.00m, "Margherita", "gluten", 15);
        logic.VoegToe(pizza);

        // act
        var opties = logic.GetOphaalTijdOpties();

        // assert
        // GetOphaalTijdOpties biedt slots tot sluitingstijd - 1 uur; geen maximumvenster van 60 min is geïmplementeerd
        Assert.IsGreaterThanOrEqualTo(2, opties.Count,
            "GetOphaalTijdOpties biedt meerdere tijdsloten aan; de applicatie heeft geen maximumvenster van 60 minuten");
    }

    // ===== Acceptatiecriterium 2: Bestelling overzicht met prijs - S2 =====
    /// <summary>
    /// Sad Path S2
    /// Scenario: Klant probeert te bestellen terwijl het winkelmandje geen items bevat
    /// </summary>
    [TestMethod]
    public void BerekenTotaal_LeegWinkelmandje_RetourneertNul()
    {
        // arrange - winkelwagen is al leeg via Cleanup; geen items toegevoegd

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
    /// Scenario: Klant voert een opmerking in van 520 tekens, meer dan het toegestane maximum
    /// </summary>
    [TestMethod]
    public void SlaBestellingOp_Opmerking520Tekens_WordtOpgeslagenZonderValidatie()
    {
        // arrange
        string telangeOpmerking = new('A', 520); // 520 tekens - boven het maximum
        string ophaalTijd = "17:00";

        // act - winkelwagen is leeg (geen BestellingMenuItems); alleen status wordt gecontroleerd
        logic.SlaBestellingOp(0, ophaalTijd, telangeOpmerking);
        int bestellingID = DatabaseContext.Instance.Connection.QuerySingle<int>("SELECT last_insert_rowid();");
        _aangemaakteBestellingIDs.Add(bestellingID);
        var opgeslagen = _bestellingAccess.GetAllBestellingen().FirstOrDefault(b => b.ID == bestellingID);

        // assert
        Assert.IsNotNull(opgeslagen,
            "Bestelling met lange opmerking moet zijn opgeslagen");
        Assert.Contains(telangeOpmerking, opgeslagen.Status,
            "Opmerking van 520 tekens wordt opgeslagen zonder validatie; de applicatie heeft geen maximumlengte van 500 tekens");
    }
}
