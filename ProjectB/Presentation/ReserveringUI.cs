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
                    string? opmerkingKeuze = KiesOpmerking();
                    if (opmerkingKeuze == null)
                    {
                        stap = 3;
                    }
                    else
                    {
                        opmerking = opmerkingKeuze;
                        stap = 5;
                    }
                    break;

                case 5:
                    bool? bevestiging = BevestigReservering(aantalPersonen, gekozenDatum, gekozenTijdslot, opmerking);
                    if (bevestiging == null)
                    {
                        stap = 4;
                    }
                    else if (bevestiging == true)
                    {
                        bool gelukt = ReservationLogic.AddReservering(
                            gebruikerID,
                            aantalPersonen,
                            gekozenTijdslot,
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
                            Console.WriteLine("Er is geen beschikbaarheid meer voor dit tijdslot.");
                        }

                        Console.WriteLine();
                        Console.WriteLine("Druk op een toets om terug te gaan...");
                        Console.ReadKey(true);
                        bezig = false;
                    }
                    else
                    {
                        stap = 4;
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
                string label =
                    $"{DateTime.Parse(tijdsloten[i].StartTijd):HH:mm} - {DateTime.Parse(tijdsloten[i].EindTijd):HH:mm}";

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

    private bool? BevestigReservering(int aantalPersonen, DateTime datum, Tijdslot tijdslot, string opmerking)
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