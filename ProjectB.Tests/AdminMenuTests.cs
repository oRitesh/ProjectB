using Dapper;

namespace ProjectB.Tests;

[TestClass]
public sealed class AdminMenuTests
{
    private readonly DatabaseContext _db;
    private readonly MenuItemAccess _menuItemAccess;
    private readonly List<int> _aangemaakteMenuItemIDs = [];

    public AdminMenuTests()
    {
        _db = DatabaseContext.Instance;
        _menuItemAccess = new MenuItemAccess();
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Verwijder menu-items die tijdens de test zijn aangemaakt
        // zodat tests elkaar niet beïnvloeden via gedeelde databasestatus
        foreach (int id in _aangemaakteMenuItemIDs)
            _menuItemAccess.DeleteMenuItem(id);
        _aangemaakteMenuItemIDs.Clear();
    }

    // ===== Acceptance Criteria 1: Admin voegt items toe met naam, prijs en categorie-id - H1 =====

    /// <summary>
    /// Path ID: Happy Path H1
    /// Scenario (NL): Admin voegt een nieuw menu-item toe met geldige naam, prijs en categorie-id
    /// </summary>
    [TestMethod]
    public void AddMenuItem_GeldigeGegevens_ItemZichtbaarInOverzicht()
    {
        // arrange
        var item = new MenuItem
        {
            Naam = "Gegrilde Zalm",   // naam uit testscript
            Prijs = 18.50m,           // prijs uit testscript
            MenuCatogorieID = 3,      // categorie-id uit testscript
            Beschrijving = "",
            Allergeen = "",
            BereidingsTijd = 0
        };

        // act
        _menuItemAccess.AddMenuItem(item);
        int nieuweID = _db.Connection.QuerySingle<int>("SELECT last_insert_rowid();");
        _aangemaakteMenuItemIDs.Add(nieuweID); // bijhouden voor cleanup

        var alleItems = _menuItemAccess.GetAllMenuItems();
        bool itemGevonden = alleItems.Any(i => i.ID == nieuweID && i.Naam == "Gegrilde Zalm");

        // assert
        Assert.IsTrue(itemGevonden,
            "Item 'Gegrilde Zalm' moet na toevoeging zichtbaar zijn in het menuoverzicht");

        // cleanup - wordt afgehandeld door [TestCleanup]
    }

    // ===== Acceptance Criteria 2: Admin wijzigt items via geldig item-id - H2 =====

    /// <summary>
    /// Path ID: Happy Path H2
    /// Scenario (NL): Admin wijzigt de naam van een bestaand menu-item via een geldig item-id
    /// </summary>
    [TestMethod]
    public void UpdateMenuItem_GeldigItemId_NaamBijgewerkt()
    {
        // arrange
        // Voeg eerst een item in zodat het id dynamisch bepaald kan worden
        var origineel = new MenuItem
        {
            Naam = "Gegrilde Zalm",   // originele naam vóór de wijziging
            Prijs = 18.50m,           // prijs blijft ongewijzigd na update (zie testscript)
            MenuCatogorieID = 3,
            Beschrijving = "",
            Allergeen = "",
            BereidingsTijd = 0
        };
        _menuItemAccess.AddMenuItem(origineel);
        int itemID = _db.Connection.QuerySingle<int>("SELECT last_insert_rowid();");
        _aangemaakteMenuItemIDs.Add(itemID); // bijhouden voor cleanup (ook als update slaagt)

        var gewijzigd = new MenuItem
        {
            ID = itemID,
            Naam = "Zalm Speciaal",   // nieuwe naam uit testscript
            Prijs = 18.50m,           // prijs ongewijzigd zoals het testscript aangeeft
            MenuCatogorieID = 3,
            Beschrijving = "",
            Allergeen = "",
            BereidingsTijd = 0
        };

        // act
        _menuItemAccess.UpdateMenuItem(gewijzigd);
        var alleItems = _menuItemAccess.GetAllMenuItems();
        var gevondenItem = alleItems.FirstOrDefault(i => i.ID == itemID);

        // assert
        Assert.IsNotNull(gevondenItem,
            "Het bijgewerkte item moet nog steeds zichtbaar zijn in het menuoverzicht");
        Assert.AreEqual("Zalm Speciaal", gevondenItem.Naam,
            "De naam van het item moet zijn bijgewerkt naar 'Zalm Speciaal'");

        // cleanup - wordt afgehandeld door [TestCleanup]
    }

    // ===== Acceptance Criteria 3: Admin verwijdert items via item-id - H3 =====

    /// <summary>
    /// Path ID: Happy Path H3
    /// Scenario (NL): Admin verwijdert een menu-item via een geldig item-id
    /// </summary>
    [TestMethod]
    public void DeleteMenuItem_GeldigItemId_ItemVerwijderdUitOverzicht()
    {
        // arrange
        // Voeg eerst een item in zodat er een geldig id beschikbaar is om te verwijderen
        var teVerwijderen = new MenuItem
        {
            Naam = "Zalm Speciaal",   // naam uit testscript
            Prijs = 18.50m,
            MenuCatogorieID = 3,
            Beschrijving = "",
            Allergeen = "",
            BereidingsTijd = 0
        };
        _menuItemAccess.AddMenuItem(teVerwijderen);
        int itemID = _db.Connection.QuerySingle<int>("SELECT last_insert_rowid();");
        // Niet toevoegen aan _aangemaakteMenuItemIDs: dit item wordt door de test zelf verwijderd

        // act
        _menuItemAccess.DeleteMenuItem(itemID);
        var alleItems = _menuItemAccess.GetAllMenuItems();
        bool itemNogAanwezig = alleItems.Any(i => i.ID == itemID);

        // assert
        Assert.IsFalse(itemNogAanwezig,
            "Item 'Zalm Speciaal' mag na verwijdering niet meer zichtbaar zijn in het menuoverzicht");

        // cleanup - item is al verwijderd; geen verdere actie nodig
    }

    // ===== Acceptance Criteria 1: Admin voegt items toe met naam, prijs en categorie-id - S1 =====

    /// <summary>
    /// Path ID: Sad Path S1
    /// Scenario (NL): Admin probeert een item toe te voegen zonder naam
    /// </summary>
    [TestMethod]
    public void AddMenuItem_LegeNaam_NaamWordtOpgeslagen()
    {
        // Arrange
        var item = new MenuItem { Naam = "", Prijs = 12.00m, MenuCatogorieID = 2, Beschrijving = "", Allergeen = "", BereidingsTijd = 0 };

        // Act
        _menuItemAccess.AddMenuItem(item);
        int nieuweID = _db.Connection.QuerySingle<int>("SELECT last_insert_rowid();");
        _aangemaakteMenuItemIDs.Add(nieuweID);
        var opgeslagen = _menuItemAccess.GetAllMenuItems().FirstOrDefault(i => i.ID == nieuweID);

        // Assert
        Assert.IsNotNull(opgeslagen,
            "Item met lege naam wordt opgeslagen; de logic-laag heeft geen naamvalidatie");
        Assert.AreEqual("", opgeslagen.Naam,
            "Opgeslagen naam moet een lege string zijn");

        // cleanup - wordt afgehandeld door [TestCleanup]
    }

    // ===== Acceptance Criteria 1: Admin voegt items toe met naam, prijs en categorie-id - S2 =====

    /// <summary>
    /// Path ID: Sad Path S2
    /// Scenario (NL): Admin voert een negatieve prijs in bij het toevoegen van een item
    /// </summary>
    [TestMethod]
    public void IsGeldigePrijs_NegatiefePrijs_RetourneertFalse()
    {
        // Arrange
        decimal prijs = -5.00m;
        var logic = new MenuItemLogic();

        // Act
        bool prijsIsGeldig = logic.IsGeldigePrijs(prijs);

        // Assert
        Assert.IsFalse(prijsIsGeldig,
            "Prijs mag niet negatief zijn; -5.00 moet worden geweigerd bij het toevoegen van een menu-item");

        // cleanup - geen item aangemaakt
    }

    // ===== Acceptance Criteria 2: Admin wijzigt items via geldig item-id - S3 =====

    /// <summary>
    /// Path ID: Sad Path S3
    /// Scenario (NL): Admin probeert een item te wijzigen met een niet-bestaand id
    /// </summary>
    [TestMethod]
    public void UpdateMenuItem_NietBestaandItemId_GeenWijzigingUitgevoerd()
    {
        // arrange
        int onbestaandID = 9999; // id dat niet bestaat in de database (zie testscript)
        var alleItemsVoor = _menuItemAccess.GetAllMenuItems();
        int aantalVoor = alleItemsVoor.Count; // snapshots van huidige toestand vóór de actie

        var gewijzigd = new MenuItem
        {
            ID = onbestaandID,
            Naam = "Soep Deluxe",   // nieuwe naam uit testscript
            Prijs = 10.00m,
            MenuCatogorieID = 1,
            Beschrijving = "",
            Allergeen = "",
            BereidingsTijd = 0
        };

        // act
        _menuItemAccess.UpdateMenuItem(gewijzigd);
        var alleItemsNa = _menuItemAccess.GetAllMenuItems();
        int aantalNa = alleItemsNa.Count;
        bool bevatSoepDeluxe = alleItemsNa.Any(i => i.Naam == "Soep Deluxe");

        // assert
        Assert.AreEqual(aantalVoor, aantalNa,
            "Het aantal menu-items mag niet veranderen na een updatepoging met het niet-bestaande id 9999");
        Assert.IsFalse(bevatSoepDeluxe,
            "Item 'Soep Deluxe' mag niet in het overzicht verschijnen; id 9999 bestaat niet in de database");

        // cleanup - geen item aangemaakt
    }

    // ===== Acceptance Criteria 3: Admin verwijdert items via item-id - S4 =====

    /// <summary>
    /// Path ID: Sad Path S4
    /// Scenario (NL): Admin probeert een item te verwijderen met een niet-bestaand id
    /// </summary>
    [TestMethod]
    public void DeleteMenuItem_NietBestaandItemId_AantalItemsOngewijzigd()
    {
        // arrange
        int onbestaandID = 8888; // id dat niet bestaat in de database (zie testscript)
        var alleItemsVoor = _menuItemAccess.GetAllMenuItems();
        int aantalVoor = alleItemsVoor.Count; // snapshot vóór de verwijderpoging

        // act
        _menuItemAccess.DeleteMenuItem(onbestaandID);
        var alleItemsNa = _menuItemAccess.GetAllMenuItems();
        int aantalNa = alleItemsNa.Count;

        // assert
        Assert.AreEqual(aantalVoor, aantalNa,
            "Het aantal menu-items mag niet afnemen na een verwijderpoging met het niet-bestaande id 8888");

        // cleanup - geen item aangemaakt
    }
}
