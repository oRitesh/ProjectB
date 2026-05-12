using System.Globalization;

public class AdminMenuUI
{
    private readonly MenuItemAccess menuItemAccess;
    private readonly ReserveringAccess reserveringAccess;
    private readonly TijdslotAccess tijdslotAccess;
    private readonly MenuCategorieAccess menuCategorieAccess;
    public AdminMenuUI()
    {
        this.menuItemAccess = new MenuItemAccess(new DatabaseContext());
        this.tijdslotAccess = new TijdslotAccess(new DatabaseContext());
        this.reserveringAccess = new ReserveringAccess(new DatabaseContext());
        this.menuCategorieAccess = new MenuCategorieAccess(new DatabaseContext());
    }

    // ─────────────────────────────────────────────
    //  Hulpfunctie: toon een menu met pijltjestoetsen
    // ─────────────────────────────────────────────
    private int? ToonKeuzeMenu(string titel, List<string> opties)
    {
        int geselecteerd = 0;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("==================================");
            Console.WriteLine($"  {titel.PadRight(32)}");
            Console.WriteLine("==================================");
            Console.WriteLine();
            Console.WriteLine("Gebruik \u2191 en \u2193 om te kiezen.");
            Console.WriteLine("Druk op Enter om te bevestigen.");
            Console.WriteLine("Druk op Escape om terug te gaan.");
            Console.WriteLine();

            for (int i = 0; i < opties.Count; i++)
            {
                Console.WriteLine(i == geselecteerd ? $"> {opties[i]}" : $"  {opties[i]}");
            }

            ConsoleKeyInfo key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.UpArrow && geselecteerd > 0) geselecteerd--;
            else if (key.Key == ConsoleKey.DownArrow && geselecteerd < opties.Count - 1) geselecteerd++;
            else if (key.Key == ConsoleKey.Enter) return geselecteerd;
            else if (key.Key == ConsoleKey.Escape) return null;
        }
    }

    // ─────────────────────────────────────────────
    //  Hulpfunctie: toon één reservering als kaartje
    // ─────────────────────────────────────────────
    private void ToonReserveringKaart(Reservering r, int nummer)
    {
        CultureInfo nl = new CultureInfo("nl-NL");
        string startTijd = DateTime.TryParse(r.StartTijd, out DateTime st) ? st.ToString("HH:mm") : r.StartTijd;
        string eindTijd  = DateTime.TryParse(r.EindTijd,  out DateTime et) ? et.ToString("HH:mm") : r.EindTijd;
        string datum     = DateTime.TryParse(r.StartTijd, out DateTime sd) ? sd.ToString("dd MMMM yyyy", nl) : "";
        string opmerking = string.IsNullOrWhiteSpace(r.Opmerking) ? "–" : r.Opmerking;

        Console.WriteLine($"  ┌─ Reservering #{nummer} ──────────────────────────┐");
        Console.WriteLine($"  │  Datum      : {datum,-28}│");
        Console.WriteLine($"  │  Tijdslot   : {startTijd} – {eindTijd,-20}│");
        Console.WriteLine($"  │  Tafel      : #{r.TafelID,-27}│");
        Console.WriteLine($"  │  Gasten     : {r.AantalGasten,-28}│");
        Console.WriteLine($"  │  Opmerking  : {opmerking,-28}│");
        Console.WriteLine($"  │  Geboekt op : {r.GemaaktOp,-28}│");
        Console.WriteLine($"  └───────────────────────────────────────────┘");
        Console.WriteLine();
    }

    // ─────────────────────────────────────────────
    //  Hoofdmenu
    // ─────────────────────────────────────────────
    public void ShowAdminMenu()
    {
        List<string> opties = new List<string>
        {
            "Wijzig menukaart",
            "Bekijk alle reserveringen",
            "Bekijk reserveringen per tijdslot",
            "Terug naar hoofdmenu"
        };

        int? keuze = ToonKeuzeMenu("ADMIN MENU", opties);

        switch (keuze)
        {
            case 0: EditMenu(); break;
            case 1: ViewReservations(); break;
            case 2: ViewReservationsPerTimeSlot(); break;
            case 3: break;   // Uitloggen – terug naar aanroeper
            case null: break; // Escape
        }
    }

    // ─────────────────────────────────────────────
    //  Menukaart bewerken
    // ─────────────────────────────────────────────
    public void EditMenu()
    {
        List<string> opties = new List<string>
        {
            "Voeg menu-item toe",
            "Werk menu-item bij",
            "Verwijder menu-item",
            "Terug naar Admin Menu"
        };

        int? keuze = ToonKeuzeMenu("WIJZIG MENUKAART", opties);

        switch (keuze)
        {
            case 0:
            {
                // Kies categorie op naam
                var categorieen = menuCategorieAccess.GetAllCategories();
                if (categorieen.Count == 0)
                {
                    Console.Clear();
                    Console.WriteLine("Er zijn geen categorieën beschikbaar.");
                    Console.WriteLine("Druk op een toets om verder te gaan...");
                    Console.ReadKey(true);
                    break;
                }
                int? catKeuze = ToonKeuzeMenu("KIES CATEGORIE", categorieen.Select(c => c.Naam).ToList());
                if (catKeuze == null) break;
                int catID = categorieen[catKeuze.Value].ID;
                string catNaam = categorieen[catKeuze.Value].Naam;

                Console.Clear();
                Console.WriteLine("== Voeg menu-item toe ==");
                Console.Write("Naam         : ");
                string naam = Console.ReadLine() ?? "";

                Console.Write("Prijs        : ");
                decimal prijs = decimal.Parse(Console.ReadLine() ?? "0");

                Console.Write("Beschrijving : ");
                string beschrijving = Console.ReadLine() ?? "";

                Console.Write("Allergenen   : ");
                string allergeen = Console.ReadLine() ?? "";

                menuItemAccess.AddMenuItem(new MenuItem { Naam = naam, Prijs = prijs, MenuCatogorieID = catID, Beschrijving = beschrijving, Allergeen = allergeen });

                Console.Clear();
                Console.WriteLine("Item toegevoegd.");
                Console.WriteLine($"  Naam        : {naam}");
                Console.WriteLine($"  Prijs       : €{prijs:F2}");
                Console.WriteLine($"  Categorie   : {catNaam}");
                Console.WriteLine($"  Beschrijving: {beschrijving}");
                Console.WriteLine($"  Allergenen  : {allergeen}");
                Console.WriteLine();
                Console.WriteLine("Druk op een toets om verder te gaan...");
                Console.ReadKey(true);
                break;
            }

            case 1:
            {
                // Kies item op naam
                var items = menuItemAccess.GetAllMenuItems();
                if (items.Count == 0)
                {
                    Console.Clear();
                    Console.WriteLine("Er zijn geen menu-items beschikbaar.");
                    Console.WriteLine("Druk op een toets om verder te gaan...");
                    Console.ReadKey(true);
                    break;
                }
                int? itemKeuze = ToonKeuzeMenu("KIES ITEM OM TE WIJZIGEN", items.Select(i => $"{i.Naam}  (€{i.Prijs:F2})").ToList());
                if (itemKeuze == null) break;
                MenuItem gekozenItem = items[itemKeuze.Value];

                // Kies nieuwe categorie op naam
                var categorieen = menuCategorieAccess.GetAllCategories();
                int? catKeuze = ToonKeuzeMenu("KIES NIEUWE CATEGORIE", categorieen.Select(c => c.Naam).ToList());
                if (catKeuze == null) break;
                int updateCatID = categorieen[catKeuze.Value].ID;
                string updateCatNaam = categorieen[catKeuze.Value].Naam;

                Console.Clear();
                Console.WriteLine($"== Wijzig: {gekozenItem.Naam} ==");
                Console.WriteLine("(Laat leeg om de huidige waarde te bewaren)");
                Console.WriteLine();

                Console.Write($"Naam         [{gekozenItem.Naam}]: ");
                string input = Console.ReadLine() ?? "";
                string updateNaam = string.IsNullOrWhiteSpace(input) ? gekozenItem.Naam : input;

                Console.Write($"Prijs        [€{gekozenItem.Prijs:F2}]: ");
                input = Console.ReadLine() ?? "";
                decimal updatePrijs = string.IsNullOrWhiteSpace(input) ? gekozenItem.Prijs : decimal.Parse(input);

                Console.Write($"Beschrijving [{gekozenItem.Beschrijving}]: ");
                input = Console.ReadLine() ?? "";
                string updateBeschrijving = string.IsNullOrWhiteSpace(input) ? gekozenItem.Beschrijving : input;

                Console.Write($"Allergenen   [{gekozenItem.Allergeen}]: ");
                input = Console.ReadLine() ?? "";
                string updateAllergeen = string.IsNullOrWhiteSpace(input) ? gekozenItem.Allergeen : input;

                menuItemAccess.UpdateMenuItem(new MenuItem { ID = gekozenItem.ID, Naam = updateNaam, Prijs = updatePrijs, MenuCatogorieID = updateCatID, Beschrijving = updateBeschrijving, Allergeen = updateAllergeen });

                Console.Clear();
                Console.WriteLine("Item bijgewerkt.");
                Console.WriteLine($"  Naam        : {updateNaam}");
                Console.WriteLine($"  Prijs       : €{updatePrijs:F2}");
                Console.WriteLine($"  Categorie   : {updateCatNaam}");
                Console.WriteLine($"  Beschrijving: {updateBeschrijving}");
                Console.WriteLine($"  Allergenen  : {updateAllergeen}");
                Console.WriteLine();
                Console.WriteLine("Druk op een toets om verder te gaan...");
                Console.ReadKey(true);
                break;
            }

            case 2:
            {
                // Kies item op naam
                var items = menuItemAccess.GetAllMenuItems();
                if (items.Count == 0)
                {
                    Console.Clear();
                    Console.WriteLine("Er zijn geen menu-items beschikbaar.");
                    Console.WriteLine("Druk op een toets om verder te gaan...");
                    Console.ReadKey(true);
                    break;
                }
                int? itemKeuze = ToonKeuzeMenu("KIES ITEM OM TE VERWIJDEREN", items.Select(i => $"{i.Naam}  (€{i.Prijs:F2})").ToList());
                if (itemKeuze == null) break;
                MenuItem teVerwijderen = items[itemKeuze.Value];

                // Bevestiging
                int? bevestig = ToonKeuzeMenu($"VERWIJDER '{teVerwijderen.Naam}'?", new List<string> { "Ja, verwijderen", "Nee, annuleren" });
                if (bevestig != 0) break;

                menuItemAccess.DeleteMenuItem(teVerwijderen.ID);

                Console.Clear();
                Console.WriteLine($"'{teVerwijderen.Naam}' verwijderd.");
                Console.WriteLine();
                Console.WriteLine("Druk op een toets om verder te gaan...");
                Console.ReadKey(true);
                break;
            }

            case 3:
                ShowAdminMenu();
                break;

            case null:
                ShowAdminMenu();
                break;
        }
    }

    // ─────────────────────────────────────────────
    //  Alle reserveringen
    // ─────────────────────────────────────────────
    public void ViewReservations()
    {
        Console.Clear();
        Console.WriteLine("==================================");
        Console.WriteLine("        ALLE RESERVERINGEN        ");
        Console.WriteLine("==================================");
        Console.WriteLine();

        var reserveringen = reserveringAccess.GetAllReserveringen();

        if (reserveringen.Count == 0)
        {
            Console.WriteLine("  Er zijn nog geen reserveringen.");
        }
        else
        {
            for (int i = 0; i < reserveringen.Count; i++)
            {
                ToonReserveringKaart(reserveringen[i], i + 1);
            }
        }

        Console.WriteLine("Druk op een toets om terug te gaan...");
        Console.ReadKey(true);
    }
    
    // ─────────────────────────────────────────────
    //  Reserveringen per tijdslot
    // ─────────────────────────────────────────────
    public void ViewReservationsPerTimeSlot()
    {
        // Stap 1 – kies datum
        List<string> datums = tijdslotAccess.GetAllDatums();

        if (datums.Count == 0)
        {
            Console.Clear();
            Console.WriteLine("Er zijn geen datums beschikbaar.");
            Console.WriteLine("Druk op een toets om terug te gaan...");
            Console.ReadKey(true);
            return;
        }

        int? datumKeuze = ToonKeuzeMenu("KIES DATUM", datums);
        if (datumKeuze == null) return;

        string gekozenDatum = datums[datumKeuze.Value];

        // Stap 2 – kies tijdslot voor die datum
        List<Tijdslot> tijdsloten = tijdslotAccess.GetTijdslotenByDatum(gekozenDatum);

        if (tijdsloten.Count == 0)
        {
            Console.Clear();
            Console.WriteLine($"Geen tijdsloten gevonden op {gekozenDatum}.");
            Console.WriteLine("Druk op een toets om terug te gaan...");
            Console.ReadKey(true);
            return;
        }

        List<string> tijdslotLabels = tijdsloten
            .Select(ts =>
            {
                string start = DateTime.TryParse(ts.StartTijd, out DateTime s) ? s.ToString("HH:mm") : ts.StartTijd;
                string eind  = DateTime.TryParse(ts.EindTijd,  out DateTime e) ? e.ToString("HH:mm") : ts.EindTijd;
                return $"{start} – {eind}";
            })
            .ToList();

        int? tsKeuze = ToonKeuzeMenu($"TIJDSLOT  ({gekozenDatum})", tijdslotLabels);
        if (tsKeuze == null) return;

        Tijdslot geselecteerd = tijdsloten[tsKeuze.Value];

        // Stap 3 – toon reserveringen voor dit tijdslot
        Console.Clear();
        Console.WriteLine("==================================");
        Console.WriteLine($"  RESERVERINGEN  {tijdslotLabels[tsKeuze.Value],-18}");
        Console.WriteLine($"  Datum: {geselecteerd.Datum,-27}");
        Console.WriteLine("==================================");
        Console.WriteLine();

        var reserveringen = reserveringAccess.GetReserveringenVoorDatum(geselecteerd.Datum);

        if (reserveringen.Count == 0)
        {
            Console.WriteLine("  Geen reserveringen voor dit tijdslot.");
        }
        else
        {
            for (int i = 0; i < reserveringen.Count; i++)
            {
                ToonReserveringKaart(reserveringen[i], i + 1);
            }
        }

        Console.WriteLine("Druk op een toets om terug te gaan...");
        Console.ReadKey(true);
    }
}