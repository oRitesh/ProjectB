public class MenuService
{
    public List<MenuItem> Starters { get; } = new();
    public List<MenuItem> Mains { get; } = new();
    public List<MenuItem> Desserts { get; } = new();
    public List<MenuItem> Drinks { get; } = new();
    public List<MenuItem> Wines { get; } = new();

    public MenuService()
    {
        // Voorgerechten//
        Starters.Add(new MenuItem("Bruschetta Classica", "Geroosterd brood met tomaat, basilicum en knoflook", 6.50m, "Gluten"));
        Starters.Add(new MenuItem("Carpaccio di Manzo", "Dungesneden rundvlees, Parmezaan, pijnboompitten, truffelmayonaise", 11.50m, "Lactose, Noten"));
        Starters.Add(new MenuItem("Garnalencocktail", "Hollandse garnalen met cocktailsaus", 9.00m, "Schaaldieren, Ei"));
        Starters.Add(new MenuItem("Geitenkaassalade", "Warme geitenkaas, honing, walnoten, rucola", 8.50m, "Lactose, Noten"));
        Starters.Add(new MenuItem("Tomatensoep", "Huisgemaakte soep van romige tomaten met room", 5.50m, "Lactose"));

        //Hoofdgerechten//
        Mains.Add(new MenuItem("Pasta Carbonara", "Romige saus met spek, Parmezaan en ei", 14.00m, "Gluten, Ei, Lactose"));
        Mains.Add(new MenuItem("Zalmfilet met Citroenboter", "Gegrilde zalm met citroen-botersaus en groenten", 18.50m, "Vis, Lactose"));
        Mains.Add(new MenuItem("Ribeye Steak 250g", "Gegrilde ribeye met kruidenboter en friet", 22.00m, "Lactose"));

        //desserts//
        Desserts.Add(new MenuItem("Tiramisu", "Klassiek Italiaans dessert met mascarpone en koffie", 7.00m, "Gluten, Ei, Lactose"));
        Desserts.Add(new MenuItem("Crème Brûlée", "Vanille custard met gekarameliseerde suikerlaag", 6.50m, "Ei, Lactose"));
        Desserts.Add(new MenuItem("Chocolademousse", "Luchtige mousse van pure chocolade", 6.00m, "Ei, Lactose"));
        Desserts.Add(new MenuItem("Dame Blanche", "Vanille-ijs met warme chocoladesaus", 6.50m, "Lactose"));

        //Wijn//
        Wines.Add(new MenuItem("Merlot – Frankrijk", "Ronde, zachte rode wijn", 5.50m, "Geen"));
        Wines.Add(new MenuItem("Cabernet Sauvignon – Chili", "Krachtige rode wijn met donkere tonen", 6.00m, "Geen"));
        Wines.Add(new MenuItem("Rioja Crianza – Spanje", "Houtgerijpte rode wijn met vanilletonen", 6.50m, "Geen"));
        Wines.Add(new MenuItem("Pinot Noir – Duitsland", "Lichte, fruitige rode wijn", 7.00m, "Geen"));
        Wines.Add(new MenuItem("Malbec – Argentinië", "Volle rode wijn met pruim en cacao", 6.50m, "Geen"));
        Wines.Add(new MenuItem("Sauvignon Blanc – Nieuw-Zeeland", "Frisse witte wijn met citrus", 6.00m, "Geen"));
        Wines.Add(new MenuItem("Chardonnay – Frankrijk", "Romige witte wijn met vanille", 6.50m, "Geen"));
        Wines.Add(new MenuItem("Pinot Grigio – Italië", "Lichte, frisse witte wijn", 5.50m, "Geen"));
        Wines.Add(new MenuItem("Rosé Provence – Frankrijk", "Droge rosé met rood fruit", 6.00m, "Geen"));
        Wines.Add(new MenuItem("Prosecco – Italië", "Mousserende wijn, licht zoet", 7.00m, "Geen"));

        //dranken//
        Drinks.Add(new MenuItem("Cola", "Frisdrank", 2.80m, "Geen"));
        Drinks.Add(new MenuItem("Cola Zero", "Frisdrank zonder suiker", 2.80m, "Geen"));
        Drinks.Add(new MenuItem("Fanta", "Sinaasappelfrisdrank", 2.80m, "Geen"));
        Drinks.Add(new MenuItem("Sprite", "Citroen-limoen frisdrank", 2.80m, "Geen"));
        Drinks.Add(new MenuItem("Spa Blauw", "Plat mineraalwater", 2.50m, "Geen"));
        Drinks.Add(new MenuItem("Spa Rood", "Bruisend mineraalwater", 2.50m, "Geen"));
        Drinks.Add(new MenuItem("Verse jus d’orange", "Vers geperst sinaasappelsap", 3.80m, "Geen"));
        Drinks.Add(new MenuItem("Iced Tea", "IJsthee", 2.80m, "Geen"));
        Drinks.Add(new MenuItem("Koffie", "Zwarte koffie", 2.50m, "Geen"));
        Drinks.Add(new MenuItem("Cappuccino", "Koffie met melkschuim", 3.00m, "Lactose"));
    }
}
