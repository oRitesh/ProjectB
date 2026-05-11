public class AdminMenuUI
{
    private readonly MenuItemAccess menuItemAccess;
    private readonly ReserveringAccess reserveringAccess;
    private readonly TijdslotAccess tijdslotAccess;

<<<<<<< HEAD
=======
    private readonly MenuCategorieAccess menuCategorieAccess;
>>>>>>> parent of 53dfb9f (Merge branch 'Admin-Menu' of https://github.com/oRitesh/ProjectB into Admin-Menu)

    public AdminMenuUI()
    {
        this.menuItemAccess = new MenuItemAccess(new DatabaseContext());
        this.tijdslotAccess = new TijdslotAccess(new DatabaseContext());
        this.reserveringAccess = new ReserveringAccess(new DatabaseContext());
<<<<<<< HEAD
    }

=======
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
>>>>>>> parent of 53dfb9f (Merge branch 'Admin-Menu' of https://github.com/oRitesh/ProjectB into Admin-Menu)
    public void ShowAdminMenu()
    {
        Console.WriteLine("Admin Menu:");
        Console.WriteLine("1. Wijzig menukaart");
        Console.WriteLine("2. Bekijk reserveringen");
        Console.WriteLine("3. Bekijk reserveringen per tijdslot");
        Console.WriteLine("4. Uitloggen");

        Console.Write("Maak een keuze: ");
        string input = Console.ReadLine();

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

                break;

            default:

                Console.WriteLine("Ongeldige keuze. Druk op een toets om verder te gaan...");
                Console.ReadKey(true);
                break;
        }
    }

    public void EditMenu()
    {
<<<<<<< HEAD
        Console.WriteLine("Wijzig menukaart:");
        Console.WriteLine("1. Voeg menu item toe");
        Console.WriteLine("2. Werk menu item bij");
        Console.WriteLine("3. Verwijder menu item");
        Console.WriteLine("4. Terug naar Admin Menu");

        Console.Write("Maak een keuze: ");
        string input = Console.ReadLine();

        switch (input)
=======
        List<string> opties = new List<string>
>>>>>>> parent of 53dfb9f (Merge branch 'Admin-Menu' of https://github.com/oRitesh/ProjectB into Admin-Menu)
        {
            case "1":

                Console.WriteLine("Naam:");
                string naam = Console.ReadLine();
                Console.Clear();

                Console.WriteLine("Prijs:");
                decimal prijs = decimal.Parse(Console.ReadLine());
                Console.Clear();

                Console.WriteLine("MenuCategorieID:");
                int menuCategorieID = int.Parse(Console.ReadLine());
                Console.Clear();

                menuItemAccess.AddMenuItem(new MenuItem { Naam = naam, Prijs = prijs, MenuCatogorieID = menuCategorieID });
                
                Console.WriteLine("Item toegevoegd.");
                Console.WriteLine($"Naam: {naam}\nPrijs: {prijs}\nMenuCategorieID: {menuCategorieID}");
                Console.WriteLine("Druk op een toets om verder te gaan...");
                Console.ReadKey(true);

                break;

            case "2":

                Console.WriteLine("ID:");
                int id = int.Parse(Console.ReadLine());
                Console.Clear();

                Console.WriteLine("Naam:");
                string updateNaam = Console.ReadLine();
                Console.Clear();

                Console.WriteLine("Prijs:");
                decimal updatePrijs = decimal.Parse(Console.ReadLine());
                Console.Clear();

                Console.WriteLine("MenuCategorieID:");
                int updateMenuCategorieID = int.Parse(Console.ReadLine());
                Console.Clear();

                menuItemAccess.UpdateMenuItem(new MenuItem { ID = id, Naam = updateNaam, Prijs = updatePrijs, MenuCatogorieID = updateMenuCategorieID });
                
                Console.WriteLine("Item bijgewerkt.");
                Console.WriteLine($"ID: {id}\nNaam: {updateNaam}\nPrijs: {updatePrijs}\nMenuCategorieID: {updateMenuCategorieID}");
                Console.WriteLine("Druk op een toets om verder te gaan...");
                Console.ReadKey(true);

                break;

            case "3":

                Console.WriteLine("ID:");
                int deleteId = int.Parse(Console.ReadLine());
                Console.Clear();

                menuItemAccess.DeleteMenuItem(deleteId);

                Console.WriteLine("Item verwijderd.");
                Console.WriteLine($"ID: {deleteId}");
                Console.WriteLine("Druk op een toets om verder te gaan...");
                Console.ReadKey(true);

                break;

            case "4":

                ShowAdminMenu();
                break;

            default:

                Console.WriteLine("Ongeldige keuze. Druk op een toets om verder te gaan...");
                Console.ReadKey(true);
                break;
        }
    }

<<<<<<< HEAD
    public void ViewReservations()
    {
        Console.WriteLine("Reserveringen:");
=======
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

>>>>>>> parent of 53dfb9f (Merge branch 'Admin-Menu' of https://github.com/oRitesh/ProjectB into Admin-Menu)
        var reserveringen = reserveringAccess.GetAllReserveringen();
        foreach (var reservering in reserveringen)
        {
<<<<<<< HEAD
            Console.WriteLine($"ID: {reservering.ID}, GebruikerID: {reservering.GebruikerID}, TafelID: {reservering.TafelID}, StartTijd: {reservering.StartTijd}, EindTijd: {reservering.EindTijd}, AantalGasten: {reservering.AantalGasten}, Opmerking: {reservering.Opmerking}, GemaaktOp: {reservering.GemaaktOp}");
        }
        Console.WriteLine("Druk op een toets om verder te gaan...");
        Console.ReadKey(true);
    }

    public void ViewReservationsPerTimeSlot()
    {
        Console.WriteLine("Vul een datum in:");
        string datum = Console.ReadLine();
        Console.Clear();

        tijdslotAccess.GetTijdslotenByDatum(datum).ForEach(ts =>
        {
            Console.WriteLine($"Tijdslot ID: {ts.ID}, Datum: {ts.Datum}, StartTijd: {ts.StartTijd}, EindTijd: {ts.EindTijd}");
            var reserveringen = reserveringAccess.GetReserveringenVoorDatum(ts.Datum);
            foreach (var reservering in reserveringen)
            {
                Console.WriteLine($"\tReservering ID: {reservering.ID}, GebruikerID: {reservering.GebruikerID}, TafelID: {reservering.TafelID}, StartTijd: {reservering.StartTijd}, EindTijd: {reservering.EindTijd}, AantalGasten: {reservering.AantalGasten}, Opmerking: {reservering.Opmerking}, GemaaktOp: {reservering.GemaaktOp}");
            }
        });

        Console.WriteLine("Selecteer een tijdslot:");
        int tijdslotId = int.Parse(Console.ReadLine());
        Console.Clear();

        var geselecteerdTijdslot = tijdslotAccess.GetTijdslotByID(tijdslotId);
        if (geselecteerdTijdslot != null)
        {
            Console.WriteLine($"Geselecteerd Tijdslot ID: {geselecteerdTijdslot.ID}, Datum: {geselecteerdTijdslot.Datum}, StartTijd: {geselecteerdTijdslot.StartTijd}, EindTijd: {geselecteerdTijdslot.EindTijd}");
            var reserveringen = reserveringAccess.GetReserveringenVoorDatum(geselecteerdTijdslot.Datum);
            foreach (var reservering in reserveringen)
            {
                Console.WriteLine($"\tReservering ID: {reservering.ID}, GebruikerID: {reservering.GebruikerID}, TafelID: {reservering.TafelID}, StartTijd: {reservering.StartTijd}, EindTijd: {reservering.EindTijd}, AantalGasten: {reservering.AantalGasten}, Opmerking: {reservering.Opmerking}, GemaaktOp: {reservering.GemaaktOp}");
            }
=======
            Console.WriteLine("  Er zijn nog geen reserveringen.");
>>>>>>> parent of 53dfb9f (Merge branch 'Admin-Menu' of https://github.com/oRitesh/ProjectB into Admin-Menu)
        }
        else
        {
            Console.WriteLine("Ongeldig tijdslot ID.");
        }

        Console.WriteLine("Druk op een toets om verder te gaan...");
        Console.ReadKey(true);
    }
}