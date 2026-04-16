public static class Menu
{
    static void ShowInformationPage()
    {
        Console.Clear();
        Console.WriteLine("==================================");
        Console.WriteLine("         INFORMATIEPAGINA         ");
        Console.WriteLine("==================================");
        Console.WriteLine("Restaurantnaam : Restaurant B");
        Console.WriteLine("Telefoonnummer : 0612345678");
        Console.WriteLine("Adres          : Witte de Withstraat 12, Rotterdam");
        Console.WriteLine("Openingstijden : Dinsdag t/m zaterdag");
        Console.WriteLine("                  17:00 - 00:00");
        Console.WriteLine();
        Console.WriteLine("Welkom bij Restaurant B!");
        Console.WriteLine("Hier kunt u genieten van heerlijk eten");
        Console.WriteLine("en binnenkort eenvoudig reserveren.");
        Console.WriteLine();
        Console.WriteLine("Druk op een toets om terug te gaan...");
        Console.ReadKey(true);
    }
    static void ShowReservationPage()
    {
        DatabaseContext db = new DatabaseContext();

        ReserveringAccess reserveringAccess = new ReserveringAccess(db);
        TafelAccess tafelAccess = new TafelAccess(db);
        TijdslotAccess tijdslotAccess = new TijdslotAccess(db);
        ReservationLogic reservationLogic = new ReservationLogic(reserveringAccess, tafelAccess, tijdslotAccess);

        int gebruikerID = 1;

        ReserveringUI reserveringUI = new ReserveringUI(reservationLogic, gebruikerID);
        reserveringUI.ShowReserveringPage();

        db.Close();
    }

    public static void Show()
    {
        bool running = true;

        while (running)
        {
            Console.Clear();
            Console.WriteLine("==================================");
            Console.WriteLine("     WELKOM BIJ RESTAURANT B      ");
            Console.WriteLine("==================================");
            Console.WriteLine("1. Bekijk informatiepagina");
            Console.WriteLine("2. Bekijk menukaart");
            Console.WriteLine("3. Reserveer een tafel");
            Console.WriteLine("0. Afsluiten");
            Console.WriteLine();
            Console.Write("Maak een keuze: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ShowInformationPage();
                    break;
                case "2":
                    DatabaseContext db = new DatabaseContext();
                    ShowMenuUI menuUI = new ShowMenuUI(db);
                    menuUI.ShowMenuPage();
                    db.Close();
                    break;
                case "3":
                    ShowReservationPage();
                    break;
                case "0":
                    running = false;
                    break;
                default:
                    Console.WriteLine("Ongeldige keuze. Druk op een toets om verder te gaan...");
                    Console.ReadKey(true);
                    break;
            }
        }
    }
}

