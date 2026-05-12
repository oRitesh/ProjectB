public static class Menu
{
    private static Gebruiker? HuidigeGebruiker = new Gebruiker(0, 0, "gast", "", "", "");

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
        UserAccess userAccess = new UserAccess(db);
        ReservationLogic reservationLogic = new ReservationLogic(reserveringAccess, tafelAccess, tijdslotAccess, userAccess);

        ReserveringUI reserveringUI = new ReserveringUI(reservationLogic, HuidigeGebruiker);
        reserveringUI.ShowReserveringPage();

        db.Close();
    }

    public static void Show()
    {
        bool running = true;

        while (running)
        {
            Console.Clear();
            if (HuidigeGebruiker.Naam != "gast")
            {
                Console.WriteLine("==================================");
                Console.WriteLine("     WELKOM BIJ RESTAURANT B      ");
                Console.WriteLine("      Ingelogd als: " + HuidigeGebruiker.Naam);
                Console.WriteLine("==================================");
                Console.WriteLine("1. Bekijk informatiepagina");
                Console.WriteLine("2. Bekijk menukaart");
                Console.WriteLine("3. Reserveer een tafel");
                Console.WriteLine("4. Overzicht reserveringen/ reservering wijzigen");
                Console.WriteLine("0. Afsluiten");
            }
            else
            {
                Console.WriteLine("==================================");
                Console.WriteLine("     WELKOM BIJ RESTAURANT B      ");
                Console.WriteLine("==================================");
                Console.WriteLine("1. Bekijk informatiepagina");
                Console.WriteLine("2. Bekijk menukaart");
                Console.WriteLine("3. Reserveer een tafel");
                Console.WriteLine("4. Login / registreer");
                Console.WriteLine("0. Afsluiten");
            }
            Console.WriteLine();
            Console.Write("Maak een keuze: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ShowInformationPage();
                    break;

                case "2":
                {
                    DatabaseContext db = new DatabaseContext();
                    ShowMenuUI menuUI = new ShowMenuUI(db);
                    menuUI.ShowMenuPage();
                    db.Close();
                    break;
                }

                case "3":
                    ShowReservationPage();
                    break;

                case "4" when HuidigeGebruiker.Naam == "gast":
                {
                    DatabaseContext db = new DatabaseContext();
                    UserAccess userAccess = new UserAccess(db);

                    InlogUI inlogUI = new InlogUI(userAccess);
                    RegistratieUI registratieUI = new RegistratieUI(userAccess);

                    if (HuidigeGebruiker.ID == 0)
                    {
                        Console.WriteLine("1. Inloggen");
                        Console.WriteLine("2. Registreren");
                        Console.WriteLine("0. Terug");
                        Console.Write("Maak een keuze: ");
                        string? loginChoice = Console.ReadLine();

                        if (loginChoice == "1")
                        {
                            var user = inlogUI.Login();
                            if (user != null) HuidigeGebruiker = user;
                        }
                        else if (loginChoice == "2")
                        {
                            var user = registratieUI.Registreer();
                            if (user != null) HuidigeGebruiker = user;
                        }
                    }
                    else
                    {
                        Console.WriteLine("U bent al ingelogd als: " + HuidigeGebruiker.Naam);
                        Console.WriteLine("Druk op een toets om verder te gaan...");
                        Console.ReadKey(true);
                    }

                    db.Close();
                    break;
                }

                case "4":
                {
                    if (HuidigeGebruiker.ID == 0)
                    {
                        Console.WriteLine("U moet eerst inloggen.");
                        Console.ReadKey();
                        break;
                    }

                    DatabaseContext db = new DatabaseContext();
                    ReserveringAccess reserveringAccess = new ReserveringAccess(db);
                    TafelAccess tafelAccess = new TafelAccess(db);
                    TijdslotAccess tijdslotAccess = new TijdslotAccess(db);
                    UserAccess userAccess = new UserAccess(db);

                    ReservationLogic logic = new ReservationLogic(reserveringAccess, tafelAccess, tijdslotAccess, userAccess);

                    ReserveringOverzichtUI overzichtUI = new ReserveringOverzichtUI(logic, HuidigeGebruiker);
                    overzichtUI.ShowOverzicht();

                    db.Close();
                    break;
                }

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
