public class AfhaalSysteemUI
{
    private readonly AfhaalSysteemLogic logic;
    private readonly MenuService menuService;

    public AfhaalSysteemUI(DatabaseContext db)
    {
        logic = new AfhaalSysteemLogic();
        menuService = new MenuService(db);
    }

    public void Start()
    {
        bool bezig = true;

        while (bezig)
        {
            // Toon winkelwagen samenvatting als extraInfo boven het menu
            var categorieen = new List<string>
            {
                "Voorgerechten",
                "Hoofdgerechten",
                "Desserts",
                "Dranken",
                "Wijnkaart",
                "---",
                "Bekijk bestelling & afronden"
            };

            string? keuze = ArrowMenu.ShowMenu(
                "AFHAALBESTELLING",
                categorieen,
                x => x,
                () =>
                {
                    if (logic.Winkelwagen.Count > 0)
                    {
                        Console.WriteLine($"Winkelwagen: {logic.Winkelwagen.Count} item(s)  |  Totaal: €{logic.BerekenTotaal():0.00}");
                        Console.WriteLine();
                    }
                }
            );

            if (keuze == null) break;

            switch (keuze)
            {
                case "Voorgerechten":
                    BestelUitCategorie("VOORGERECHTEN", menuService.Starters);
                    break;
                case "Hoofdgerechten":
                    BestelUitCategorie("HOOFDGERECHTEN", menuService.Mains);
                    break;
                case "Desserts":
                    BestelUitCategorie("DESSERTS", menuService.Desserts);
                    break;
                case "Dranken":
                    BestelUitCategorie("DRANKEN", menuService.Drinks);
                    break;
                case "Wijnkaart":
                    BestelUitCategorie("WIJNKAART", menuService.Wines);
                    break;
                case "---":
                    break;
                case "Bekijk bestelling & afronden":
                    bezig = !ToonOverzichtEnAfronden();
                    break;
            }
        }
    }

    private void BestelUitCategorie(string titel, List<MenuItem> items)
    {
        if (items.Count == 0)
        {
            Console.Clear();
            Console.WriteLine("Geen items beschikbaar in deze categorie.");
            Console.WriteLine("Druk op een toets om terug te gaan...");
            Console.ReadKey(true);
            return;
        }

        MenuItem? gekozen = ArrowMenu.ShowMenu(
            titel,
            items,
            x => $"{x.Naam}  -  €{x.Prijs:0.00}",
            () =>
            {
                if (logic.Winkelwagen.Count > 0)
                {
                    Console.WriteLine($"Winkelwagen: {logic.Winkelwagen.Count} item(s)  |  Totaal: €{logic.BerekenTotaal():0.00}");
                    Console.WriteLine();
                }
            }
        );

        if (gekozen == null) return;

        // item detail
        Console.Clear();
        Console.WriteLine("==================================");
        Console.WriteLine($"  {gekozen.Naam}");
        Console.WriteLine("==================================");
        Console.WriteLine($"Prijs      : €{gekozen.Prijs:0.00}");
        Console.WriteLine($"Beschrijving: {gekozen.Beschrijving}");
        Console.WriteLine($"Allergenen : {gekozen.Allergeen}");
        Console.WriteLine();

        var bevestig = new List<string> { "Toevoegen aan bestelling", "Terug" };
        string? actie = ArrowMenu.ShowMenu(
            "",
            bevestig,
            x => x,
            showHeader: false
        );

        if (actie == "Toevoegen aan bestelling")
        {
            logic.VoegToe(gekozen);
        }
    }

    //  true als bestelling is geplaatst (dan stoppen)
    private bool ToonOverzichtEnAfronden()
    {
        if (logic.Winkelwagen.Count == 0)
        {
            Console.Clear();
            Console.WriteLine("Uw winkelwagen is leeg.");
            Console.WriteLine("Druk op een toets om terug te gaan...");
            Console.ReadKey(true);
            return false;
        }

        // Overzicht tonen met optie om item te verwijderen
        while (true)
        {
            var regels = new List<string>();
            foreach (var (item, aantal) in logic.Winkelwagen)
                regels.Add($"[Verwijder]  {item.Naam} x{aantal}  - €{item.Prijs * aantal:0.00}");

            regels.Add("──────────────────────────────────");
            regels.Add("Doorgaan naar afrekenen");
            regels.Add("Annuleer bestelling");

            string? keuze = ArrowMenu.ShowMenu(
                "UW BESTELLING",
                regels,
                x => x,
                () =>
                {
                    Console.WriteLine($"Totaal: €{logic.BerekenTotaal():0.00}");
                    Console.WriteLine();
                    Console.WriteLine("Selecteer een item om te verwijderen, of ga door.");
                    Console.WriteLine();
                }
            );

            if (keuze == null || keuze == "Annuleer bestelling")
                return false;

            if (keuze == "Doorgaan naar afrekenen")
                break;

            // Item verwijderen
            int itemBestaat = regels.IndexOf(keuze);
            if (itemBestaat >= 0 && itemBestaat < logic.Winkelwagen.Count)
                logic.VerwijderItem(itemBestaat);

            if (logic.Winkelwagen.Count == 0)
            {
                Console.Clear();
                Console.WriteLine("Winkelwagen is nu leeg.");
                Console.WriteLine("Druk op een toets om terug te gaan...");
                Console.ReadKey(true);
                return false;
            }
        }

        // Ophaal tijd kiezen
        var tijdOpties = logic.GetOphaalTijdOpties();
        string? ophaalTijd = ArrowMenu.ShowMenu(
            "OPHAALTIJD KIEZEN",
            tijdOpties,
            x => x
        );

        if (ophaalTijd == null) return false;

        // Opmerking / allergie invoer
        Console.Clear();
        Console.WriteLine("==================================");
        Console.WriteLine("  OPMERKING / ALLERGIEËN");
        Console.WriteLine("==================================");
        Console.WriteLine("Laat een opmerking achter (bijv. allergieën).");
        Console.WriteLine("Druk gewoon op Enter om over te slaan.");
        Console.WriteLine();
        Console.Write("Opmerking: ");
        Console.CursorVisible = true;
        string opmerking = Console.ReadLine() ?? "";
        Console.CursorVisible = false;

        // Bevestiging tonen
        Console.Clear();
        Console.WriteLine("==================================");
        Console.WriteLine("  BESTELLINGSOVERZICHT");
        Console.WriteLine("==================================");
        Console.WriteLine();

        foreach (var (item, aantal) in logic.Winkelwagen)
            Console.WriteLine($"  {item.Naam} x{aantal}  - €{item.Prijs * aantal:0.00}");

        Console.WriteLine();
        Console.WriteLine($"  Totaal       : €{logic.BerekenTotaal():0.00}");
        Console.WriteLine($"  Ophaaltijd   : {ophaalTijd}");
        if (!string.IsNullOrWhiteSpace(opmerking))
            Console.WriteLine($"  Opmerking    : {opmerking}");

        Console.WriteLine();

        var bevestig = new List<string> { "Bestelling plaatsen", "Terug" };
        string? beslissing = ArrowMenu.ShowMenu(
            "",
            bevestig,
            x => x,
            showHeader: false
        );

        if (beslissing != "Bestelling plaatsen") return false;

        // Bevestiging
        Console.Clear();
        Console.WriteLine("==================================");
        Console.WriteLine("  BESTELLING GEPLAATST!");
        Console.WriteLine("==================================");
        Console.WriteLine();
        Console.WriteLine($"  Uw bestelling wordt klaargemaakt.");
        Console.WriteLine($"  Ophaaltijd: {ophaalTijd}");
        Console.WriteLine();
        Console.WriteLine("Druk op een toets om terug te gaan naar het hoofdmenu...");
        Console.ReadKey(true);

        return true;
    }
}