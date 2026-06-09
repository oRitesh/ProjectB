using System;

public class ReserveringOverzichtUI
{
    private readonly ReservationLogic logic;
    private readonly Gebruiker gebruiker;
    private readonly UserLogic userLogic;

    public ReserveringOverzichtUI(ReservationLogic logic, Gebruiker gebruiker, UserLogic userLogic)
    {
        this.logic = logic;
        this.gebruiker = gebruiker;
        this.userLogic = userLogic;
    }

    public void ShowOverzicht()
    {
        while (true)
        {
            var reserveringen = logic.GetReserveringenByGebruikerID(gebruiker.ID);

            if (reserveringen.Count == 0)
            {
                Console.Clear();
                Console.WriteLine("==================================");
                Console.WriteLine("        MIJN RESERVERINGEN        ");
                Console.WriteLine("==================================");
                Console.WriteLine();
                Console.WriteLine("U heeft nog geen reserveringen.");
                Console.ReadKey();
                return;
            }

            var keuze = ArrowMenu.ShowMenu(
                "MIJN RESERVERINGEN",
                reserveringen,
                r => $"{r.StartTijd} - {r.AantalGasten} personen"
            );

            if (keuze == null) return;

            ShowDetails(keuze);
        }
    }

    private void ShowDetails(Reservering r)
    {
        List<string> opties = new()
        {
            "Reservering wijzigen",
            "Opmerking wijzigen",
            "Verwijderen"
        };

        while (true)
        {
            string? keuze = ArrowMenu.ShowMenu(
                "RESERVERINGSDETAILS",
                opties,
                x => x,
                () =>
                {
                    Console.WriteLine($"Datum:         {r.StartTijd}");
                    Console.WriteLine($"Tafel:         {r.TafelID}");
                    Console.WriteLine($"Aantal gasten: {r.AantalGasten}");
                    Console.WriteLine($"Opmerking:     {r.Opmerking}");
                    Console.WriteLine();
                }
            );

            if (keuze == null) return;

            switch (keuze)
            {
                case "Reservering wijzigen":
                    WijzigVolledigeReservering(r);
                    break;

                case "Opmerking wijzigen":
                    WijzigOpmerking(r);
                    break;

                case "Verwijderen":
                    VerwijderReservering(r);
                    return;

                default:
                    break;
                    //Niks hoort te gebeuren
            }
        }
    }

    // reservering wijzigen

    private void WijzigVolledigeReservering(Reservering r)
    {
        var ui = new ReserveringUI(logic, gebruiker, userLogic);

        // aantal personen
        int? nieuwAantal = ui.KiesAantalPersonen();
        if (nieuwAantal == null) return;

        // datum
        DateTime? nieuweDatum = ui.KiesDatum();
        if (nieuweDatum == null) return;

        // tijdslot
        Tijdslot? nieuwTijdslot = ui.KiesTijdslot(nieuwAantal.Value, nieuweDatum.Value);
        if (nieuwTijdslot == null) return;

        // tafel
        int? nieuweTafel = KiesNieuweTafelVoorWijziging(nieuwAantal.Value, nieuwTijdslot);
        if (nieuweTafel == null) return;

        r.AantalGasten = nieuwAantal.Value;
        r.StartTijd = nieuwTijdslot.StartTijd;
        r.EindTijd = nieuwTijdslot.EindTijd;
        r.TafelID = nieuweTafel.Value;

        logic.UpdateReservering(r);

        Console.WriteLine("Reservering succesvol bijgewerkt!");
        Console.ReadKey();
    }

    private int? KiesNieuweTafelVoorWijziging(int aantalPersonen, Tijdslot tijdslot)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Kies een nieuwe tafel ===");

            var tafels = logic.GetTafelWeergaveVoorTijdslot(aantalPersonen, tijdslot);

            Console.WriteLine();
            Console.WriteLine("Legenda:");
            Console.WriteLine("[2] = beschikbaar");
            Console.WriteLine("(2) = gereserveerd");
            Console.WriteLine("-2- = verkeerde capaciteit");
            Console.WriteLine();

            foreach (var t in tafels.OrderBy(t => t.TafelNummer))
            {
                string vak =
                    !t.IsToegestaan ? $"-{t.TafelNummer}-" :
                    !t.IsBeschikbaar ? $"({t.TafelNummer})" :
                    $"[{t.TafelNummer}]";

                Console.Write(vak + " ");
            }

            Console.WriteLine();
            Console.WriteLine("\nTyp het tafelnummer of druk op Escape om terug te gaan:");
            Console.Write("> ");

            string? input = Console.ReadLine();

            if (input == null) return null;

            if (!int.TryParse(input, out int tafelNummer))
            {
                Console.WriteLine("Ongeldige invoer.");
                Console.ReadKey();
                continue;
            }

            if (!logic.IsTafelBeschikbaarVoorKeuze(tafelNummer, aantalPersonen, tijdslot))
            {
                Console.WriteLine("Deze tafel is niet beschikbaar of niet toegestaan.");
                Console.ReadKey();
                continue;
            }

            return tafelNummer;
        }
    }

    //opmerking wijzigen
    private void WijzigOpmerking(Reservering r)
    {
        Console.Clear();
        Console.WriteLine("=== Opmerking wijzigen ===");
        Console.Write("Nieuwe opmerking: ");

        string? nieuweOpmerking = Console.ReadLine();

        r.Opmerking = nieuweOpmerking ?? "";

        logic.UpdateReservering(r);

        Console.WriteLine("Opmerking bijgewerkt!");
        Console.ReadKey();
    }

    // 3. VERWIJDEREN
    private void VerwijderReservering(Reservering r)
    {
        Console.Write("Weet u zeker dat u deze reservering wilt verwijderen (j/n): ");
        string? confirm = Console.ReadLine();

        if (confirm?.ToLower() == "j")
        {
            logic.DeleteReservering(r.ID);
            Console.WriteLine("Reservering verwijderd.");
        }
        else
        {
            Console.WriteLine("Actie geannuleerd.");
        }

        Console.ReadKey();
    }
}
