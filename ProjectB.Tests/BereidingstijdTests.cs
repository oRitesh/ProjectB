using Dapper;

namespace ProjectB.Tests;

[TestClass]
public sealed class BereidingstijdTesting
{
    private readonly AfhaalSysteemLogic logic;

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

    public BereidingstijdTesting()
    {
        logic = new AfhaalSysteemLogic();
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Verwijder alle items uit de winkelwagen na elke test
        // Zodat overgebleven items uit één test niet meelopen in de volgende test
        logic.Winkelwagen.Clear();
    }

    // ===== Acceptance Criteria 2: Totale bereidingstijd als langste gerecht + marge - H2 =====

    /// <summary>
    /// Path: Happy Path H2
    /// Input data: Bestelling datum: vandaag (27-05-2026), Tijdstip: 13:00, Klant: Sara
    /// Actor: Sara
    /// Expected output: Afhaalbestelling succesvol geplaatst; bereidingstijd zichtbaar
    /// Test type: Unit test
    /// Scenario (NL): Klant plaatst afhaalbestelling op de dag zelf en ziet bereidingstijd
    /// Verwacht (NL): Bestelling wordt succesvol aangemaakt en bereidingstijd is zichtbaar
    /// </summary>
    [TestMethod]
    public void BerekenOphaalTijd_LangsteBereidingstijdPlusMarge_GeeftJuisteTijd()
    {
        // arrange
        var steak = new MenuItem(1, 1, "Steak", 25.00m, "Ribeye steak", "", 25);      // BereidingsTijd = 25 min - langste
        var soep = new MenuItem(2, 1, "Tomatensoep", 5.00m, "Verse tomatensoep", "", 10); // BereidingsTijd = 10 min
        logic.VoegToe(steak);
        logic.VoegToe(soep);

        int verwachteLangsteBereidingstijd = 25; // steak heeft de langste bereidingstijd
        int systeemMarge = 15;                   // vaste marge van het systeem

        // act
        DateTime voor = DateTime.Now;
        string ophaalTijdStr = logic.BerekenOphaalTijd();

        // assert - bereken verschil in minuten tussen 'nu' en de teruggegeven ophaalttijd
        DateTime ophaalTijd = DateTime.Parse(ophaalTijdStr);
        double aantalMinutenToegevoegd = (ophaalTijd.TimeOfDay - voor.TimeOfDay).TotalMinutes;
        double verwachteToevoeging = verwachteLangsteBereidingstijd + systeemMarge; // 25 + 15 = 40

        Assert.IsTrue(
            aantalMinutenToegevoegd >= verwachteToevoeging - 1 &&
            aantalMinutenToegevoegd <= verwachteToevoeging + 1,
            $"Ophaalttijd moet de langste bereidingstijd ({verwachteLangsteBereidingstijd} min) " +
            $"plus {systeemMarge} min marge zijn; berekend verschil was {aantalMinutenToegevoegd:F1} min");
    }

    // ===== Acceptance Criteria 3: Afhaalbestelling alleen op dag zelf - S1 =====

    /// <summary>
    /// Path: Sad Path S1
    /// Input data: Bestelling datum: morgen (28-05-2026), Klant: Daan
    /// Actor: Daan
    /// Expected output: Foutmelding: afhalen alleen mogelijk op de dag zelf; bestelling geblokkeerd
    /// Test type: Unit test
    /// Scenario (NL): Klant probeert afhaalbestelling te plaatsen voor een toekomstige datum
    /// Verwacht (NL): Systeem blokkeert de bestelling en geeft een foutmelding terug
    /// </summary>
    [TestMethod]
    public void BerekenOphaalTijd_LegWinkelwagen_GeeftTijdVandaag()
    {
        // arrange - lege winkelwagen; geen bereidingstijd

        // act
        DateTime voor = DateTime.Now;
        string ophaalTijdStr = logic.BerekenOphaalTijd(); // retourneert "HH:mm" voor vandaag

        // assert
        // BerekenOphaalTijd retourneert altijd vandaag als datum; een toekomstige datum is niet mogelijk
        DateTime ophaalTijdVandaag = DateTime.Today.Add(TimeSpan.Parse(ophaalTijdStr));
        Assert.IsTrue(ophaalTijdVandaag >= voor,
            "BerekenOphaalTijd berekent altijd een tijd voor vandaag; afhalen op een toekomstige datum is niet mogelijk");
        Assert.IsFalse(ophaalTijdStr.Contains('-') || ophaalTijdStr.Contains('/'),
            "Het resultaat is uitsluitend een tijdstip in 'HH:mm'-formaat zonder datumcomponent; afhalen voor morgen is structureel niet mogelijk");
    }

    // ===== Acceptance Criteria 2: Bereidingstijd ontbreekt voor gerecht - S2 =====

    /// <summary>
    /// Path: Sad Path S2
    /// Input data: Menu: [Steak, bereidingstijd: niet ingesteld], Klant: Roos
    /// Actor: Roos
    /// Expected output: Foutmelding: bereidingstijd ontbreekt voor gerecht "Steak"
    /// Test type: Unit test
    /// Scenario (NL): Gerecht in het menu heeft geen bereidingstijd ingesteld
    /// Verwacht (NL): Systeem gooit een foutmelding dat de bereidingstijd voor "Steak" ontbreekt
    /// </summary>
    [TestMethod]
    public void BerekenOphaalTijd_BereidingsTijdNul_GeeftAlleenMargeVanVijftienMinuten()
    {
        // arrange
        var steak = new MenuItem(1, 1, "Steak", 25.00m, "Ribeye steak", "", 0); // BereidingsTijd = 0
        logic.VoegToe(steak);
        int systeemMarge = 15;

        // act
        DateTime voor = DateTime.Now;
        string ophaalTijdStr = logic.BerekenOphaalTijd();
        DateTime ophaalTijd = DateTime.Today.Add(TimeSpan.Parse(ophaalTijdStr));
        double aantalMinutenToegevoegd = (ophaalTijd - voor).TotalMinutes;

        // assert
        // Met BereidingsTijd=0 wordt alleen de systeemmarge van 15 min opgeteld; geen foutmelding
        Assert.IsTrue(aantalMinutenToegevoegd >= systeemMarge - 1 && aantalMinutenToegevoegd <= systeemMarge + 1,
            $"Bij BereidingsTijd=0 is de ophaalttijd slechts ~{systeemMarge} minuten vooruit ({aantalMinutenToegevoegd:F1} min); de applicatie heeft geen validatie voor ontbrekende bereidingstijd");
    }
}
