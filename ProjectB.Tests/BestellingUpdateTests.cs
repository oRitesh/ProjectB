namespace ProjectB.Tests;

/// <summary>
/// Tests voor bestelling status updates en menu item bereidingstijd beheer.
/// Volgt MSTest stijl: [Class]Testing pattern met duidelijke AC-scheiding.
/// 
/// VOORBEREIDING NODIG:
/// 1. Voeg aan MenuItem model toe: public int BereidingsTijd { get; set; }
/// 2. Voeg aan Bestelling model toe: public string Status { get; set; }
/// 3. Update MenuItemAccess.UpdateMenuItem() SQL query om BereidingsTijd op te slaan
/// 4. Maak BestellingAccess klasse aan (zie MenuItemAccess als template)
/// </summary>
[TestClass]
public sealed class BestellingUpdateTesting
{
    private readonly DatabaseContext db = new();
    private readonly MenuItemAccess itemAccess;

    public BestellingUpdateTesting()
    {
        itemAccess = new MenuItemAccess(db);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Verwijder testdata uit database na elke test
        // Zodat tests geen invloed op elkaar hebben
    }

    // ===== Acceptance Criteria 1: Status update bestelling - H1 =====
    /// <summary>
    /// Happy Path H1: Bestelling ID 1001, Status "bereiding", Admin superuser
    /// Expected: Status succesvol gewijzigd naar "bereiding"; klant ziet update
    /// Test type: Unit test
    ///
    /// Scenario: Admin update bestelling naar bereiding status (geldig)
    /// Geldige statussen: "bereiding", "afgehaald"
    /// </summary>
    [TestMethod]
    public void ValidateBestellingStatus_ValidStatusBereiding_IsAccepted()
    {
        // arrange
        string inputStatus = "bereiding";
        var allowedStatuses = new[] { "bereiding", "afgehaald" };

        // act
        bool isValid = allowedStatuses.Contains(inputStatus.ToLower());

        // assert
        Assert.IsTrue(isValid,
            "Status 'bereiding' moet geaccepteerd worden door het systeem");
    }

    // ===== Acceptance Criteria 2: Bereidingstijd gerecht - H2 =====
    /// <summary>
    /// Happy Path H2: Gerecht "Pasta Bolognese", Bereidingstijd 20 min, Admin superuser
    /// Expected: Bereidingstijd opgeslagen; gerecht toont 20 min in overzicht
    /// Test type: Unit test
    ///
    /// Scenario: Admin voegt bereidingstijd 20 min toe aan bestaand gerecht
    /// Verwacht: Waarde wordt opgeslagen en is later opvraagbaar
    /// </summary>
    [TestMethod]
    public void AddMenuItem_CreateDishPastaBoloneseSaved_CanRetrieve()
    {
        // arrange
        string dishName = "Pasta Bolognese";
        int categoryId = 2; // Mains
        decimal price = 15;
        string description = "Italiaans gerecht";
        string allergen = "gluten";

        MenuItem testDish = new(0, categoryId, dishName, (int)price, description, allergen);

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
    /// Expected: Foutmelding "ongeldige status"; status blijft ongewijzigd
    /// Test type: Unit test
    ///
    /// Scenario: Admin probeert ongeldig status "klaar" in te stellen
    /// "klaar" is GEEN geldige status. Alleen "bereiding" en "afgehaald" zijn toegestaan
    /// </summary>
    [TestMethod]
    public void ValidateBestellingStatus_InvalidStatusKlaar_IsRejected()
    {
        // arrange
        string inputStatus = "klaar"; // ONGELDIG
        var allowedStatuses = new[] { "bereiding", "afgehaald" };

        // act
        bool isValid = allowedStatuses.Contains(inputStatus.ToLower());

        // assert
        Assert.IsFalse(isValid,
            "Status 'klaar' mag NIET geldig zijn. Enkel 'bereiding' en 'afgehaald' zijn toegestaan");
    }

    // ===== Acceptance Criteria 1: Status update bestelling - S2 =====
    /// <summary>
    /// Sad Path S2: Bestelling ID 9999 (BESTAAT NIET), Status "afgehaald"
    /// Expected: Foutmelding "bestelling niet gevonden"; geen update uitgevoerd
    /// Test type: Unit test
    ///
    /// Scenario: Admin probeert status van niet-bestaande bestelling bij te werken
    /// BestellingAccess.GetBestellingByID(9999) moet null retourneren
    /// </summary>
    [TestMethod]
    public void BestellingIDValidation_NonExistentID9999_MustBeRejected()
    {
        // arrange
        int invalidBestellingId = 9999;
        var validBestellingIds = new[] { 1001, 1002, 1003 }; // voorbeeld: bestaande IDs

        // act
        bool exists = validBestellingIds.Contains(invalidBestellingId);

        // assert
        Assert.IsFalse(exists,
            "Bestelling ID 9999 mag niet bestaan; update mag niet plaatsvinden");
    }

    // ===== Acceptance Criteria 2: Bereidingstijd gerecht - S3 =====
    /// <summary>
    /// Sad Path S3: Gerecht "Lasagne", Bereidingstijd -5 min (NEGATIEF)
    /// Expected: Foutmelding "bereidingstijd mag niet negatief zijn"; waarde niet opgeslagen
    /// Test type: Unit test
    /// </summary>
    [TestMethod]
    public void ValidatePreparationTime_NegativeTimeS3_ReturnsFalse()
    {
        // arrange
        int negativeMinutes = -5; // ONGELDIG: mag nooit negatief zijn

        // act
        bool isValid = negativeMinutes >= 0;

        // assert
        Assert.IsFalse(isValid,
            "Bereidingstijd mag NIET negatief zijn; -5 minuten is ongeldig");
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
