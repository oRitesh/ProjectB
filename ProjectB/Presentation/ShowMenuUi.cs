public class ShowMenuUI
{
    private readonly MenuService menuService;

    public ShowMenuUI()
    {
        menuService = new MenuService();
    }

    public void ShowMenuPage()
    {
        bool viewingMenu = true;

        while (viewingMenu)
        {
            Console.Clear();
            Console.WriteLine("==================================");
            Console.WriteLine("            MENUKAART             ");
            Console.WriteLine("==================================");
            Console.WriteLine("1. Voorgerechten");
            Console.WriteLine("2. Hoofdgerechten");
            Console.WriteLine("3. Desserts");
            Console.WriteLine("4. Dranken");
            Console.WriteLine("5. Wijnkaart");
            Console.WriteLine("0. Terug");
            Console.WriteLine();
            Console.Write("Maak een keuze: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ShowCategory("VOORGERECHTEN", menuService.Starters);
                    break;
                case "2":
                    ShowCategory("HOOFDGERECHTEN", menuService.Mains);
                    break;
                case "3":
                    ShowCategory("DESSERTS", menuService.Desserts);
                    break;
                case "4":
                    ShowCategory("DRANKEN", menuService.Drinks);
                    break;
                case "5":
                    ShowCategory("WIJNKAART", menuService.Wines);
                    break;
                case "0":
                    viewingMenu = false;
                    break;
                default:
                    Console.WriteLine("Ongeldige keuze. Druk op een toets om verder te gaan...");
                    Console.ReadKey(true);
                    break;
            }
        }
    }

    private void ShowCategory(string title, List<MenuItem> items)
    {
        Console.Clear();
        Console.WriteLine("==================================");
        Console.WriteLine($"          {title}          ");
        Console.WriteLine("==================================");

        foreach (var item in items)
        {
            Console.WriteLine($"{item.Naam} - €{item.Prijs:0.00}");
            Console.WriteLine(item.Beschrijving);
            Console.WriteLine($"Allergenen: {item.Allergenen}");
            Console.WriteLine("----------------------------------");
        }

        Console.WriteLine("Druk op een toets om terug te gaan...");
        Console.ReadKey(true);
    }
}