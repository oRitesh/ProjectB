public class AdminMenuUI
{
    private readonly MenuItemAccess menuItemAccess;
    private readonly ReserveringAccess reserveringAccess;
    private readonly TijdslotAccess tijdslotAccess;


    public AdminMenuUI()
    {
        this.menuItemAccess = new MenuItemAccess(new DatabaseContext());
        this.tijdslotAccess = new TijdslotAccess(new DatabaseContext());
        this.reserveringAccess = new ReserveringAccess(new DatabaseContext());
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
        string input = Console.ReadLine();

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
    string naam = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(naam))
    {
        Console.WriteLine("Naam mag niet leeg zijn.");
        Console.ReadKey();
        return;
    }

    decimal prijs;
    Console.Write("Prijs: ");
    while (!decimal.TryParse(Console.ReadLine(), out prijs))
    {
        Console.WriteLine("Ongeldige prijs. Probeer opnieuw:");
    }

    int categorieID;
    Console.Write("MenuCategorieID: ");
    while (!int.TryParse(Console.ReadLine(), out categorieID))
    {
        Console.WriteLine("Ongeldige categorie. Probeer opnieuw:");
    }

    menuItemAccess.AddMenuItem(new MenuItem
    {
        Naam = naam,
        Prijs = prijs,
        MenuCatogorieID = categorieID
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
    while (!int.TryParse(Console.ReadLine(), out id))
    {
        Console.WriteLine("Ongeldig ID. Probeer opnieuw:");
    }

    Console.Write("Nieuwe naam: ");
    string naam = Console.ReadLine();

    decimal prijs;
    Console.Write("Nieuwe prijs: ");
    while (!decimal.TryParse(Console.ReadLine(), out prijs))
    {
        Console.WriteLine("Ongeldige prijs. Probeer opnieuw:");
    }

    int categorieID;
    Console.Write("Nieuwe MenuCategorieID: ");
    while (!int.TryParse(Console.ReadLine(), out categorieID))
    {
        Console.WriteLine("Ongeldige categorie. Probeer opnieuw:");
    }

    menuItemAccess.UpdateMenuItem(new MenuItem
    {
        ID = id,
        Naam = naam,
        Prijs = prijs,
        MenuCatogorieID = categorieID
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
    while (!int.TryParse(Console.ReadLine(), out id))
    {
        Console.WriteLine("Ongeldig ID. Probeer opnieuw:");
    }

    menuItemAccess.DeleteMenuItem(id);

    Console.WriteLine("Item verwijderd!");
    Console.ReadKey();
}

    public void ViewReservations()
    {
        Console.WriteLine("Reserveringen:");
        var reserveringen = reserveringAccess.GetAllReserveringen();
        foreach (var reservering in reserveringen)
        {
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
        }
        else
        {
            Console.WriteLine("Ongeldig tijdslot ID.");
        }

        Console.WriteLine("Druk op een toets om verder te gaan...");
        Console.ReadKey(true);
    }
}