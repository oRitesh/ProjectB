

using System.Collections;

public static class Menu
{
    private static Gebruiker? HuidigeGebruiker = new Gebruiker(0, 0, "gast", "", "", "");

    static void ShowInformationPage()
    {
        OpeningsTijdenLogic openingsTijdenLogic = new OpeningsTijdenLogic();
        OpeningsDagLogic openingsDagLogic = new OpeningsDagLogic();

        OpeningsTijden? tijden = openingsTijdenLogic.GetOpeningsTijden();
        List<OpeningsDag> dagen = openingsDagLogic.GetAllOpeningsDagen();

        string openingsDagenTekst = openingsDagLogic.GetOpeningsDagenTekst(dagen);
        string openingsTijdenTekst = tijden == null
            ? "Onbekend"
            : $"{tijden.OpeningsTijd} - {tijden.SluitingsTijd}";

        Console.Clear();
        Console.WriteLine(@"                                                                                                                                                      
▄▄▄▄▄ ▄▄▄    ▄▄▄  ▄▄▄▄▄▄▄   ▄▄▄▄▄   ▄▄▄▄▄▄▄   ▄▄▄      ▄▄▄   ▄▄▄▄   ▄▄▄▄▄▄▄▄▄ ▄▄▄▄▄  ▄▄▄▄▄▄▄   ▄▄▄▄▄▄▄     ▄▄▄▄    ▄▄▄▄▄▄▄  ▄▄▄▄▄ ▄▄▄    ▄▄▄   ▄▄▄▄   
 ███  ████▄  ███ ███▀▀▀▀▀ ▄███████▄ ███▀▀███▄ ████▄  ▄████ ▄██▀▀██▄ ▀▀▀███▀▀▀  ███  ███▀▀▀▀▀   ███▀▀███▄ ▄██▀▀██▄ ███▀▀▀▀▀   ███  ████▄  ███ ▄██▀▀██▄ 
 ███  ███▀██▄███ ███▄▄    ███   ███ ███▄▄███▀ ███▀████▀███ ███  ███    ███     ███  ███▄▄      ███▄▄███▀ ███  ███ ███        ███  ███▀██▄███ ███  ███ 
 ███  ███  ▀████ ███▀▀    ███▄▄▄███ ███▀▀██▄  ███  ▀▀  ███ ███▀▀███    ███     ███  ███        ███▀▀▀▀   ███▀▀███ ███  ███▀  ███  ███  ▀████ ███▀▀███ 
▄███▄ ███    ███ ███       ▀█████▀  ███  ▀███ ███      ███ ███  ███    ███    ▄███▄ ▀███████   ███       ███  ███ ▀██████▀  ▄███▄ ███    ███ ███  ███
");
        Console.WriteLine();
        Console.WriteLine("Restaurantnaam : Het Culinaire Bootje");
        Console.WriteLine("Telefoonnummer : 0612345678");
        Console.WriteLine("Adres          : Witte de Withstraat 12, Rotterdam");
        Console.WriteLine($"Openingsdagen  : {openingsDagenTekst}");
        Console.WriteLine($"Openingstijden : {openingsTijdenTekst}");
        Console.WriteLine();
        Console.WriteLine("Welkom bij Het Culinaire Bootje!");
        Console.WriteLine("Hier kunt u genieten van heerlijk eten");
        Console.WriteLine("en binnenkort eenvoudig reserveren.");
        Console.WriteLine();
        Console.WriteLine("Druk op een toets om terug te gaan...");
        Console.ReadKey(true);
    }

    static void ShowReservationPage()
    {
        ReservationLogic reservationLogic = new ReservationLogic();
        UserLogic userLogic = new UserLogic();

        ReserveringUI reserveringUI = new ReserveringUI(reservationLogic, HuidigeGebruiker, userLogic);
        reserveringUI.ShowReserveringPage();
    }

    public static void Show()
    {
        bool running = true;

        while (running)
        {
            AdminLogic adminLogicCheck = new AdminLogic();
            string? tempPassword = adminLogicCheck.EnsureAdminExists();

            if (tempPassword != null)
            {
                Console.Clear();
                Console.WriteLine("=== EERSTE OPSTART ===");
                Console.WriteLine($"Admin aangemaakt. Wachtwoord: {tempPassword}");
                Console.WriteLine("Bewaar dit wachtwoord veilig!");
                Console.WriteLine("======================");
                Console.WriteLine("Druk op een toets om door te gaan...");
                Console.ReadKey(true);
            }
            Console.Clear();

            // ===== MENU OPTIES (gast vs user) =====
            List<MainMenuOption?> opties = new();

            opties.Add(MainMenuOption.Info);
            opties.Add(MainMenuOption.Menu);
            opties.Add(MainMenuOption.Reserveren);
            opties.Add(MainMenuOption.Afhalen);

            if (HuidigeGebruiker.Rol == 0)
            {
                opties.Add(MainMenuOption.Login);
            }
            else if (HuidigeGebruiker.Rol == 2 || HuidigeGebruiker.Rol == 3)
            {
                opties.Add(MainMenuOption.Admin);
                opties.Add(MainMenuOption.Loguit);
            }
            else
            {
                opties.Add(MainMenuOption.Overzicht);
                opties.Add(MainMenuOption.Loguit);
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
                    MainMenuOption.Afhalen => "Plaats afhaalbestelling",
                    MainMenuOption.Overzicht => "Overzicht reserveringen",
                    MainMenuOption.Login => "Login / registreer",
                    MainMenuOption.Loguit => "Loguit",
                    MainMenuOption.Admin => "Open Admin menu",
                    MainMenuOption.Exit => "Afsluiten",
                    _ => ""
                },
                () =>
                {
                    Console.WriteLine(@"
                                                                                                                                     ⠀⣀⠀⡀⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⠀⠀⠀
                                                                                                                                     ⠀⣿⠀⡇⣿⠀⠀⠀⠀⠀⠀⠀⢀⣀⣀⣀⣀⡀⠀⠀⠀⠀⠀⠀⠀⠀⢸⣷⡀⠀
                                                                                                                                     ⠀⣿⣤⣧⣿⠀⠀⠀⠀⣠⣴⣾⣿⣿⣿⣿⣿⣿⣷⣦⣄⠀⠀⠀⠀⠀⢸⣿⡇⠀
                                                                                                                                     ⠀⣿⣿⣿⣿⠀⠀⣠⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⣄⠀⠀⠀⢸⣿⣿⠀
                                                                                                                                     ⠀⠈⢻⡟⠁⠀⣰⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣆⠀⠀⢸⣿⡇⠀
 _______  __   __  ___      ___   __    _  _______  ___   ______    _______    _______  _______  _______  _______      ___  _______  ⠀⠀⢸⡇⠀⢰⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡆⠀⢸⡿⠁⠀
|       ||  | |  ||   |    |   | |  |  | ||   _   ||   | |    _ |  |       |  |  _    ||       ||       ||       |    |   ||       | ⠀⠀⢸⣿⠀⢸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡇⠀⢸⡆⠀⠀
|       ||  | |  ||   |    |   | |   |_| ||  |_|  ||   | |   | ||  |    ___|  | |_|   ||   _   ||   _   ||_     _|    |   ||    ___| ⠀⠀⢸⣿⠀⠸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠇⠀⢸⡇⠀⠀
|       ||  |_|  ||   |    |   | |       ||       ||   | |   |_||_ |   |___   |       ||  | |  ||  | |  |  |   |      |   ||   |___  ⠀⠀⣿⣿⠀⠀⠹⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠏⠀⠀⢸⡇⠀⠀
|      _||       ||   |___ |   | |  _    ||       ||   | |    __  ||    ___|  |  _   | |  |_|  ||  |_|  |  |   |   ___|   ||    ___| ⠀⠀⣿⣿⠀⠀⠀⠙⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠋⠀⠀⠀⢸⣿⠀⠀
|     |_ |       ||       ||   | | | |   ||   _   ||   | |   |  | ||   |___   | |_|   ||       ||       |  |   |  |       ||   |___  ⠀⠀⣿⣿⠀⠀⠀⠀⠀⠙⠻⢿⣿⣿⣿⣿⣿⣿⡿⠟⠋⠀⠀⠀⠀⠀⢸⣿⠀⠀
|_______||_______||_______||___| |_|  |__||__| |__||___| |___|  |_||_______|  |_______||_______||_______|  |___|  |_______||_______| ⠀⠀⣿⣿⠀⠀⠀⠀⠀⠀⠀⠀⠈⠉⠉⠉⠉⠁⠀⠀⠀⠀⠀⠀⠀⠀⢸⣿⡄⠀
                                                                                                                                    ⠀ ⠀⠉⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠉⠀⠀

");
                    Console.WriteLine("===================================");
                    Console.WriteLine("  WELKOM BIJ HET CULINAIRE BOOTJE     ");

                    if (HuidigeGebruiker.Naam != "gast")
                    {
                        Console.WriteLine($"     Ingelogd als: {HuidigeGebruiker.Naam}");
                    }

                    Console.WriteLine("===================================");
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
                        ShowMenuUI menuUI = new ShowMenuUI(new MenuService());
                        menuUI.ShowMenuPage();
                        break;
                    }

                case MainMenuOption.Reserveren:
                    ShowReservationPage();
                    break;

                case MainMenuOption.Afhalen:
                    {
                        AfhaalSysteemUI afhaalUI = new AfhaalSysteemUI(HuidigeGebruiker);
                        afhaalUI.Start();
                        break;
                    }

                case MainMenuOption.Login:
                    {
                        UserLogic userLogic = new UserLogic();
                        InlogUI inlogUI = new InlogUI(userLogic);
                        RegistratieUI registratieUI = new RegistratieUI(userLogic);

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
                                Console.WriteLine("==========================================================================================");
                                Console.WriteLine(@"                                                 
▄▄▄        ▄▄▄▄▄    ▄▄▄▄▄▄▄  ▄▄▄▄▄ ▄▄▄    ▄▄▄   ▄▄▄      ▄▄▄  ▄▄▄▄▄▄▄ ▄▄▄    ▄▄▄ ▄▄▄  ▄▄▄ 
███      ▄███████▄ ███▀▀▀▀▀   ███  ████▄  ███   ████▄  ▄████ ███▀▀▀▀▀ ████▄  ███ ███  ███ 
███      ███   ███ ███        ███  ███▀██▄███   ███▀████▀███ ███▄▄    ███▀██▄███ ███  ███ 
███      ███▄▄▄███ ███  ███▀  ███  ███  ▀████   ███  ▀▀  ███ ███      ███  ▀████ ███▄▄███ 
████████  ▀█████▀  ▀██████▀  ▄███▄ ███    ███   ███      ███ ▀███████ ███    ███ ▀██████▀ 
                                                                                          ");
                                Console.WriteLine("==========================================================================================");
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

                        break;
                    }

                case MainMenuOption.Loguit:
                    string? bevestig = ArrowMenu.ShowMenu(
                        "WEET U ZEKER DAT U WILT UITLOGGEN?",
                        new List<string> { "Ja, uitloggen", "Nee, annuleer" },
                        x => x
                    );
                    if (bevestig == "Ja, uitloggen")
                        HuidigeGebruiker = new Gebruiker(0, 0, "gast", "", "", "");
                    break;

                case MainMenuOption.Admin:
                    AdminMenuUI adminMenuUI = new AdminMenuUI();
                    adminMenuUI.ShowAdminMenu(HuidigeGebruiker.Rol);
                    break;

                case MainMenuOption.Overzicht:
                    {
                        if (HuidigeGebruiker.ID == 0)
                        {
                            Console.WriteLine("U moet eerst inloggen.");
                            Console.ReadKey();
                            break;
                        }

                        ReservationLogic logic = new ReservationLogic();
                        UserLogic userLogic = new UserLogic();

                        ReserveringOverzichtUI overzichtUI = new ReserveringOverzichtUI(logic, HuidigeGebruiker, userLogic);
                        overzichtUI.ShowOverzicht();

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
