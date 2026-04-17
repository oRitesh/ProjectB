using System.Globalization;

public class ReserveringUI
{
    private readonly ReservationLogic ReservationLogic;
    private readonly int gebruikerID;

    public ReserveringUI(ReservationLogic ReservationLogic, int gebruikerID)
    {
        this.ReservationLogic = ReservationLogic;
        this.gebruikerID = gebruikerID;
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
                    if (personenKeuze == null)
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
                    int? tafelKeuze = KiesTafel(aantalPersonen, gekozenTijdslot);
                    if (tafelKeuze == null)
                    {
                        stap = 3;
                    }
                    else
                    {
                        gekozenTafelNummer = tafelKeuze.Value;
                        stap = 5;
                    }
                    break;

                case 5:
                    string? opmerkingKeuze = KiesOpmerking();
                    if (opmerkingKeuze == null)
                    {
                        stap = 4;
                    }
                    else
                    {
                        opmerking = opmerkingKeuze;
                        stap = 6;
                    }
                    break;

                case 6:
                    bool? bevestiging = BevestigReservering(aantalPersonen, gekozenDatum, gekozenTijdslot, gekozenTafelNummer, opmerking);
                    if (bevestiging == null)
                    {
                        stap = 5;
                    }
                    else if (bevestiging == true)
                    {
                        bool gelukt = ReservationLogic.AddReservering(
                            gebruikerID,
                            aantalPersonen,
                            gekozenTijdslot,
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

    private int? KiesAantalPersonen()
    {
        List<int> opties = ReservationLogic.GetAantalPersonenOpties();
        int geselecteerd = 0;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("==================================");
            Console.WriteLine("       KIES AANTAL PERSONEN       ");
            Console.WriteLine("==================================");
            Console.WriteLine();
            Console.WriteLine("Gebruik ↑ en ↓ om te kiezen.");
            Console.WriteLine("Druk op Enter om te bevestigen.");
            Console.WriteLine("Druk op Escape om terug te gaan.");
            Console.WriteLine();

            for (int i = 0; i < opties.Count; i++)
            {
                if (i == geselecteerd)
                {
                    Console.WriteLine($"> {opties[i]} personen");
                }
                else
                {
                    Console.WriteLine($"  {opties[i]} personen");
                }
            }

            ConsoleKeyInfo key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.UpArrow && geselecteerd > 0)
            {
                geselecteerd--;
            }
            else if (key.Key == ConsoleKey.DownArrow && geselecteerd < opties.Count - 1)
            {
                geselecteerd++;
            }
            else if (key.Key == ConsoleKey.Enter)
            {
                return opties[geselecteerd];
            }
            else if (key.Key == ConsoleKey.Escape)
            {
                return null;
            }
        }
    }

    private DateTime? KiesDatum()
    {
        List<DateTime> datums = ReservationLogic.GetBeschikbareDatums();
        int geselecteerd = 0;

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

            if (key.Key == ConsoleKey.LeftArrow && geselecteerd > 0)
            {
                geselecteerd--;
            }
            else if (key.Key == ConsoleKey.RightArrow && geselecteerd < datums.Count - 1)
            {
                geselecteerd++;
            }
            else if (key.Key == ConsoleKey.Enter)
            {
                return datums[geselecteerd];
            }
            else if (key.Key == ConsoleKey.Escape)
            {
                return null;
            }
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

    private Tijdslot? KiesTijdslot(int aantalPersonen, DateTime datum)
    {
        List<Tijdslot> tijdsloten = ReservationLogic.GetBeschikbareTijdsloten(aantalPersonen, datum);
        int geselecteerd = 0;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("==================================");
            Console.WriteLine("          KIES TIJDSLOT           ");
            Console.WriteLine("==================================");
            Console.WriteLine();
            Console.WriteLine($"Aantal personen: {aantalPersonen}");
            Console.WriteLine($"Datum: {datum:dd-MM-yyyy}");
            Console.WriteLine();
            Console.WriteLine("Gebruik ↑ en ↓ om te kiezen.");
            Console.WriteLine("Druk op Enter om te bevestigen.");
            Console.WriteLine("Druk op Escape om terug te gaan.");
            Console.WriteLine();

            if (tijdsloten.Count == 0)
            {
                Console.WriteLine("Er zijn geen beschikbare tijdsloten op deze datum.");
                Console.WriteLine();
                Console.WriteLine("Druk op Escape om terug te gaan.");

                ConsoleKeyInfo legeKey = Console.ReadKey(true);
                if (legeKey.Key == ConsoleKey.Escape)
                {
                    return null;
                }

                continue;
            }

            for (int i = 0; i < tijdsloten.Count; i++)
            {
                string label = $"{DateTime.Parse(tijdsloten[i].StartTijd):HH:mm} - {DateTime.Parse(tijdsloten[i].EindTijd):HH:mm}";

                if (i == geselecteerd)
                {
                    Console.WriteLine($"> {label}");
                }
                else
                {
                    Console.WriteLine($"  {label}");
                }
            }

            ConsoleKeyInfo key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.UpArrow && geselecteerd > 0)
            {
                geselecteerd--;
            }
            else if (key.Key == ConsoleKey.DownArrow && geselecteerd < tijdsloten.Count - 1)
            {
                geselecteerd++;
            }
            else if (key.Key == ConsoleKey.Enter)
            {
                return tijdsloten[geselecteerd];
            }
            else if (key.Key == ConsoleKey.Escape)
            {
                return null;
            }
        }
    }

    private int? KiesTafel(int aantalPersonen, Tijdslot tijdslot)
    {
        while (true)
        {
            Console.Clear();
            ToonPlattegrond(aantalPersonen, tijdslot);

            Console.WriteLine();
            Console.WriteLine("Typ het tafelnummer en druk op Enter.");
            Console.WriteLine("Typ /back om terug te gaan.");
            Console.Write("> ");

            string? input = Console.ReadLine();

            if (input == "/back")
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

    private void ToonPlattegrond(int aantalPersonen, Tijdslot tijdslot)
    {
        List<TafelWeergave> tafels = ReservationLogic.GetTafelWeergaveVoorTijdslot(aantalPersonen, tijdslot);
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
        Console.WriteLine("              INGANG");
        Console.WriteLine();

        List<TafelWeergave> gesorteerd = tafels.OrderBy(t => t.TafelNummer).ToList();

        for (int i = 0; i < gesorteerd.Count; i++)
        {
            string vak = MaakTafelVak(gesorteerd[i]);
            Console.Write(vak.PadRight(10));

            if ((i + 1) % 3 == 0)
            {
                Console.WriteLine();
                Console.WriteLine();
            }
        }

        if (gesorteerd.Count % 3 != 0)
        {
            Console.WriteLine();
            Console.WriteLine();
        }

        Console.WriteLine("              KEUKEN");
        Console.WriteLine();

        List<int> beschikbareTafels = ReservationLogic.GetBeschikbareTafelNummers(aantalPersonen, tijdslot);

        if (beschikbareTafels.Count == 0)
        {
            Console.WriteLine("Beschikbare tafelnummers: geen");
        }
        else
        {
            Console.WriteLine($"Beschikbare tafelnummers: {string.Join(", ", beschikbareTafels)}");
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

    private bool? BevestigReservering(int aantalPersonen, DateTime datum, Tijdslot tijdslot, int tafelNummer, string opmerking)
    {
        List<string> opties = new List<string> { "Bevestigen", "Terug" };
        int geselecteerd = 0;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("==================================");
            Console.WriteLine("       BEVESTIG RESERVERING       ");
            Console.WriteLine("==================================");
            Console.WriteLine();
            Console.WriteLine($"Aantal personen: {aantalPersonen}");
            Console.WriteLine($"Datum: {datum:dd-MM-yyyy}");
            Console.WriteLine($"Tijdslot: {DateTime.Parse(tijdslot.StartTijd):HH:mm} - {DateTime.Parse(tijdslot.EindTijd):HH:mm}");
            Console.WriteLine($"Tafelnummer: {tafelNummer}");
            Console.WriteLine($"Opmerking: {opmerking}");
            Console.WriteLine();
            Console.WriteLine("Gebruik ↑ en ↓ om te kiezen.");
            Console.WriteLine("Druk op Enter om te bevestigen.");
            Console.WriteLine("Druk op Escape om terug te gaan.");
            Console.WriteLine();

            for (int i = 0; i < opties.Count; i++)
            {
                if (i == geselecteerd)
                {
                    Console.WriteLine($"> {opties[i]}");
                }
                else
                {
                    Console.WriteLine($"  {opties[i]}");
                }
            }

            ConsoleKeyInfo key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.UpArrow && geselecteerd > 0)
            {
                geselecteerd--;
            }
            else if (key.Key == ConsoleKey.DownArrow && geselecteerd < opties.Count - 1)
            {
                geselecteerd++;
            }
            else if (key.Key == ConsoleKey.Enter)
            {
                return geselecteerd == 0;
            }
            else if (key.Key == ConsoleKey.Escape)
            {
                return null;
            }
        }
    }
}