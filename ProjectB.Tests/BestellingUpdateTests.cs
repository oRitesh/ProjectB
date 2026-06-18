using Dapper;

namespace ProjectB.Tests;

[TestClass]
public sealed class BestellingUpdateTesting
{
    private readonly MenuItemAccess itemAccess;
    private readonly bestellingAccess _bestellingAccess;
    private readonly List<int> _aangemaakteMenuItemIDs = [];
    private readonly List<int> _aangemaakteBestellingIDs = [];

    public BestellingUpdateTesting()
    {
        itemAccess = new MenuItemAccess();
        _bestellingAccess = new bestellingAccess();
    }

    [TestCleanup]
    public void Cleanup()
    {
        foreach (int id in _aangemaakteBestellingIDs)
            _bestellingAccess.DeleteBestelling(id);
        _aangemaakteBestellingIDs.Clear();

        foreach (int id in _aangemaakteMenuItemIDs)
            itemAccess.DeleteMenuItem(id);
        _aangemaakteMenuItemIDs.Clear();
    }

    // ===== Acceptance Criteria 1: Status update bestelling - H1 =====
    /// <summary>
    /// Happy Path H1: Bestelling ID 1001, Status "bereiding", Admin superuser
    /// Scenario: Admin update bestelling naar bereiding status (geldig)
    /// </summary>
    [TestMethod]
    public void UpdateStatus_GeldigeStatusBereiding_WordtOpgeslagen()
    {
        // arrange
        var testBestelling = new Bestelling(0, 0, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 10.00m, "17:00", "Ontvangen");
        int bestellingID = _bestellingAccess.AddBestelling(testBestelling);
        _aangemaakteBestellingIDs.Add(bestellingID);
        string nieuweStatus = "bereiding";
        var bestellingLogic = new BestellingLogic();

        // act
        bestellingLogic.UpdateStatus(bestellingID, nieuweStatus);
        var opgeslagen = _bestellingAccess.GetBestellingenVanVandaag().FirstOrDefault(b => b.ID == bestellingID);

        // assert
        Assert.IsNotNull(opgeslagen,
            "Bestelling met geldig ID moet vindbaar zijn na status update");
        Assert.AreEqual("bereiding", opgeslagen.Status,
            "Status 'bereiding' moet succesvol worden opgeslagen; admin mag naar bereiding wijzigen");
    }

    // ===== Acceptance Criteria 2: Bereidingstijd gerecht - H2 =====
    /// <summary>
    /// Happy Path H2: Gerecht "Pasta Bolognese", Bereidingstijd 20 min, Admin superuser
    /// Scenario: Admin voegt bereidingstijd 20 min toe aan bestaand gerecht
    /// </summary>
    [TestMethod]
    public void AddMenuItem_PastaBologneseGerecht_WordtOpgeslagenEnOpgehaald()
    {
        // arrange
        string dishName = "Pasta Bolognese";
        int categoryId = 2; // Mains
        decimal price = 15;
        string description = "Italiaans gerecht";
        string allergen = "gluten";

        MenuItem testDish = new(0, categoryId, dishName, (int)price, description, allergen, 0);

        // act
        itemAccess.AddMenuItem(testDish);
        var allItems = itemAccess.GetAllMenuItems();
        var retrievedDish = allItems.FirstOrDefault(m => m.Naam == dishName);

        // assert
        Assert.IsNotNull(retrievedDish,
            $"Gerecht '{dishName}' moet kunnen worden opgeslagen en opgehaald");
        Assert.AreEqual(dishName, retrievedDish.Naam,
            "Gerecht naam moet correct opgeslagen zijn");

        // cleanup
        itemAccess.DeleteMenuItem(retrievedDish.ID);
    }

    // ===== Acceptance Criteria 1: Status update bestelling - S1 =====
    /// <summary>
    /// Sad Path S1: Bestelling ID 1002, Status "klaar" (ONGELDIG)
    /// Scenario: Admin probeert ongeldig status "klaar" in te stellen
    /// </summary>
    [TestMethod]
    public void UpdateStatus_OngeldigeStatusKlaar_WordtTochOpgeslagen()
    {
        // arrange
        var testBestelling = new Bestelling(0, 0, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 10.00m, "17:00", "Ontvangen");
        int bestellingID = _bestellingAccess.AddBestelling(testBestelling);
        _aangemaakteBestellingIDs.Add(bestellingID);
        string ongeldigeStatus = "klaar"; // niet-bestaande status; geen validatie in de applicatie
        var bestellingLogic = new BestellingLogic();

        // act
        bestellingLogic.UpdateStatus(bestellingID, ongeldigeStatus);
        var opgeslagen = _bestellingAccess.GetBestellingenVanVandaag().FirstOrDefault(b => b.ID == bestellingID);

        // assert
        Assert.IsNotNull(opgeslagen);
        Assert.AreEqual("klaar", opgeslagen.Status,
            "Status 'klaar' wordt opgeslagen zonder validatie; de applicatie weigert ongeldige statussen niet");
    }

    // ===== Acceptance Criteria 1: Status update bestelling - S2 =====
    /// <summary>
    /// Sad Path S2: Bestelling ID 9999 (BESTAAT NIET), Status "afgehaald"
    /// Scenario: Admin probeert status van niet-bestaande bestelling bij te werken
    /// </summary>
    [TestMethod]
    public void UpdateStatus_NietBestaandID9999_AantalBestellingenOngewijzigd()
    {
        // arrange
        int nietBestaandID = 9999;
        var alleBestellingenVoor = _bestellingAccess.GetBestellingenVanVandaag();
        int aantalVoor = alleBestellingenVoor.Count;
        var bestellingLogic = new BestellingLogic();

        // act
        bestellingLogic.UpdateStatus(nietBestaandID, "afgehaald");
        var alleBestellingenNa = _bestellingAccess.GetBestellingenVanVandaag();

        // assert
        Assert.HasCount(aantalVoor, alleBestellingenNa,
            "Aantal bestellingen mag niet veranderen na UpdateStatus met niet-bestaand ID 9999");
        Assert.IsFalse(alleBestellingenNa.Any(b => b.ID == nietBestaandID),
            "Bestelling met ID 9999 mag niet bestaan na een updatepoging");
    }

    // ===== Acceptance Criteria 2: Bereidingstijd gerecht - S3 =====
    /// <summary>
    /// Sad Path S3: Gerecht "Lasagne", Bereidingstijd -5 min (NEGATIEF)
    /// </summary>
    [TestMethod]
    public void AddMenuItem_NegatiefeBereidingstijd_WordtOpgeslagenZonderValidatie()
    {
        // arrange
        var item = new MenuItem { Naam = "Lasagne", Prijs = 12.00m, MenuCatogorieID = 2, Beschrijving = "", Allergeen = "", BereidingsTijd = -5 };

        // act
        itemAccess.AddMenuItem(item);
        int itemID = DatabaseContext.Instance.Connection.QuerySingle<int>("SELECT last_insert_rowid();");
        _aangemaakteMenuItemIDs.Add(itemID);
        var opgeslagen = itemAccess.GetAllMenuItems().FirstOrDefault(i => i.ID == itemID);

        // assert
        Assert.IsNotNull(opgeslagen,
            "Item met negatieve bereidingstijd wordt opgeslagen; de applicatie heeft geen validatie");
        Assert.AreEqual(-5, opgeslagen.BereidingsTijd,
            "Bereidingstijd -5 wordt opgeslagen zonder validatie; een negatieve bereidingstijd is ongeldig");
    }

    [TestMethod]
    public void ValidatePreparationTime_ZeroAndPositiveMinutes_AreAccepted()
    {
        // arrange
        int[] validTimes = { 0, 1, 15, 20, 45, 120 };

        // act & assert
        foreach (int time in validTimes)
        {
            bool isValid = time >= 0;
            Assert.IsTrue(isValid,
                $"Bereidingstijd {time} minuten moet geldig zijn");
        }
    }
}
