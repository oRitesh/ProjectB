

using System.Collections;

public static class Menu
{
    private static Gebruiker? HuidigeGebruiker = new Gebruiker(0, 0, "gast", "", "", "");

    static void ShowInformationPage()
    {
        DatabaseContext db = new DatabaseContext();

        OpeningsTijdenAccess openingsTijdenAccess = new OpeningsTijdenAccess(db);
        OpeningsDagAccess openingsDagAccess = new OpeningsDagAccess(db);

        OpeningsTijden? tijden = openingsTijdenAccess.GetOpeningsTijden();
        List<OpeningsDag> dagen = openingsDagAccess.GetAllOpeningsDagen();

        db.Close();

        string openingsDagenTekst = MaakOpeningsDagenTekst(dagen);
        string openingsTijdenTekst = tijden == null
            ? "Onbekend"
            : $"{tijden.OpeningsTijd} - {tijden.SluitingsTijd}";

        Console.Clear();
        Console.WriteLine("==================================");
        Console.WriteLine("         INFORMATIEPAGINA         ");
        Console.WriteLine("==================================");
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

    static string MaakOpeningsDagenTekst(List<OpeningsDag> dagen)
    {
        List<int> openDagen = dagen
            .Where(d => d.IsOpen == 1)
            .Select(d => d.DagVanWeek)
            .OrderBy(d => d)
            .ToList();

        if (openDagen.Count == 0)
        {
            return "Gesloten";
        }

        if (openDagen.Count == 7)
        {
            return "Elke dag";
        }

        return string.Join(", ", openDagen.Select(DagNaam));
    }

    static string DagNaam(int dagVanWeek)
    {
        return dagVanWeek switch
        {
            0 => "Zondag",
            1 => "Maandag",
            2 => "Dinsdag",
            3 => "Woensdag",
            4 => "Donderdag",
            5 => "Vrijdag",
            6 => "Zaterdag",
            _ => "Onbekend"
        };
    }

    static void ShowReservationPage()
    {
        DatabaseContext db = new DatabaseContext();

        ReserveringAccess reserveringAccess = new ReserveringAccess(db);
        TafelAccess tafelAccess = new TafelAccess(db);
        UserAccess userAccess = new UserAccess(db);
        ReservationLogic reservationLogic = new ReservationLogic(reserveringAccess, tafelAccess, userAccess);

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
                        DatabaseContext db = new DatabaseContext();
                        ShowMenuUI menuUI = new ShowMenuUI(db);
                        menuUI.ShowMenuPage();
                        db.Close();
                        break;
                    }

                case MainMenuOption.Reserveren:
                    ShowReservationPage();
                    break;

                case MainMenuOption.Afhalen:
                    {
                        DatabaseContext db = new DatabaseContext();
                        AfhaalSysteemUI afhaalUI = new AfhaalSysteemUI(db, HuidigeGebruiker);
                        afhaalUI.Start();
                        db.Close();
                        break;
                    }

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

                        DatabaseContext db = new DatabaseContext();
                        ReserveringAccess reserveringAccess = new ReserveringAccess(db);
                        TafelAccess tafelAccess = new TafelAccess(db);
                        UserAccess userAccess = new UserAccess(db);

                        ReservationLogic logic = new ReservationLogic(reserveringAccess, tafelAccess, userAccess);

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
