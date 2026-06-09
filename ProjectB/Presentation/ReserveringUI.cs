using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class ReserveringUI
{
    private readonly ReservationLogic ReservationLogic;
    private readonly Gebruiker huidigeGebruiker;
    private string gastNaam = "";
    private string gastTelefoon = "";

    public ReserveringUI(ReservationLogic ReservationLogic, Gebruiker gebruiker)
    {
        this.ReservationLogic = ReservationLogic;
        this.huidigeGebruiker = gebruiker;
    }

    public void ShowReserveringPage()
    {
        int stap = 1;

        int aantalPersonen = 1;
        DateTime gekozenDatum = DateTime.Today;
        Tijdslot? gekozenTijdslot = null;
        int gekozenTafelNummer = 0;
        string opmerking = "";

        bool bezig = true;

        while (bezig)
        {
            switch (stap)
            {
                case 1:
                    int? personenKeuze = KiesAantalPersonen();
                    if (personenKeuze == null || personenKeuze < 1)
                    {
                        bezig = false;
                    }
                    else
                    {
                        aantalPersonen = personenKeuze.Value;
                        stap = 2;
                    }
                    break;

                case 2:
                    DateTime? datumKeuze = KiesDatum();
                    if (datumKeuze == null)
                    {
                        stap = 1;
                    }
                    else
                    {
                        gekozenDatum = datumKeuze.Value;
                        stap = 3;
                    }
                    break;

                case 3:
                    Tijdslot? tijdslotKeuze = KiesTijdslot(aantalPersonen, gekozenDatum);
                    if (tijdslotKeuze == null)
                    {
                        stap = 2;
                    }
                    else
                    {
                        gekozenTijdslot = tijdslotKeuze;
                        stap = 4;
                    }
                    break;

                case 4:
                    int? tafelKeuze = KiesTafel(aantalPersonen, gekozenTijdslot!);
                    if (tafelKeuze == null)
                    {
                        stap = 3;
                    }
                    else
                    {
                        gekozenTafelNummer = tafelKeuze.Value;
                        stap = (huidigeGebruiker.Rol == 0) ? 44 : 5;
                    }
                    break;

                case 44: // keuze: inloggen / registreren / als gast
                    bool? loginResultaat = VraagLoginOfGast();
                    if (loginResultaat == null)
                    {
                        stap = 4;
                    }
                    else if (loginResultaat == true)
                    {
                        // succesvol ingelogd of geregistreerd
                        stap = 5;
                    }
                    else
                    {
                        // doorgaan als gast
                        stap = 45;
                    }
                    break;

                case 45:
                    if (VulGastGegevensIn())
                    {
                        stap = 5;
                    }
                    else
                    {
                        stap = 44;
                    }
                    break;

                case 5:
                    string? opmerkingKeuze = KiesOpmerking();
                    if (opmerkingKeuze == null)
                    {
                        stap = (huidigeGebruiker.Rol == 0 && huidigeGebruiker.ID == 0) ? 45 : 4;
                    }
                    else
                    {
                        opmerking = opmerkingKeuze;
                        stap = 6;
                    }
                    break;

                case 6:
                    string displayNaam = (huidigeGebruiker.Rol == 1) ? huidigeGebruiker.Naam : gastNaam;
                    string displayTel = (huidigeGebruiker.Rol == 1) ? huidigeGebruiker.Telefoonnummer : gastTelefoon;

                    bool? bevestiging = BevestigReservering(
                        aantalPersonen,
                        gekozenDatum,
                        gekozenTijdslot!,
                        gekozenTafelNummer,
                        opmerking,
                        displayNaam,
                        displayTel
                    );

                    if (bevestiging == null)
                    {
                        stap = 5;
                    }
                    else if (bevestiging == true)
                    {
                        int definitiefID = huidigeGebruiker.ID;

                        if (huidigeGebruiker.Rol == 0)
                        {
                            definitiefID = ReservationLogic.VoegGastToe(displayNaam, displayTel);
                        }

                        bool gelukt = ReservationLogic.AddReservering(
                            definitiefID,
                            aantalPersonen,
                            gekozenTijdslot!,
                            gekozenTafelNummer,
                            opmerking
                        );

                        Console.Clear();
                        Console.WriteLine("==================================");
                        Console.WriteLine("           RESERVERING            ");
                        Console.WriteLine("==================================");
                        Console.WriteLine();

                        if (gelukt)
                        {
                            Console.WriteLine("Je reservering is succesvol opgeslagen.");
                            Console.WriteLine($"Naam: {displayNaam}");
                            Console.WriteLine($"Telefoon: {displayTel}");
                        }
                        else
                        {
                            Console.WriteLine("De gekozen tafel is niet meer beschikbaar of is ongeldig.");
                        }

                        Console.WriteLine();
                        Console.WriteLine("Druk op een toets om terug te gaan...");
                        Console.ReadKey(true);
                        bezig = false;
                    }
                    else
                    {
                        stap = 5;
                    }
                    break;
            }
        }
    }

    private bool? VraagLoginOfGast()
    {
        while (true)
        {
            var opties = new List<string> { "Inloggen", "Registreren", "Doorgaan als gast" };

            string? keuze = ArrowMenu.ShowMenu(
                "INLOGGEN OF ALS GAST DOORGAAN?",
                opties,
                x => x
            );

            if (keuze == null) return null;

            DatabaseContext db = new DatabaseContext();
            UserAccess userAccess = new UserAccess(db);

            if (keuze == "Inloggen")
            {
                var user = new InlogUI(userAccess).Login();
                if (user != null)
                {
                    huidigeGebruiker.ID = user.ID;
                    huidigeGebruiker.Naam = user.Naam;
                    huidigeGebruiker.Telefoonnummer = user.Telefoonnummer;
                    huidigeGebruiker.Rol = user.Rol;
                    db.Close();
                    return true;
                }
            }
            else if (keuze == "Registreren")
            {
                var user = new RegistratieUI(userAccess).Registreer();
                if (user != null)
                {
                    huidigeGebruiker.ID = user.ID;
                    huidigeGebruiker.Naam = user.Naam;
                    huidigeGebruiker.Telefoonnummer = user.Telefoonnummer;
                    huidigeGebruiker.Rol = user.Rol;
                    db.Close();
                    return true;
                }
            }
            else if (keuze == "Doorgaan als gast")
            {
                db.Close();
                return false;
            }

            db.Close();
        }
    }

    private bool VulGastGegevensIn()
    {
        Console.Clear();
        Console.WriteLine("=== Gastgegevens ===");
        Console.WriteLine("Druk op Escape om terug te gaan.");
        Console.WriteLine();

        // Naam validatie
        string? naam = LeesInvoer.LeesInvoerMetEscape("Voer uw naam in: ");
        if (naam == null) return false;

        while (string.IsNullOrEmpty(naam))
        {
            Console.WriteLine("Naam mag niet leeg zijn. Probeer opnieuw.");
            naam = LeesInvoer.LeesInvoerMetEscape("Voer uw naam in: ");
            if (naam == null) return false;
        }

        // Telefoonnummer validatie
        string? telefoon = LeesInvoer.LeesInvoerMetEscape("Voer uw telefoonnummer in (10 cijfers): ");
        if (telefoon == null) return false;

        while (string.IsNullOrEmpty(telefoon) || !telefoon.All(char.IsDigit) || telefoon.Length != 10)
        {
            Console.WriteLine("Ongeldig telefoonnummer. Voer precies 10 cijfers in.");
            telefoon = LeesInvoer.LeesInvoerMetEscape("Voer uw telefoonnummer in (10 cijfers): ");
            if (telefoon == null) return false;
        }

        gastNaam = naam;
        gastTelefoon = telefoon;
        return true;
    }
    public int? KiesAantalPersonen()
    {
        return ArrowMenu.ShowMenu(
       "KIES AANTAL PERSONEN",
       ReservationLogic.GetAantalPersonenOpties(),
       x => $"{x} personen");
    }

    public DateTime? KiesDatum()
    {
        List<DateTime> datums = ReservationLogic.GetBeschikbareDatums();
        int geselecteerd = 0;
        Console.CursorVisible = false;

        try
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("==================================");
                Console.WriteLine("            KIES DATUM            ");
                Console.WriteLine("==================================");
                Console.WriteLine();
                Console.WriteLine("Gebruik ← en → om te bewegen.");
                Console.WriteLine("Druk op Enter om te bevestigen.");
                Console.WriteLine("Druk op Escape om terug te gaan.");
                Console.WriteLine();

                ToonDatums(datums, geselecteerd);

                ConsoleKeyInfo key = Console.ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.LeftArrow when geselecteerd > 0:
                        geselecteerd--;
                        break;
                    case ConsoleKey.RightArrow when geselecteerd < datums.Count - 1:
                        geselecteerd++;
                        break;
                    case ConsoleKey.Enter:
                        return datums[geselecteerd];
                    case ConsoleKey.Escape:
                        return null;
                    default:
                        break;
                        //ignore all other keys
                }
            }
        }
        finally
        {
            Console.CursorVisible = true;
        }
    }

    private void ToonDatums(List<DateTime> datums, int geselecteerd)
    {
        CultureInfo nl = new CultureInfo("nl-NL");

        var groepen = datums
            .Select((datum, index) => new { Datum = datum, Index = index })
            .GroupBy(x => new { x.Datum.Year, x.Datum.Month });

        foreach (var groep in groepen)
        {
            string maandNaam = new DateTime(groep.Key.Year, groep.Key.Month, 1).ToString("MMMM", nl);
            Console.WriteLine(maandNaam);
            Console.WriteLine();

            foreach (var item in groep)
            {
                if (item.Index == geselecteerd)
                {
                    Console.Write($"[{item.Datum.Day}] ");
                }
                else
                {
                    Console.Write($"{item.Datum.Day} ");
                }
            }

            Console.WriteLine();
            Console.WriteLine();
        }

        Console.WriteLine($"Geselecteerde datum: {datums[geselecteerd].ToString("dddd dd MMMM yyyy", nl)}");
    }

    public Tijdslot? KiesTijdslot(int aantalPersonen, DateTime datum)
    {
        Console.Clear();

        List<Tijdslot> tijdsloten =
            ReservationLogic.GetBeschikbareTijdsloten(aantalPersonen, datum);

        if (tijdsloten.Count == 0)
        {
            Console.WriteLine("Er zijn geen beschikbare tijdsloten op deze datum.");
            Console.WriteLine();
            Console.WriteLine("Druk op een toets om terug te gaan...");
            Console.ReadKey(true);
            return null;
        }

        return ArrowMenu.ShowMenu(
            "KIES TIJDSLOT",
            tijdsloten,
            x => $"{DateTime.Parse(x.StartTijd):HH:mm} - {DateTime.Parse(x.EindTijd):HH:mm}",
            () =>
            {
                Console.WriteLine($"Aantal personen: {aantalPersonen}");
                Console.WriteLine($"Datum: {datum:dd-MM-yyyy}");
                Console.WriteLine();
            },
            true
        );
    }

    private int? KiesTafel(int aantalPersonen, Tijdslot tijdslot)
    {
        while (true)
        {
            Console.Clear();
            ToonPlattegrond(aantalPersonen, tijdslot);

            Console.WriteLine();
            Console.WriteLine("Typ het tafelnummer en druk op Enter.");
            Console.WriteLine("Druk op Escape om terug te gaan.");

            string? input = LeesInvoer.LeesInvoerMetEscape("> ");

            if (input == null)
            {
                return null;
            }

            if (!int.TryParse(input, out int tafelNummer))
            {
                Console.WriteLine();
                Console.WriteLine("Ongeldige invoer. Voer een geldig tafelnummer in.");
                Console.WriteLine("Druk op een toets om opnieuw te proberen...");
                Console.ReadKey(true);
                continue;
            }

            if (!ReservationLogic.IsTafelBeschikbaarVoorKeuze(tafelNummer, aantalPersonen, tijdslot))
            {
                Console.WriteLine();
                Console.WriteLine("Deze tafel is niet beschikbaar of niet toegestaan voor jouw groepsgrootte.");
                Console.WriteLine("Druk op een toets om opnieuw te proberen...");
                Console.ReadKey(true);
                continue;
            }

            return tafelNummer;
        }
    }

    private readonly string[] plattegrondMap =
    {
        "┌──────────────────────────────────────────────────┐",
        "│                                                  │",
        "│○ ■1■ ○                   ○ ■4■ ○         ○       │",
        "│             ○ ■7■ ○                   ○ ■12■ ○   │",
        "\\             ○ ■■■ ○      ○ ■5■ ○      ○ ■■■■ ○   │",
        " \\                                         ○       │",
        "  \\                        ○ ■6■ ○                 │",
        "│                                                  │",
        "│             ○ ■8■ ○         ○  ○        ○  ○     │",
        "│ ■2■ ○       ○ ■■■ ○         ■9■■        ■10■     │",
        "│ ○                           ○  ○        ○  ○     │",
        "│                ○  ○                              │",
        "│ ■3■ ○        ○ ■11■ ○      ┌──────────┐          │",
        "│  ○             ○  ○        │  KEUKEN  │          │",
        "│                            └──────────┘          │",
        "└──────────────────────────────────────────────────┘"
    };


    private void ToonPlattegrond(int aantalPersonen, Tijdslot tijdslot)
    {
        List<TafelWeergave> tafels =
            ReservationLogic.GetTafelWeergaveVoorTijdslot(aantalPersonen, tijdslot);

        int benodigdeCapaciteit = ReservationLogic.GetBenodigdeCapaciteit(aantalPersonen);

        Console.WriteLine("==================================");
        Console.WriteLine("         KIES EEN TAFEL           ");
        Console.WriteLine("==================================");
        Console.WriteLine();
        Console.WriteLine("Legenda:");
        Console.WriteLine("[2] = beschikbaar");
        Console.WriteLine("(2) = gereserveerd");
        Console.WriteLine("-2- = verkeerde capaciteit");
        Console.WriteLine();
        Console.WriteLine($"Jouw gezelschap: {aantalPersonen} personen");
        Console.WriteLine($"Toegestane tafels: capaciteit {benodigdeCapaciteit}");
        Console.WriteLine();
        Console.WriteLine("INGANG");
        Console.WriteLine();

        ToonAsciiPlattegrond(tafels);

        Console.WriteLine();

        List<int> beschikbareTafels =
            ReservationLogic.GetBeschikbareTafelNummers(aantalPersonen, tijdslot);

        if (beschikbareTafels.Count == 0)
        {
            Console.WriteLine("Beschikbare tafelnummers: geen");
        }
        else
        {
            Console.WriteLine($"Beschikbare tafelnummers: {string.Join(", ", beschikbareTafels)}");
        }
    }

    private void ToonAsciiPlattegrond(List<TafelWeergave> tafels)
    {
        Dictionary<int, TafelWeergave> tafelPerNummer = tafels
            .ToDictionary(t => t.TafelNummer);

        foreach (string origineleRegel in plattegrondMap)
        {
            string regel = origineleRegel;

            foreach (TafelWeergave tafel in tafels)
            {
                string vak = MaakTafelVak(tafel);

                string token = tafel.TafelNummer switch
                {
                    9 => "■9■■",
                    _ => $"■{tafel.TafelNummer}■"
                };

                regel = regel.Replace(token, vak.PadRight(token.Length));
            }

            Console.WriteLine(regel);
        }
    }

    private string MaakTafelVak(TafelWeergave tafel)
    {
        if (!tafel.IsToegestaan)
        {
            return $"-{tafel.TafelNummer}-";
        }

        if (!tafel.IsBeschikbaar)
        {
            return $"({tafel.TafelNummer})";
        }

        return $"[{tafel.TafelNummer}]";
    }

    private string? KiesOpmerking()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("==================================");
            Console.WriteLine("            OPMERKING             ");
            Console.WriteLine("==================================");
            Console.WriteLine();
            Console.WriteLine("Typ een opmerking en druk op Enter.");
            Console.WriteLine("Laat leeg als je geen opmerking hebt.");
            Console.WriteLine("Typ /back om terug te gaan.");
            Console.WriteLine();

            string? input = Console.ReadLine();

            if (input == "/back")
            {
                return null;
            }

            return input ?? "";
        }
    }

    private bool? BevestigReservering(
        int aantalPersonen,
        DateTime datum,
        Tijdslot tijdslot,
        int tafelNummer,
        string opmerking,
        string naam,
        string tel)
    {
        List<string> opties = new List<string> { "Bevestigen", "Terug" };
        string? keuze = ArrowMenu.ShowMenu(
           "BEVESTIG RESERVERING",
           opties,
           x => x,
           () =>
           {
               Console.WriteLine($"Naam: {naam}");
               Console.WriteLine($"Telefoon: {tel}");
               Console.WriteLine();
               Console.WriteLine($"Aantal personen: {aantalPersonen}");
               Console.WriteLine($"Datum: {datum:dd-MM-yyyy}");
               Console.WriteLine($"Tijdslot: {DateTime.Parse(tijdslot.StartTijd):HH:mm} - {DateTime.Parse(tijdslot.EindTijd):HH:mm}");
               Console.WriteLine($"Tafelnummer: {tafelNummer}");
               Console.WriteLine($"Opmerking: {opmerking}");
               Console.WriteLine();
           }
       );

        if (keuze == null) return null;
        return keuze == "Bevestigen";
    }


    // string[] map =
    // {
    //     "┌──────────────────────────────────────────────────┐",
    //     "│                                                  │",
    //     "│○ ■1■ ○                   ○ ■4■ ○         ○       │",
    //     "│             ○ ■7■ ○                   ○ ■12■ ○   │",
    //     "\             ○ ■■■ ○      ○ ■5■ ○      ○ ■■■■ ○   │",
    //     " \                                         ○       │",
    //     "  \                        ○ ■6■ ○                 │",
    //     "│                                                  │",
    //     "│             ○ ■8■ ○         ○  ○        ○  ○     │",
    //     "│ ■2■ ○       ○ ■■■ ○         ■9■■        ■10■     │",
    //     "│ ○                           ○  ○        ○  ○     │",
    //     "│                ○  ○                              │",
    //     "│ ■3■ ○        ○ ■11■ ○      ┌──────────┐          │",
    //     "│  ○             ○  ○        │  KEUKEN  │          │",
    //     "│                            └──────────┘          │",
    //     "└──────────────────────────────────────────────────┘"
    // };

}
