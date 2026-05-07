public class AdminMenuUI
{
    private readonly MenuItemAccess menuItemAccess;
    private readonly ReserveringAccess reserveringAccess;
    private readonly TijdslotAccess tijdslotAccess;

    public AdminMenuUI(DatabaseContext db)
    {
        this.menuItemAccess = new MenuItemAccess(db);
        this.tijdslotAccess = new TijdslotAccess(db);
        this.reserveringAccess = new ReserveringAccess(db);
    }

    public void ShowAdminMenu()
    {
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
        }
    }

    public void EditMenu()
    {
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
        var reserveringen = reserveringAccess.GetAllReserveringen();

        if (reserveringen.Count == 0)
        {
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
        }
        else
        {
            Console.WriteLine("Ongeldig tijdslot ID.");
        }

        Console.WriteLine("Druk op een toets om verder te gaan...");
        Console.ReadKey(true);
    }

    private void PrintReservering(Reservering reservering)
    {
        Console.WriteLine($"ID: {reservering.ID}, GebruikerID: {reservering.GebruikerID}, TafelID: {reservering.TafelID}, StartTijd: {reservering.StartTijd}, EindTijd: {reservering.EindTijd}, AantalGasten: {reservering.AantalGasten}, Opmerking: {reservering.Opmerking}, GemaaktOp: {reservering.GemaaktOp}");
    }
}