public class MenuService
{
    public List<MenuItem> Starters { get; } = new();
    public List<MenuItem> Mains { get; } = new();
    public List<MenuItem> Desserts { get; } = new();
    public List<MenuItem> Drinks { get; } = new();
    public List<MenuItem> Wines { get; } = new();

    public MenuService()
    {
        // Voorgerechten (MenuCategorieID = 1)
        Starters.Add(new MenuItem(1, 1, "Bruschetta Classica", 6, "Geroosterd brood met tomaat, basilicum en knoflook", "Gluten"));
        Starters.Add(new MenuItem(2, 1, "Carpaccio di Manzo", 11, "Dungesneden rundvlees, Parmezaan, pijnboompitten, truffelmayonaise", "Lactose, Noten"));
        Starters.Add(new MenuItem(3, 1, "Garnalencocktail", 9, "Hollandse garnalen met cocktailsaus", "Schaaldieren, Ei"));
        Starters.Add(new MenuItem(4, 1, "Geitenkaassalade", 8, "Warme geitenkaas, honing, walnoten, rucola", "Lactose, Noten"));
        Starters.Add(new MenuItem(5, 1, "Tomatensoep", 5, "Huisgemaakte soep van romige tomaten met room", "Lactose"));

        // Hoofdgerechten (MenuCategorieID = 2)
        Mains.Add(new MenuItem(6, 2, "Pasta Carbonara", 14, "Romige saus met spek, Parmezaan en ei", "Gluten, Ei, Lactose"));
        Mains.Add(new MenuItem(7, 2, "Zalmfilet met Citroenboter", 18, "Gegrilde zalm met citroen-botersaus en groenten", "Vis, Lactose"));
        Mains.Add(new MenuItem(8, 2, "Ribeye Steak 250g", 22, "Gegrilde ribeye met kruidenboter en friet", "Lactose"));

        // Desserts (MenuCategorieID = 3)
        Desserts.Add(new MenuItem(9, 3, "Tiramisu", 7, "Klassiek Italiaans dessert met mascarpone en koffie", "Gluten, Ei, Lactose"));
        Desserts.Add(new MenuItem(10, 3, "Crème Brûlée", 6, "Vanille custard met gekarameliseerde suikerlaag", "Ei, Lactose"));
        Desserts.Add(new MenuItem(11, 3, "Chocolademousse", 6, "Luchtige mousse van pure chocolade", "Ei, Lactose"));
        Desserts.Add(new MenuItem(12, 3, "Dame Blanche", 6, "Vanille-ijs met warme chocoladesaus", "Lactose"));

        // Wijn (MenuCategorieID = 4)
        Wines.Add(new MenuItem(13, 4, "Merlot – Frankrijk", 5, "Ronde, zachte rode wijn", "Geen"));
        Wines.Add(new MenuItem(14, 4, "Cabernet Sauvignon – Chili", 6, "Krachtige rode wijn met donkere tonen", "Geen"));
        Wines.Add(new MenuItem(15, 4, "Rioja Crianza – Spanje", 6, "Houtgerijpte rode wijn met vanilletonen", "Geen"));
        Wines.Add(new MenuItem(16, 4, "Pinot Noir – Duitsland", 7, "Lichte, fruitige rode wijn", "Geen"));
        Wines.Add(new MenuItem(17, 4, "Malbec – Argentinië", 6, "Volle rode wijn met pruim en cacao", "Geen"));
        Wines.Add(new MenuItem(18, 4, "Sauvignon Blanc – Nieuw-Zeeland", 6, "Frisse witte wijn met citrus", "Geen"));
        Wines.Add(new MenuItem(19, 4, "Chardonnay – Frankrijk", 6, "Romige witte wijn met vanille", "Geen"));
        Wines.Add(new MenuItem(20, 4, "Pinot Grigio – Italië", 5, "Lichte, frisse witte wijn", "Geen"));
        Wines.Add(new MenuItem(21, 4, "Rosé Provence – Frankrijk", 6, "Droge rosé met rood fruit", "Geen"));
        Wines.Add(new MenuItem(22, 4, "Prosecco – Italië", 7, "Mousserende wijn, licht zoet", "Geen"));

        // Dranken (MenuCategorieID = 5)
        Drinks.Add(new MenuItem(23, 5, "Cola", 2, "Frisdrank", "Geen"));
        Drinks.Add(new MenuItem(24, 5, "Cola Zero", 2, "Frisdrank zonder suiker", "Geen"));
        Drinks.Add(new MenuItem(25, 5, "Fanta", 2, "Sinaasappelfrisdrank", "Geen"));
        Drinks.Add(new MenuItem(26, 5, "Sprite", 2, "Citroen-limoen frisdrank", "Geen"));
        Drinks.Add(new MenuItem(27, 5, "Spa Blauw", 2, "Plat mineraalwater", "Geen"));
        Drinks.Add(new MenuItem(28, 5, "Spa Rood", 2, "Bruisend mineraalwater", "Geen"));
        Drinks.Add(new MenuItem(29, 5, "Verse jus d’orange", 3, "Vers geperst sinaasappelsap", "Geen"));
        Drinks.Add(new MenuItem(30, 5, "Iced Tea", 2, "IJsthee", "Geen"));
        Drinks.Add(new MenuItem(31, 5, "Koffie", 2, "Zwarte koffie", "Geen"));
        Drinks.Add(new MenuItem(32, 5, "Cappuccino", 3, "Koffie met melkschuim", "Lactose"));
    }
}
