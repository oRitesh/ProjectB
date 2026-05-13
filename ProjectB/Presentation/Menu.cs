

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
            DatabaseContext adminDb = new DatabaseContext();
            AdminAccess adminAccess = new AdminAccess(adminDb);
            adminAccess.CheckAdminAccountExistence();
            adminDb.Close();
            Console.Clear();

            // ===== MENU OPTIES (gast vs user) =====
            List<MainMenuOption?> opties = new();

            opties.Add(MainMenuOption.Info);
            opties.Add(MainMenuOption.Menu);
            opties.Add(MainMenuOption.Reserveren);

            if (HuidigeGebruiker.Naam == "gast")
            {
                opties.Add(MainMenuOption.Login);
            }
            else if (HuidigeGebruiker.Naam == "Admin")
            {
                opties.Add(MainMenuOption.Admin);
            }
            else
            {
                opties.Add(MainMenuOption.Overzicht);
            }

            opties.Add(MainMenuOption.Exit);

            // ===== ARROWMENU (vervangt Console.ReadLine + switch input) =====
            MainMenuOption? keuze = ArrowMenu.ShowMenu(
                "",
                opties,
                x => x switch
                {
                    MainMenuOption.Info => "Bekijk informatiepagina",
                    MainMenuOption.Menu => "Bekijk menukaart",
                    MainMenuOption.Reserveren => "Reserveer een tafel",
                    MainMenuOption.Overzicht => "Overzicht reserveringen",
                    MainMenuOption.Login => "Login / registreer",
                    MainMenuOption.Admin => "Open Admin menu",
                    MainMenuOption.Exit => "Afsluiten",
                    _ => ""
                },
                () =>
                {
                    Console.WriteLine("==================================");
                    Console.WriteLine("     WELKOM BIJ RESTAURANT B      ");

                    if (HuidigeGebruiker.Naam != "gast")
                    {
                        Console.WriteLine($"     Ingelogd als: {HuidigeGebruiker.Naam}");
                    }

                    Console.WriteLine("==================================");
                    Console.WriteLine();
                },
                false
            );

            // Escape in ArrowMenu
            if (keuze == null)
            {
                continue;
            }

            switch (keuze)
            {
                case MainMenuOption.Info:
                    ShowInformationPage();
                    break;

                case MainMenuOption.Menu:
                    {
                        DatabaseContext db = new DatabaseContext();
                        ShowMenuUI menuUI = new ShowMenuUI(db);
                        menuUI.ShowMenuPage();
                        db.Close();
                        break;
                    }

                case MainMenuOption.Reserveren:
                    ShowReservationPage();
                    break;

                case MainMenuOption.Login:
                    {
                        DatabaseContext db = new DatabaseContext();
                        UserAccess userAccess = new UserAccess(db);
                        AdminLogic adminLogic = new AdminLogic(db);

                        InlogUI inlogUI = new InlogUI(userAccess);
                        RegistratieUI registratieUI = new RegistratieUI(userAccess);

                        if (HuidigeGebruiker.ID != 0)
                        {
                            Console.WriteLine("U bent al ingelogd als: " + HuidigeGebruiker.Naam);
                            Console.WriteLine("Druk op een toets om verder te gaan...");
                            Console.ReadKey(true);
                            break;
                        }

                        List<LoginOption?> loginOpties = new()
                        {
                            LoginOption.Inloggen,
                            LoginOption.Registreren,
                        };

                        LoginOption? loginChoice = ArrowMenu.ShowMenu(
                            "",
                            loginOpties,
                            x => x switch
                            {
                                LoginOption.Inloggen => "Inloggen",
                                LoginOption.Registreren => "Registreren",
                                _ => ""
                            },
                            () =>
                            {
                                Console.WriteLine("==================================");
                                Console.WriteLine("            LOGIN MENU            ");
                                Console.WriteLine("==================================");
                                Console.WriteLine();
                            },
                            false
                        );

                        if (loginChoice == LoginOption.Inloggen)
                        {
                            var user = inlogUI.Login();
                            if (user != null) HuidigeGebruiker = user;
                        }
                        else if (loginChoice == LoginOption.Registreren)
                        {
                            var user = registratieUI.Registreer();
                            if (user != null) HuidigeGebruiker = user;
                        }

                        db.Close();
                        break;
                    }

                case MainMenuOption.Admin:
                    AdminMenuUI adminMenuUI = new AdminMenuUI();
                    adminMenuUI.ShowAdminMenu();
                    break;

                case MainMenuOption.Overzicht:
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

                case MainMenuOption.Exit:
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
