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
            Console.WriteLine("==================================");
            Console.WriteLine("     WELKOM BIJ RESTAURANT B      ");
            Console.WriteLine("      Ingelogd als: " + HuidigeGebruiker.Naam);
            Console.WriteLine("==================================");
            Console.WriteLine("1. Bekijk informatiepagina");
            Console.WriteLine("2. Bekijk menukaart");
            Console.WriteLine("3. Reserveer een tafel");
            Console.WriteLine("4. Login / registreer");
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
                case "4":
                    {
                        DatabaseContext db = new DatabaseContext();
                        UserAccess userAccess = new UserAccess(db);
                        if (HuidigeGebruiker.ID == 0)
                        {
                            Console.WriteLine("1. Inloggen");
                            Console.WriteLine("2. Registreren");
                            Console.WriteLine("0. Terug");
                            Console.Write("Maak een keuze: ");
                            string? loginChoice = Console.ReadLine();

                            if (loginChoice == "1")
                            {
                                Console.Clear();
                                Console.Write("E-mailadres: ");
                                string? email = Console.ReadLine();
                                Console.Write("Wachtwoord: ");
                                string? pass = Console.ReadLine();

                                var user = userAccess.GetUserByEmail(email, pass);
                                if (user != null)
                                {
                                    HuidigeGebruiker = user;
                                    Console.WriteLine($"Succesvol ingelogd! Welkom terug, {user.Naam}.");
                                }
                                else
                                {
                                    Console.WriteLine("Onjuiste gegevens.");
                                }
                                Console.ReadKey();
                            }
                            else if (loginChoice == "2")
                            {
                                string? regName = null;
                                string? regEmail = null;
                                string? regPhone = null;
                                string? regPassword = null;

                                while (string.IsNullOrWhiteSpace(regName))
                                {
                                    Console.Write("Voer uw naam in: ");
                                    regName = Console.ReadLine();

                                    if (string.IsNullOrWhiteSpace(regName))
                                    {
                                        Console.WriteLine("Naam mag niet leeg zijn. Probeer het opnieuw.");
                                        Console.ReadKey();
                                    }
                                }

                                while (string.IsNullOrWhiteSpace(regEmail))
                                {
                                    Console.Write("Voer uw e-mailadres in: ");
                                    regEmail = Console.ReadLine();
                                    if (string.IsNullOrWhiteSpace(regEmail))
                                    {
                                        Console.WriteLine("E-mailadres mag niet leeg zijn. Probeer het opnieuw.");
                                        Console.ReadKey();
                                    }

                                    else if (!regEmail.Contains("@") || !regEmail.Contains("."))
                                    {
                                        Console.WriteLine("Ongeldig e-mailadres. Probeer het opnieuw.");
                                        regEmail = null; // Reset zodat de loop doorgaat
                                    }

                                    var checkUser = userAccess.GetUserByEmail(regEmail);
                                    if (checkUser != null)
                                    {
                                        Console.WriteLine("E-mailadres is al in gebruik. Probeer het opnieuw.");
                                        regEmail = null; // Reset zodat de loop doorgaat
                                    }
                                }

                                while (string.IsNullOrWhiteSpace(regPhone))
                                {
                                    Console.Write("Voer uw telefoonnummer in: ");
                                    regPhone = Console.ReadLine();
                                    if (string.IsNullOrWhiteSpace(regPhone))
                                    {
                                        Console.WriteLine("Telefoonnummer mag niet leeg zijn. Probeer het opnieuw.");
                                        Console.ReadKey();
                                    }
                                }

                                while (string.IsNullOrWhiteSpace(regPassword))
                                {
                                    Console.Write("Voer uw wachtwoord in: ");
                                    regPassword = Console.ReadLine();
                                    if (string.IsNullOrWhiteSpace(regPassword))
                                    {
                                        Console.WriteLine("Wachtwoord mag niet leeg zijn. Probeer het opnieuw.");
                                        Console.ReadKey();
                                    }

                                    else if (regPassword.Length < 6)
                                    {
                                        Console.WriteLine("Wachtwoord moet minimaal 6 tekens bevatten. Probeer het opnieuw.");
                                        regPassword = null; // Reset zodat de loop doorgaat
                                    }

                                }

                                Console.Clear();
                                Console.WriteLine("Registratie succesvol!");
                                Console.WriteLine($"Welkom {regName}!");
                                Console.WriteLine("\nDruk op een toets om verder te gaan...");
                                Console.ReadKey();


                                HuidigeGebruiker = new Gebruiker(1, 1, regName, regEmail, regPhone, regPassword);
                                userAccess.AddUser(HuidigeGebruiker);
                            }
                            else if (loginChoice == "0")
                            {
                                // Do nothing, just return to the main menu
                            }
                            else
                            {
                                Console.WriteLine("Ongeldige keuze. Druk op een toets om verder te gaan...");
                                Console.ReadKey(true);
                            }

                        }
                        else
                        {
                            Console.WriteLine("U bent al ingelogd als: " + HuidigeGebruiker.Naam);
                            Console.WriteLine("Druk op een toets om verder te gaan...");
                            Console.ReadKey(true);
                        }
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

