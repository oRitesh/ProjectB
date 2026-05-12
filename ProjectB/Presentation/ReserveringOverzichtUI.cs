using System;

public class ReserveringOverzichtUI
{
    private readonly ReservationLogic logic;
    private readonly Gebruiker gebruiker;

    public ReserveringOverzichtUI(ReservationLogic logic, Gebruiker gebruiker)
    {
        this.logic = logic;
        this.gebruiker = gebruiker;
    }

    public void ShowOverzicht()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Mijn reserveringen ===");

            var reserveringen = logic.ReserveringAccess.GetReserveringenByGebruikerID(gebruiker.ID);

            if (reserveringen.Count == 0)
            {
                Console.WriteLine("U heeft nog geen reserveringen.");
                Console.ReadKey();
                return;
            }

            for (int i = 0; i < reserveringen.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {reserveringen[i].StartTijd} - {reserveringen[i].AantalGasten} personen");
            }

            Console.WriteLine("\n0. Terug");
            Console.Write("\nKies een reservering: ");

            string? input = Console.ReadLine();
            if (input == "0") return;

            if (!int.TryParse(input, out int keuze) || keuze < 1 || keuze > reserveringen.Count)
            {
                Console.WriteLine("Ongeldige keuze.");
                Console.ReadKey();
                continue;
            }

            ShowDetails(reserveringen[keuze - 1]);
        }
    }

    private void ShowDetails(Reservering r)
    {
        bool bezig = true;

        while (bezig)
        {
            Console.Clear();
            Console.WriteLine("=== Reserveringsdetails ===");
            Console.WriteLine($"Datum: {r.StartTijd}");
            Console.WriteLine($"Tafel: {r.TafelID}");
            Console.WriteLine($"Aantal gasten: {r.AantalGasten}");
            Console.WriteLine($"Opmerking: {r.Opmerking}");
            Console.WriteLine();

            Console.WriteLine("1. Wijzig reservering");
            Console.WriteLine("2. Wijzig opmerking");
            Console.WriteLine("3. Verwijderen");
            Console.WriteLine("0. Terug");
            Console.Write("Maak een keuze: ");

            string? keuze = Console.ReadLine();

            switch (keuze)
            {
                case "1":
                    WijzigVolledigeReservering(r);
                    break;

                case "2":
                    WijzigOpmerking(r);
                    break;

                case "3":
                    VerwijderReservering(r);
                    return;

                case "0":
                    bezig = false;
                    break;

                default:
                    Console.WriteLine("Ongeldige keuze.");
                    Console.ReadKey();
                    break;
            }
        }
    }

    // reservering wijzigen

    private void WijzigVolledigeReservering(Reservering r)
    {
        var ui = new ReserveringUI(logic, gebruiker);

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

        logic.ReserveringAccess.UpdateReservering(r);

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
            int benodigdeCapaciteit = logic.GetBenodigdeCapaciteit(aantalPersonen);

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
            Console.WriteLine("\nTyp het tafelnummer of /back om terug te gaan:");
            Console.Write("> ");

            string? input = Console.ReadLine();

            if (input == "/back")
                return null;

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
    // 2. OPMERKING WIJZIGEN
    private void WijzigOpmerking(Reservering r)
    {
        Console.Clear();
        Console.WriteLine("=== Opmerking wijzigen ===");
        Console.Write("Nieuwe opmerking: ");

        string? nieuweOpmerking = Console.ReadLine();

        r.Opmerking = nieuweOpmerking ?? "";

        logic.ReserveringAccess.UpdateReservering(r);

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
            logic.ReserveringAccess.DeleteReservering(r.ID);
            Console.WriteLine("Reservering verwijderd.");
        }
        else
        {
            Console.WriteLine("Actie geannuleerd.");
        }

        Console.ReadKey();
    }
}
