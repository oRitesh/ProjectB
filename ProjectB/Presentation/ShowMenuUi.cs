public class ShowMenuUI
{
    private readonly MenuService menuService;

    public ShowMenuUI(DatabaseContext db)
    {
        menuService = new MenuService(db);
    }
    public void ShowMenuPage()
    {
        bool viewingMenu = true;

        while (viewingMenu)
        {
            List<MenuKaartOption> opties = new()
            {
                MenuKaartOption.Voorgerechten,
                MenuKaartOption.Hoofdgerechten,
                MenuKaartOption.Desserts,
                MenuKaartOption.Dranken,
                MenuKaartOption.Wijnkaart
            };

            MenuKaartOption? keuze = ArrowMenu.ShowMenu(
                "MENUKAART",
                opties,
                x => x switch
                {
                    MenuKaartOption.Voorgerechten => "Voorgerechten",
                    MenuKaartOption.Hoofdgerechten => "Hoofdgerechten",
                    MenuKaartOption.Desserts => "Desserts",
                    MenuKaartOption.Dranken => "Dranken",
                    MenuKaartOption.Wijnkaart => "Wijnkaart",
                    _ => ""
                }
            );

            if (keuze == null)
            {
                viewingMenu = false;
                continue;
            }

            switch (keuze)
            {
                case MenuKaartOption.Voorgerechten:
                    ShowCategory("VOORGERECHTEN", menuService.Starters);
                    break;
                case MenuKaartOption.Hoofdgerechten:
                    ShowCategory("HOOFDGERECHTEN", menuService.Mains);
                    break;
                case MenuKaartOption.Desserts:
                    ShowCategory("DESSERTS", menuService.Desserts);
                    break;
                case MenuKaartOption.Dranken:
                    ShowCategory("DRANKEN", menuService.Drinks);
                    break;
                case MenuKaartOption.Wijnkaart:
                    ShowCategory("WIJNKAART", menuService.Wines);
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
            Console.WriteLine($"Allergenen: {item.Allergeen}");
            Console.WriteLine("----------------------------------");
        }

        Console.WriteLine("Druk op een toets om terug te gaan...");
        Console.ReadKey(true);
    }
}