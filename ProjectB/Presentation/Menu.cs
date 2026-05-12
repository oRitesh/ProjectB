

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
            AdminAccess adminAccess = new AdminAccess(new DatabaseContext());
            adminAccess.CheckAdminAccountExistence();
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

                        if (HuidigeGebruiker.ID != 0)
                        {
                            Console.WriteLine("U bent al ingelogd als: " + HuidigeGebruiker.Naam);
                            Console.WriteLine("Druk op een toets om verder te gaan...");
                            Console.ReadKey(true);
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

                        if (loginChoice == null)
                        {
                            break;
                        }

                        if (loginChoice == LoginOption.Inloggen)
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
                                adminLogic.ShowAdminMenuIfAuthorized(user.ID);
                            }
                            else
                            {
                                Console.WriteLine("Onjuiste gegevens.");
                            }
                            Console.ReadKey();
                        }
                        else if (loginChoice == LoginOption.Registreren)
                        {
                            string? regName = null;
                            string? regEmail = null;
                            string? regPhone = null;
                            string? regPassword = null;

                            bool userExists = false;

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
                                    Console.WriteLine();
                                    Console.WriteLine("E-mailadres mag niet leeg zijn. Probeer het opnieuw.");
                                    Console.ReadKey();
                                }

                                else if (!regEmail.Contains("@") || !regEmail.Contains("."))
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Ongeldig e-mailadres. Probeer het opnieuw.");
                                    regEmail = null;
                                    Console.ReadKey();
                                }

                                var checkUser = userAccess.GetUserByEmail(regEmail);
                                if (checkUser != null)
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("E-mailadres is al in gebruik. Probeer het opnieuw.");
                                    regEmail = null;
                                    Console.ReadKey();
                                }
                            }

                            while (string.IsNullOrWhiteSpace(regPhone))
                            {
                                Console.Write("Voer uw telefoonnummer in: ");
                                regPhone = Console.ReadLine();
                                if (string.IsNullOrWhiteSpace(regPhone))
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Telefoonnummer mag niet leeg zijn. Probeer het opnieuw.");
                                    Console.ReadKey();
                                }

                                var checkUser = userAccess.GetUserByPhoneNumber(regPhone);
                                if (checkUser != null && checkUser.Rol == 1)
                                {

                                    Console.WriteLine();
                                    Console.WriteLine("Dit telefoonnummer is al gekoppeld aan een account.");
                                    regPhone = null;
                                    Console.ReadKey();
                                }

                                else if (checkUser != null && checkUser.Rol == 0)
                                {
                                    userExists = true;
                                }
                            }

                            while (string.IsNullOrWhiteSpace(regPassword))
                            {
                                Console.Write("Voer uw wachtwoord in: ");
                                regPassword = Console.ReadLine();
                                if (string.IsNullOrWhiteSpace(regPassword))
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Wachtwoord mag niet leeg zijn. Probeer het opnieuw.");
                                    Console.ReadKey();
                                }

                                else if (regPassword.Length < 6)
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Wachtwoord moet minimaal 6 tekens bevatten. Probeer het opnieuw.");
                                    Console.ReadKey();
                                    regPassword = null; // Reset zodat de loop doorgaat
                                }

                            }

                            Console.Clear();
                            Console.WriteLine("Registratie succesvol!");
                            Console.WriteLine($"Welkom {regName}!");
                            Console.WriteLine("\nDruk op een toets om verder te gaan...");
                            Console.ReadKey();

                            if (userExists)
                            {
                                Console.WriteLine("We hebben uw gegevens gevonden van een eerdere reservering.");
                                Console.WriteLine("Uw gast-account wordt omgezet naar een officieel account...");
                                HuidigeGebruiker = userAccess.ChangeRole(regPhone, regName, 1, regEmail, regPassword);
                                Console.WriteLine("Account succesvol bijgewerkt!");
                            }
                            else
                            {
                                HuidigeGebruiker = new Gebruiker(1, 1, regName, regEmail, regPhone, regPassword);
                                userAccess.AddUser(HuidigeGebruiker);
                            }
                        }

                        break;
                    }

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
                // case "5" when HuidigeGebruiker.Rol == 2:
                //     AdminMenuUI adminMenuUI = new AdminMenuUI();
                //     adminMenuUI.ShowAdminMenu();
                //     break;

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

