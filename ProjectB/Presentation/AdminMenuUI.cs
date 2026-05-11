using System.Globalization;

public class AdminMenuUI
{
    private readonly MenuItemAccess menuItemAccess;
    private readonly ReserveringAccess reserveringAccess;
    private readonly TijdslotAccess tijdslotAccess;

<<<<<<< HEAD
    private readonly MenuCategorieAccess menuCategorieAccess;

    public AdminMenuUI()
    {
        this.menuItemAccess = new MenuItemAccess(new DatabaseContext());
        this.tijdslotAccess = new TijdslotAccess(new DatabaseContext());
        this.reserveringAccess = new ReserveringAccess(new DatabaseContext());
        this.menuCategorieAccess = new MenuCategorieAccess(new DatabaseContext());
=======
    public AdminMenuUI(DatabaseContext db)
    {
        this.menuItemAccess = new MenuItemAccess(db);
        this.tijdslotAccess = new TijdslotAccess(db);
        this.reserveringAccess = new ReserveringAccess(db);
>>>>>>> 4d049fc5f6569a0bfc0b22c3c8ac440a067965e9
    }

    // ─────────────────────────────────────────────
    //  Hulpfunctie: toon een menu met pijltjestoetsen
    // ─────────────────────────────────────────────
    private int? ToonKeuzeMenu(string titel, List<string> opties)
    {
<<<<<<< HEAD
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
=======
        bool bezig = true;

        while (bezig)
        {
            Console.Clear();
            Console.WriteLine("Admin Menu:");
            Console.WriteLine("1. Wijzig menukaart");
            Console.WriteLine("2. Bekijk reserveringen");
            Console.WriteLine("3. Bekijk reserveringen per tijdslot");
            Console.WriteLine("4. Uitloggen");

            Console.Write("Maak een keuze: ");
            string? input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    EditMenu();
                    break;

                case "2":
                    ViewReservations();
                    break;

                case "3":
                    ViewReservationsPerTimeSlot();
                    break;

                case "4":
                    bezig = false;
                    break;

                default:
                    Console.WriteLine("Ongeldige keuze. Druk op een toets om verder te gaan...");
                    Console.ReadKey(true);
                    break;
            }
>>>>>>> 4d049fc5f6569a0bfc0b22c3c8ac440a067965e9
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

        Console.WriteLine($"  ┌─ Reservering #{nummer} ─────────────────────┐");
        Console.WriteLine($"  │  Datum      : {datum,-28}│");
        Console.WriteLine($"  │  Tijdslot   : {startTijd} – {eindTijd,-22}│");
        Console.WriteLine($"  │  Tafel      : #{r.TafelID,-28}│");
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
<<<<<<< HEAD
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
        Console.WriteLine("     ALLE RESERVERINGEN           ");
        Console.WriteLine("==================================");
        Console.WriteLine();

=======
        bool bezig = true;

        while (bezig)
        {
            Console.Clear();
            Console.WriteLine("Wijzig menukaart:");
            Console.WriteLine("1. Voeg menu item toe");
            Console.WriteLine("2. Werk menu item bij");
            Console.WriteLine("3. Verwijder menu item");
            Console.WriteLine("4. Terug naar Admin Menu");

            Console.Write("Maak een keuze: ");
            string? input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    VoegMenuItemToe();
                    break;

                case "2":
                    WerkMenuItemBij();
                    break;

                case "3":
                    VerwijderMenuItem();
                    break;

                case "4":
                    bezig = false;
                    break;

                default:
                    Console.WriteLine("Ongeldige keuze. Druk op een toets om verder te gaan...");
                    Console.ReadKey(true);
                    break;
            }
        }
    }

    private void VoegMenuItemToe()
    {
        Console.Clear();
        Console.WriteLine("=== Nieuw menu item ===");

        Console.Write("Naam: ");
        string? naam = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(naam))
        {
            Console.WriteLine("Naam mag niet leeg zijn.");
            Console.ReadKey();
            return;
        }

        decimal prijs;
        Console.Write("Prijs: ");
        while (!decimal.TryParse(Console.ReadLine(), out prijs) || prijs < 0)
        {
            Console.WriteLine("Ongeldige prijs. Probeer opnieuw:");
        }

        int categorieID;
        Console.Write("MenuCategorieID: ");
        while (!int.TryParse(Console.ReadLine(), out categorieID) || categorieID <= 0)
        {
            Console.WriteLine("Ongeldige categorie. Probeer opnieuw:");
        }

        Console.Write("Beschrijving: ");
        string? beschrijving = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(beschrijving))
        {
            Console.WriteLine("Beschrijving mag niet leeg zijn.");
            Console.ReadKey();
            return;
        }

        Console.Write("Allergeen: ");
        string? allergeen = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(allergeen))
        {
            Console.WriteLine("Allergeen mag niet leeg zijn.");
            Console.ReadKey();
            return;
        }

        menuItemAccess.AddMenuItem(new MenuItem
        {
            Naam = naam,
            Prijs = prijs,
            MenuCatogorieID = categorieID,
            Beschrijving = beschrijving,
            Allergeen = allergeen
        });

        Console.WriteLine("Item toegevoegd!");
        Console.ReadKey();
    }

    private void WerkMenuItemBij()
    {
        Console.Clear();
        Console.WriteLine("=== Menu item bijwerken ===");

        int id;
        Console.Write("ID: ");
        while (!int.TryParse(Console.ReadLine(), out id) || id <= 0)
        {
            Console.WriteLine("Ongeldig ID. Probeer opnieuw:");
        }

        var bestaandItem = menuItemAccess.GetItemsByCategory(id).FirstOrDefault(i => i.ID == id);
        if (bestaandItem == null)
        {
            Console.WriteLine("Menu item niet gevonden.");
            Console.ReadKey();
            return;
        }

        Console.Write($"Nieuwe naam (huidig: {bestaandItem.Naam}): ");
        string? naam = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(naam)) naam = bestaandItem.Naam;

        decimal prijs;
        Console.Write($"Nieuwe prijs (huidig: {bestaandItem.Prijs}): ");
        string? prijsInput = Console.ReadLine();
        if (!decimal.TryParse(prijsInput, out prijs) || prijs < 0) prijs = bestaandItem.Prijs;

        int categorieID;
        Console.Write($"Nieuwe MenuCategorieID (huidig: {bestaandItem.MenuCatogorieID}): ");
        string? catInput = Console.ReadLine();
        if (!int.TryParse(catInput, out categorieID) || categorieID <= 0) categorieID = bestaandItem.MenuCatogorieID;

        Console.Write($"Nieuwe beschrijving (huidig: {bestaandItem.Beschrijving}): ");
        string? beschrijving = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(beschrijving)) beschrijving = bestaandItem.Beschrijving;

        Console.Write($"Nieuw allergeen (huidig: {bestaandItem.Allergeen}): ");
        string? allergeen = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(allergeen)) allergeen = bestaandItem.Allergeen;

        menuItemAccess.UpdateMenuItem(new MenuItem
        {
            ID = id,
            Naam = naam,
            Prijs = prijs,
            MenuCatogorieID = categorieID,
            Beschrijving = beschrijving,
            Allergeen = allergeen
        });

        Console.WriteLine("Item bijgewerkt!");
        Console.ReadKey();
    }

    private void VerwijderMenuItem()
    {
        Console.Clear();
        Console.WriteLine("=== Menu item verwijderen ===");

        int id;
        Console.Write("ID: ");
        while (!int.TryParse(Console.ReadLine(), out id) || id <= 0)
        {
            Console.WriteLine("Ongeldig ID. Probeer opnieuw:");
        }

        Console.Write("Weet u zeker dat u dit item wilt verwijderen? (j/n): ");
        string? bevestiging = Console.ReadLine();

        if (bevestiging?.ToLower() == "j")
        {
            menuItemAccess.DeleteMenuItem(id);
            Console.WriteLine("Item verwijderd!");
        }
        else
        {
            Console.WriteLine("Verwijderen geannuleerd.");
        }

        Console.ReadKey();
    }

    public void ViewReservations()
    {
        Console.Clear();
        Console.WriteLine("Reserveringen:");
>>>>>>> 4d049fc5f6569a0bfc0b22c3c8ac440a067965e9
        var reserveringen = reserveringAccess.GetAllReserveringen();

        if (reserveringen.Count == 0)
        {
<<<<<<< HEAD
            Console.WriteLine("  Er zijn nog geen reserveringen.");
=======
            Console.WriteLine("Geen reserveringen gevonden.");
        }
        else
        {
            foreach (var reservering in reserveringen)
            {
                PrintReservering(reservering);
            }
        }

        Console.WriteLine("Druk op een toets om verder te gaan...");
        Console.ReadKey(true);
    }

    public void ViewReservationsPerTimeSlot()
    {
        Console.Clear();
        Console.Write("Vul een datum in (yyyy-MM-dd): ");
        string? datum = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(datum))
        {
            Console.WriteLine("Datum mag niet leeg zijn.");
            Console.ReadKey();
            return;
        }

        Console.Clear();

        var tijdsloten = tijdslotAccess.GetTijdslotenByDatum(datum);

        if (tijdsloten.Count == 0)
        {
            Console.WriteLine("Geen tijdsloten gevonden voor deze datum.");
            Console.ReadKey();
            return;
        }

        foreach (var ts in tijdsloten)
        {
            Console.WriteLine($"Tijdslot ID: {ts.ID}, Datum: {ts.Datum}, StartTijd: {ts.StartTijd}, EindTijd: {ts.EindTijd}");

            var reserveringen = reserveringAccess.GetOverlappendeReserveringenVoorTijdslot(ts);

            if (reserveringen.Count == 0)
            {
                Console.WriteLine("\tGeen reserveringen voor dit tijdslot.");
            }
            else
            {
                foreach (var reservering in reserveringen)
                {
                    Console.Write("\t");
                    PrintReservering(reservering);
                }
            }
            Console.WriteLine();
        }

        Console.Write("Selecteer een tijdslot ID (of 0 om terug te gaan): ");
        if (!int.TryParse(Console.ReadLine(), out int tijdslotId) || tijdslotId == 0)
        {
            return;
        }

        Console.Clear();

        var geselecteerdTijdslot = tijdslotAccess.GetTijdslotByID(tijdslotId);
        if (geselecteerdTijdslot != null)
        {
            Console.WriteLine($"Geselecteerd Tijdslot ID: {geselecteerdTijdslot.ID}, Datum: {geselecteerdTijdslot.Datum}, StartTijd: {geselecteerdTijdslot.StartTijd}, EindTijd: {geselecteerdTijdslot.EindTijd}");

            var reserveringen = reserveringAccess.GetOverlappendeReserveringenVoorTijdslot(geselecteerdTijdslot);

            if (reserveringen.Count == 0)
            {
                Console.WriteLine("Geen reserveringen voor dit tijdslot.");
            }
            else
            {
                foreach (var reservering in reserveringen)
                {
                    PrintReservering(reservering);
                }
            }
>>>>>>> 4d049fc5f6569a0bfc0b22c3c8ac440a067965e9
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

    private void PrintReservering(Reservering reservering)
    {
        Console.WriteLine($"ID: {reservering.ID}, GebruikerID: {reservering.GebruikerID}, TafelID: {reservering.TafelID}, StartTijd: {reservering.StartTijd}, EindTijd: {reservering.EindTijd}, AantalGasten: {reservering.AantalGasten}, Opmerking: {reservering.Opmerking}, GemaaktOp: {reservering.GemaaktOp}");
    }
}