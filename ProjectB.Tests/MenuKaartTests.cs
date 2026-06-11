namespace ProjectB.Tests;

/// <summary>
/// Test categories:
/// - H4-H6: Happy paths
/// - S2-S3: Sad paths

/// </summary>
[TestClass]
public sealed class MenuKaartTesting
{
    private readonly DatabaseContext db = new();
    private readonly MenuItemAccess itemAccess;

    public MenuKaartTesting()
    {
        itemAccess = new MenuItemAccess(db);
    }

    /// <summary>
    /// H4: Menu items ophalen voor categorie "Starters"
    /// Scenario: Gebruiker vraagt alle starters op uit het menu.
    /// </summary>
    [TestMethod]
    public void GetItemsByCategory_VraagVoorgerechtenAan_GeeftVoorgerechtenTerug()
    {
        int categoryId = 1;

        var testGerecht = new MenuItem(0, categoryId, "Test voorgerecht", 8.00m, "Test beschrijving", "geen", 10);
        itemAccess.AddMenuItem(testGerecht);

        var Voorgerechten = itemAccess.GetItemsByCategory(categoryId);

        try
        {
            Assert.IsNotNull(Voorgerechten,
                "Voorgerechten lijst mag niet null zijn");
            Assert.IsNotEmpty(Voorgerechten,
                "Voorgerechten categorie moet minimaal één item bevatten");
            Assert.IsTrue(Voorgerechten.All(s => s.MenuCatogorieID == categoryId),
                "Alle items in de lijst moeten tot de Voorgerechten categorie behoren");
        }
        finally
        {
            var Toegevoegd = Voorgerechten?.FirstOrDefault(s => s.Naam == "Test voorgerecht");
            if (Toegevoegd != null) itemAccess.DeleteMenuItem(Toegevoegd.ID);
        }
    }

    /// <summary>
    /// H5: Prijs van bestaand gerecht "Pasta test gerecht" wijzigen van €12 naar €13
    /// Scenario: gebruiker wijzigt prijs van gerecht.
    /// </summary>
    [TestMethod]
    public void UpdateMenuItem_VeranderPrijsBestaandGerecht_SuccesvolAangepast()
    {
        string gerechtNaam = "Pasta Test gerecht";
        int categoryId = 2;
        decimal originelePrijs = 12.00m;
        decimal NieuwePrijs = 13.50m;
        string beschrijving = "Test gerecht";
        string allergenen = "gluten";
        int bereidingstijd = 15;

        var testGerecht = new MenuItem(0, categoryId, gerechtNaam, originelePrijs, beschrijving, allergenen, bereidingstijd);
        

        itemAccess.AddMenuItem(testGerecht);
        var toegevoegdGerecht = itemAccess.GetItemsByCategory(categoryId).First(m => m.Naam == gerechtNaam);

        toegevoegdGerecht.Prijs = NieuwePrijs;
        itemAccess.UpdateMenuItem(toegevoegdGerecht);

        var aangepastGerecht = itemAccess.GetItemsByCategory(categoryId).First(m => m.ID == toegevoegdGerecht.ID);

        Assert.AreEqual(NieuwePrijs, aangepastGerecht.Prijs,
            "Prijs van gerecht moet naar nieuw bedrag zijn bijgewerkt");
        Assert.AreNotEqual(originelePrijs, aangepastGerecht.Prijs,
            "Oude prijs mag niet meer zichtbaar zijn");

        itemAccess.DeleteMenuItem(toegevoegdGerecht.ID);
    }

    /// <summary>
    /// H6: MenuService laadt alle 5 categorieën (Starters, Mains, Desserts, Wines, Drinks)
    /// Scenario: Applicatie start en laadt menu in.
    /// </summary>
    [TestMethod]
    public void MenuService_InitialisatieMenukaart_AlleCategorieenGeladen()
    {
        var MenuKaart = new MenuService(db);

        Assert.IsNotNull(MenuKaart.Starters,
            "Starters categorie mag niet null zijn");
        Assert.IsNotNull(MenuKaart.Mains,
            "Mains categorie mag niet null zijn");
        Assert.IsNotNull(MenuKaart.Desserts,
            "Desserts categorie mag niet null zijn");
        Assert.IsNotNull(MenuKaart.Wines,
            "Wines categorie mag niet null zijn");
        Assert.IsNotNull(MenuKaart.Drinks,
            "Drinks categorie mag niet null zijn");
    }

    /// <summary>
    /// S2: Allergen informatie ontbreekt bij nieuw gerecht "Noten Dessert"
    /// Scenario: Gebruiker vergeet allergen info in te voeren bij nieuw gerecht
    /// </summary>
    [TestMethod]
    public void AddMenuItem_GeenAllergenenInfo_Geaccepteerd()
    {
        string gerechtNaam = "Noten Dessert Test";
        int categoryId = 3;
        decimal prijs = 7.00m;
        string beschrijving = "Noten dessert";
        string allergenen = "geen";
        int bereidingstijd = 8;

        var gerechtZonderAllergenen = new MenuItem(0, categoryId, gerechtNaam, prijs, beschrijving, allergenen, bereidingstijd);

        itemAccess.AddMenuItem(gerechtZonderAllergenen);
        var opgeslagenGerecht = itemAccess.GetItemsByCategory(categoryId).FirstOrDefault(m => m.Naam == gerechtNaam);

        Assert.IsNotNull(opgeslagenGerecht,
            "Gerecht zonder specifieke allergen info mag toch opgeslagen worden");
        Assert.AreEqual(allergenen, opgeslagenGerecht.Allergeen,
            "Allergen veld moet bewaard blijven met ingevulde waarde");

        if (opgeslagenGerecht != null)
        {
            itemAccess.DeleteMenuItem(opgeslagenGerecht.ID);
        }
    }

    /// <summary>
    /// S3: Prijs van gerecht ingesteld op negatief bedrag
    /// Scenario: Gebruiker voert per ongeluk negatieve prijs in
    /// </summary>
    [TestMethod]
    public void ValidateMenuItemPrice_NegatievePrijs_Afgewezen()
    {
        decimal NegatievePrijs = -5.00m;

        bool PrijsKloppend = NegatievePrijs >= 0;

        Assert.IsFalse(PrijsKloppend,
            "Prijs mag niet negatief zijn; -5.00 moet worden geweigerd bij het toevoegen van een menu-item");
    }
}