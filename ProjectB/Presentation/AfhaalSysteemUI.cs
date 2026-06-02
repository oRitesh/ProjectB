public class AfhaalSysteemUI
{
    private readonly AfhaalSysteemLogic logic;
    private readonly MenuService menuService;
    private readonly DatabaseContext db;
    private int gebruikerID;

    // Gastgegevens
    private string gastNaam = "";
    private string gastTelefoon = "";

    public AfhaalSysteemUI(DatabaseContext db, Gebruiker gebruiker)
    {
        logic = new AfhaalSysteemLogic(db);
        menuService = new MenuService(db);
        this.db = db;
        this.gebruikerID = gebruiker.ID;
    }

    public void Start()
    {
        bool bezig = true;

        while (bezig)
        {
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
            x => $"{x.Naam}  -  €{x.Prijs:0.00}  -  {x.BereidingsTijd} min",
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

        Console.Clear();
        Console.WriteLine("==================================");
        Console.WriteLine($"  {gekozen.Naam}");
        Console.WriteLine("==================================");
        Console.WriteLine($"Prijs      : €{gekozen.Prijs:0.00}");
        Console.WriteLine($"Beschrijving: {gekozen.Beschrijving}");
        Console.WriteLine($"Allergenen : {gekozen.Allergeen}");
        Console.WriteLine();

        var bevestig = new List<string> { "Toevoegen aan bestelling", "Terug" };
        string? actie = ArrowMenu.ShowMenu("", bevestig, x => x, showHeader: false);

        if (actie == "Toevoegen aan bestelling")
        {
            logic.VoegToe(gekozen);
        }
    }

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

            int index = regels.IndexOf(keuze);
            if (index >= 0 && index < logic.Winkelwagen.Count)
                logic.VerwijderItem(index);

            if (logic.Winkelwagen.Count == 0)
            {
                Console.Clear();
                Console.WriteLine("Winkelwagen is nu leeg.");
                Console.WriteLine("Druk op een toets om terug te gaan...");
                Console.ReadKey(true);
                return false;
            }
        }

        if (gebruikerID == 0)
        {
            if (!VraagLoginOfGast())
                return false;
        }

        // Ophaaltijd kiezen
        // Ophaaltijd kiezen
        var tijdOpties = logic.GetOphaalTijdOpties();

        if (tijdOpties.Count == 0)
        {
            Console.Clear();
            Console.WriteLine("==================================");
            Console.WriteLine("  RESTAURANT GESLOTEN");
            Console.WriteLine("==================================");
            Console.WriteLine();
            Console.WriteLine("Het restaurant is gesloten voor afhaalbestellingen.");
            Console.WriteLine();
            Console.WriteLine("Druk op een toets om terug te gaan...");
            Console.ReadKey(true);

            return false;
        }

        string? ophaalTijd = ArrowMenu.ShowMenu("OPHAALTIJD KIEZEN", tijdOpties, x => x);

        if (ophaalTijd == null) return false;

        // Opmerking
        Console.Clear();
        Console.WriteLine("==================================");
        Console.WriteLine("  OPMERKING / ALLERGIEËN");
        Console.WriteLine("==================================");
        Console.WriteLine("Laat een opmerking achter (bijv. allergieën).");
        Console.WriteLine("Druk op Enter om over te slaan.");
        Console.WriteLine();
        Console.Write("Opmerking: ");
        Console.CursorVisible = true;
        string opmerking = Console.ReadLine() ?? "";
        Console.CursorVisible = false;

        // Bevestiging
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
        string? beslissing = ArrowMenu.ShowMenu("", bevestig, x => x, showHeader: false);

        if (beslissing != "Bestelling plaatsen") return false;

        int definitiefID = gebruikerID;

        if (gebruikerID == 0)
        {
            definitiefID = logic.VoegGastToe(gastNaam, gastTelefoon);
        }

        logic.SlaBestellingOp(db, definitiefID, ophaalTijd, opmerking);

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

    private bool VraagLoginOfGast()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("==================================");
            Console.WriteLine("  INLOGGEN OF ALS GAST Doorgaan?  ");
            Console.WriteLine("==================================");
            Console.WriteLine();
            Console.WriteLine("1. Inloggen");
            Console.WriteLine("2. Registreren");
            Console.WriteLine("3. Doorgaan als gast");
            Console.WriteLine("0. Terug");
            Console.WriteLine();
            Console.Write("Maak een keuze: ");

            string? keuze = Console.ReadLine();

            if (keuze == "1" || keuze == "2")
            {
                DatabaseContext db2 = new DatabaseContext();
                UserAccess userAccess = new UserAccess(db2);
                InlogUI inlogUI = new InlogUI(userAccess);
                RegistratieUI registratieUI = new RegistratieUI(userAccess);

                Gebruiker? user = null;

                if (keuze == "1")
                    user = inlogUI.Login();
                else
                    user = registratieUI.Registreer();

                if (user != null)
                {
                    gebruikerID = user.ID;
                    db2.Close();
                    return true;
                }

                db2.Close();
            }
            else if (keuze == "3")
            {
                VulGastGegevensIn();
                return true;
            }
            else if (keuze == "0")
            {
                return false;
            }
            else
            {
                Console.WriteLine("Ongeldige keuze. Druk op een toets om opnieuw te proberen...");
                Console.ReadKey(true);
            }
        }
    }

    private void VulGastGegevensIn()
    {
        Console.Clear();
        Console.WriteLine("=== Gastgegevens ===");
        Console.Write("Naam: ");
        gastNaam = Console.ReadLine() ?? "Gast";

        Console.Write("Telefoonnummer: ");
        gastTelefoon = Console.ReadLine() ?? "Onbekend";
    }
}
