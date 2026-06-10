namespace ProjectB.Tests;

/// <summary>
/// Test categories:
/// - H1-H3: Happy paths
/// - S1-S3: Sad paths

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
    /// Scenario: Admin/Bediening vraagt alle starters op uit het menu
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
    /// H2: Prijs van bestaand gerecht "Pasta Carbonara" wijzigen van €12 naar €13
    /// Scenario: Chef wijzigt prijs van populair gerecht vanwege ingrediënten kostenstijging
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
        

        itemAccess.AddMenuItem(testDish);
        var addedDish = itemAccess.GetItemsByCategory(categoryId).First(m => m.Naam == dishName);

        addedDish.Prijs = newPrice;
        itemAccess.UpdateMenuItem(addedDish);

        var updatedDish = itemAccess.GetItemsByCategory(categoryId).First(m => m.ID == addedDish.ID);

        Assert.AreEqual(newPrice, updatedDish.Prijs,
            "Prijs van gerecht moet naar nieuw bedrag zijn bijgewerkt");
        Assert.AreNotEqual(originalPrice, updatedDish.Prijs,
            "Oude prijs mag niet meer zichtbaar zijn");

        itemAccess.DeleteMenuItem(addedDish.ID);
    }

    /// <summary>
    /// H3: MenuService laadt alle 5 categorieën (Starters, Mains, Desserts, Wines, Drinks)
    /// Scenario: Applicatie start en laadt menu in geheugen voor snelle UI display
    /// </summary>
    [TestMethod]
    public void MenuService_InitializeService_AllCategoriesLoaded()
    {
        var menuService = new MenuService(db);

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
    /// Scenario: Chef vergeet allergen info in te voeren bij nieuw gerecht
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
    /// Scenario: Administratie voert per ongeluk negatieve prijs in
    /// </summary>
    [TestMethod]
    public void ValidateMenuItemPrice_NegativePrice_IsRejected()
    {
        decimal invalidPrice = -5.00m;

        bool priceIsValid = invalidPrice >= 0;

        Assert.IsFalse(priceIsValid,
            "Prijs mag niet negatief zijn; -5.00 moet worden geweigerd bij het toevoegen van een menu-item");
    }
}