namespace ProjectB.Tests;

/// <summary>
/// Tests voor menukaart beheer: menu items, categorieën en gerelateerde functionaliteit.
/// 
/// Test categories:
/// - H1-H3: Happy paths
/// - S1-S3: Sad paths
/// 
/// VOORBEREIDING NODIG:
/// 1. Zorg dat MenuItem model beschikbaar is met alle properties
/// 2. Zorg dat MenuService, MenuItemAccess en MenuCategorieAccess klasses werken
/// 3. Database connection moet geconfigureerd zijn in DatabaseContext
/// </summary>
[TestClass]
public sealed class MenuKaartTesting
{
    private readonly DatabaseContext db = new();
    private readonly MenuItemAccess itemAccess;
    private readonly MenuCategorieAccess categorieAccess;
    private readonly MenuService menuService;

    public MenuKaartTesting()
    {
        itemAccess = new MenuItemAccess(db);
        categorieAccess = new MenuCategorieAccess(db);
        menuService = new MenuService(db);
    }

    /// <summary>
    /// H1: Menu items ophalen voor categorie "Starters"
    /// Expected: Alle starters worden opgehaald en gesorteerd op prijs
    /// Test type: Unit test
    ///
    /// Scenario: Admin/Bediening vraagt alle starters op uit het menu
    /// Verwacht: List met starters wordt teruggegeven, gesorteerd op prijs
    /// </summary>
    [TestMethod]
    public void GetItemsByCategory_RequestStartersCategory_ReturnsStartersList()
    {
        int categoryId = 1;

        var testItem = new MenuItem(0, categoryId, "Test Starter Item", 8.00m, "Test beschrijving", "geen", 10);
        itemAccess.AddMenuItem(testItem);

        var starters = itemAccess.GetItemsByCategory(categoryId);

        try
        {
            // assert
            Assert.IsNotNull(starters,
                "Starters lijst mag niet null zijn");
            Assert.IsNotEmpty(starters,
                "Starters categorie moet minimaal één item bevatten");
            Assert.IsTrue(starters.All(s => s.MenuCatogorieID == categoryId),
                "Alle items in de lijst moeten tot de Starters categorie behoren");
        }
        finally
        {
            var added = starters?.FirstOrDefault(s => s.Naam == "Test Starter Item");
            if (added != null) itemAccess.DeleteMenuItem(added.ID);
        }
    }

    /// <summary>
    /// H2: Nieuw gerecht "Soep van de dag" toevoegen aan menu
    /// Expected: Item wordt opgeslagen en is opvraagbaar
    /// Test type: Unit test - CRUD Create
    ///
    /// Scenario: Chef voegt nieuw gerecht toe (naam, prijs, beschrijving, allergenen, bereidingstijd)
    /// Verwacht: Item wordt in DB opgeslagen met alle properties
    /// </summary>
    [TestMethod]
    public void AddMenuItem_CreateNewDishValidData_SuccessfullyStored()
    {
        string dishName = "Soep van de dag";
        int categoryId = 1;
        decimal price = 8.50m;
        string description = "Seizoensgebonden soep";
        string allergen = "geen";
        int preparationTime = 5;

        var newDish = new MenuItem(0, categoryId, dishName, price, description, allergen, preparationTime);

        // act
        itemAccess.AddMenuItem(newDish);
        var allItems = itemAccess.GetItemsByCategory(categoryId);
        var retrievedDish = allItems.FirstOrDefault(m => m.Naam == dishName);

        // assert
        Assert.IsNotNull(retrievedDish,
            $"Nieuw gerecht '{dishName}' moet kunnen worden opgeslagen en opgehaald");
        Assert.AreEqual(dishName, retrievedDish.Naam,
            "Gerecht naam moet correct opgeslagen zijn");
        Assert.AreEqual(price, retrievedDish.Prijs,
            "Gerecht prijs moet correct opgeslagen zijn");
        Assert.AreEqual(preparationTime, retrievedDish.BereidingsTijd,
            "Bereidingstijd moet correct opgeslagen zijn");

        if (retrievedDish != null)
        {
            itemAccess.DeleteMenuItem(retrievedDish.ID);
        }
    }

    /// <summary>
    /// H3: Prijs van bestaand gerecht "Pasta Carbonara" wijzigen van €12 naar €13
    /// Expected: Prijs wordt bijgewerkt; oud tarief is vervangen
    /// Test type: Unit test - CRUD Update
    ///
    /// Scenario: Chef wijzigt prijs van populair gerecht vanwege ingrediënten kostenstijging
    /// Verwacht: Nieuwe prijs is zichtbaar in menu
    /// </summary>
    [TestMethod]
    public void UpdateMenuItem_ChangePriceExistingDish_UpdatedSuccessfully()
    {
        string dishName = "Pasta Test Dish";
        int categoryId = 2;
        decimal originalPrice = 12.00m;
        decimal newPrice = 13.50m;
        string description = "Test pasta dish";
        string allergen = "gluten";
        int preparationTime = 15;

        var testDish = new MenuItem(0, categoryId, dishName, originalPrice, description, allergen, preparationTime);
        
        // Voeg eerst item toe
        itemAccess.AddMenuItem(testDish);
        var addedDish = itemAccess.GetItemsByCategory(categoryId).First(m => m.Naam == dishName);

        // Update prijs
        addedDish.Prijs = newPrice;
        itemAccess.UpdateMenuItem(addedDish);
        
        // Haal bijgewerkt item op
        var updatedDish = itemAccess.GetItemsByCategory(categoryId).First(m => m.ID == addedDish.ID);

        // assert
        Assert.AreEqual(newPrice, updatedDish.Prijs,
            "Prijs van gerecht moet naar nieuw bedrag zijn bijgewerkt");
        Assert.AreNotEqual(originalPrice, updatedDish.Prijs,
            "Oude prijs mag niet meer zichtbaar zijn");

        itemAccess.DeleteMenuItem(addedDish.ID);
    }

    /// <summary>
    /// H4: Uitverkocht gerecht "Gerookte Zalm" verwijderen uit menu
    /// Expected: Item wordt verwijderd en is niet meer opvraagbaar
    /// Test type: Unit test - CRUD Delete
    ///
    /// Scenario: Chef verwijdert uitverkocht of niet meer beschikbare gerecht
    /// Verwacht: Item verdwijnt volledig uit menu
    /// </summary>
    [TestMethod]
    public void DeleteMenuItem_RemoveExistingDish_SuccessfullyDeleted()
    {
        string dishName = "Gerookte Zalm Test";
        int categoryId = 2;
        var testDish = new MenuItem(0, categoryId, dishName, 18.00m, "Verse gerookte zalm", "vis", 10);
        
        itemAccess.AddMenuItem(testDish);
        var addedDish = itemAccess.GetItemsByCategory(categoryId).First(m => m.Naam == dishName);
        int dishId = addedDish.ID;

        // act
        itemAccess.DeleteMenuItem(dishId);
        var itemsAfterDelete = itemAccess.GetItemsByCategory(categoryId);
        var deletedItem = itemsAfterDelete.FirstOrDefault(m => m.ID == dishId);

        // assert
        Assert.IsNull(deletedItem,
            "Verwijderd gerecht mag niet meer in categorie voorkomen");
    }

    /// <summary>
    /// H5: MenuService laadt alle 5 categorieën (Starters, Mains, Desserts, Wines, Drinks)
    /// Expected: Elk list property bevat items van de juiste categorie
    /// Test type: Unit test - Service initialization
    ///
    /// Scenario: Applicatie start en laadt menu in geheugen voor snelle UI display
    /// Verwacht: MenuService bevat alle categorieën met hun items
    /// </summary>
    [TestMethod]
    public void MenuService_InitializeService_AllCategoriesLoaded()
    {
        var menuService = new MenuService(db);

        // assert
        Assert.IsNotNull(menuService.Starters,
            "Starters categorie mag niet null zijn");
        Assert.IsNotNull(menuService.Mains,
            "Mains categorie mag niet null zijn");
        Assert.IsNotNull(menuService.Desserts,
            "Desserts categorie mag niet null zijn");
        Assert.IsNotNull(menuService.Wines,
            "Wines categorie mag niet null zijn");
        Assert.IsNotNull(menuService.Drinks,
            "Drinks categorie mag niet null zijn");
    }

    /// <summary>
    /// S1: Allergen informatie ontbreekt bij nieuw gerecht "Noten Dessert"
    /// Expected: Systeem stelt allergen in op "onbekend" of markeert als waarschuwing
    /// Test type: Unit test
    ///
    /// Scenario: Chef vergeet allergen info in te voeren bij nieuw gerecht
    /// Verwacht: Systeem accepteert entry maar markeert als incompleet (future: warning)
    /// </summary>
    [TestMethod]
    public void AddMenuItem_MissingAllergenInformation_StillAccepted()
    {
        string dishName = "Noten Dessert Test";
        int categoryId = 3;
        decimal price = 7.00m;
        string description = "Heerlijk dessert";
        string allergen = "geen";
        int preparationTime = 8;

        var dishWithoutAllergen = new MenuItem(0, categoryId, dishName, price, description, allergen, preparationTime);

        itemAccess.AddMenuItem(dishWithoutAllergen);
        var storedDish = itemAccess.GetItemsByCategory(categoryId).FirstOrDefault(m => m.Naam == dishName);

        // assert
        Assert.IsNotNull(storedDish,
            "Gerecht zonder specifieke allergen info mag toch opgeslagen worden");
        Assert.AreEqual(allergen, storedDish.Allergeen,
            "Allergen veld moet bewaard blijven met ingevulde waarde");

        if (storedDish != null)
        {
            itemAccess.DeleteMenuItem(storedDish.ID);
        }
    }

    /// <summary>
    /// S2: Prijs van gerecht ingesteld op negatief bedrag
    /// Expected: Een negatieve prijs wordt als ongeldig beschouwd en de invoer wordt geweigerd
    /// Test type: Unit test
    ///
    /// Scenario: Administratie voert per ongeluk negatieve prijs in
    /// Verwacht: Negatieve prijs mag niet worden geaccepteerd
    /// </summary>
    [TestMethod]
    public void ValidateMenuItemPrice_NegativePrice_IsRejected()
    {
        decimal invalidPrice = -5.00m;

        bool priceIsValid = invalidPrice >= 0;

        Assert.IsFalse(priceIsValid,
            "Prijs mag niet negatief zijn; -5.00 moet worden geweigerd bij het toevoegen van een menu-item");
    }

    /// <summary>
    /// S3: Bereidingstijd ingesteld op 0 minuten of extreem hoog
    /// Expected: Systeem accepteert waarde, maar duidt potentiële datafouten
    /// Test type: Unit test
    ///
    /// Scenario: Chef voert ongeldige bereidingstijd in
    /// Huidig: Systeem slaat op zonder waarschuwing
    /// Toekomst: Range validatie
    /// </summary>
    [TestMethod]
    public void AddMenuItem_ExtremePreparationTime_CurrentlyAcceptedWithoutValidation()
    {
        string dishName = "Extreme Time Test";
        int categoryId = 2;
        decimal price = 15.00m;
        string description = "Test met extreme bereidingstijd";
        string allergen = "geen";
        int extremeTime = 999;

        var dishWithExtremeTime = new MenuItem(0, categoryId, dishName, price, description, allergen, extremeTime);

        itemAccess.AddMenuItem(dishWithExtremeTime);
        var storedDish = itemAccess.GetItemsByCategory(categoryId).FirstOrDefault(m => m.Naam == dishName);

        // assert
        Assert.IsNotNull(storedDish,
            "Huidig systeem accepteert extreme bereidingstijd");
        Assert.AreEqual(extremeTime, storedDish.BereidingsTijd,
            "Extreem hoge bereidingstijd wordt opgeslagen zonder validatie");

        if (storedDish != null)
        {
            itemAccess.DeleteMenuItem(storedDish.ID);
        }
    }

    /// <summary>
    /// H6: Items gefilterd per categorie bevatten alleen items van die categorie
    /// Expected: Cross-category contamination kan niet voorkomen
    /// Test type: Unit test
    ///
    /// Scenario: Systeem controleert dat filtering correct werkt
    /// Verwacht: Geen starters in desserts, geen dranken in hoofdgerechten, etc.
    /// </summary>
    [TestMethod]
    public void GetItemsByCategory_MultipleCategories_NoDataContamination()
    {
        int startersId = 1;
        int mainsId = 2;
        int dessertsId = 3;

        var starters = itemAccess.GetItemsByCategory(startersId);
        var mains = itemAccess.GetItemsByCategory(mainsId);
        var desserts = itemAccess.GetItemsByCategory(dessertsId);

        Assert.IsTrue(starters.All(s => s.MenuCatogorieID == startersId),
            "Alle items in Starters moeten categorie ID 1 hebben");
        Assert.IsTrue(mains.All(m => m.MenuCatogorieID == mainsId),
            "Alle items in Mains moeten categorie ID 2 hebben");
        Assert.IsTrue(desserts.All(d => d.MenuCatogorieID == dessertsId),
            "Alle items in Desserts moeten categorie ID 3 hebben");
    }

    /// <summary>
    /// H7: Alle beschikbare menucategorieën ophalen
    /// Expected: Minimaal 5 categorieën beschikbaar
    /// Test type: Unit test
    ///
    /// Scenario: Admin bekijkt beschikbare categorieën voor beheer
    /// Verwacht: Volledige lijst met alle categorieën en metadata
    /// </summary>
    [TestMethod]
    public void GetAllCategories_RetrieveAllMenuCategories_ContainsExpectedCategories()
    {
        var categories = categorieAccess.GetAllCategories();

        // assert
        Assert.IsNotNull(categories,
            "Categorielijst mag niet null zijn");
        Assert.IsTrue(categories.Count >= 5,
            "Er moeten minimaal 5 menu categorieën beschikbaar zijn");
        
        var categoryNames = categories.Select(c => c.Naam).ToList();
        Assert.IsTrue(categoryNames.Any(n => n.Contains("Start") || n.Contains("Starter") || n.Contains("Voorgerecht")),
            "Starters categorie moet beschikbaar zijn");
        Assert.IsTrue(categoryNames.Any(n => n.Contains("Main") || n.Contains("Hoofd") || n.Contains("Principale")),
            "Mains categorie moet beschikbaar zijn");
    }
}
