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

            Console.WriteLine("1. Aantal gasten wijzigen");
            Console.WriteLine("2. Datum + tijdslot wijzigen");
            Console.WriteLine("3. Opmerking wijzigen");
            Console.WriteLine("4. Verwijderen");
            Console.WriteLine("0. Terug");
            Console.Write("Maak een keuze: ");

            string? keuze = Console.ReadLine();

            switch (keuze)
            {
                case "1":
                    WijzigAantalGasten(r);
                    break;

                case "2":
                    WijzigDatumEnTijdslot(r);
                    break;

                case "3":
                    WijzigOpmerking(r);
                    break;

                case "4":
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

    //aantal gasten wijzigen

    private void WijzigAantalGasten(Reservering r)
    {
        int? nieuwAantal = KiesAantalPersonenVoorWijziging(r.AantalGasten);

        if (nieuwAantal == null)
            return;

        if (nieuwAantal.Value == r.AantalGasten)
        {
            Console.WriteLine("Je hebt hetzelfde aantal personen gekozen. Geen wijzigingen doorgevoerd.");
            Console.ReadKey();
            return;
        }

        r.AantalGasten = nieuwAantal.Value;

        logic.ReserveringAccess.UpdateReservering(r);

        Console.WriteLine("Aantal gasten succesvol bijgewerkt!");
        Console.ReadKey();
    }

    private int? KiesAantalPersonenVoorWijziging(int huidigAantal)
    {
        var opties = logic.GetAantalPersonenOpties();
        int geselecteerd = opties.IndexOf(huidigAantal);

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Nieuw aantal personen kiezen ===");
            Console.WriteLine("Gebruik ↑ en ↓ om te kiezen.");
            Console.WriteLine("Enter = bevestigen, Escape = terug");
            Console.WriteLine();

            for (int i = 0; i < opties.Count; i++)
            {
                if (i == geselecteerd)
                    Console.WriteLine($"> {opties[i]} personen");
                else
                    Console.WriteLine($"  {opties[i]} personen");
            }

            ConsoleKeyInfo key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.UpArrow && geselecteerd > 0)
                geselecteerd--;

            else if (key.Key == ConsoleKey.DownArrow && geselecteerd < opties.Count - 1)
                geselecteerd++;

            else if (key.Key == ConsoleKey.Enter)
                return opties[geselecteerd];

            else if (key.Key == ConsoleKey.Escape)
                return null;
        }
    }

    //datum en tijdslot wijzigen

    private void WijzigDatumEnTijdslot(Reservering r)
    {
        var ui = new ReserveringUI(logic, gebruiker);

        DateTime? nieuweDatum = ui.KiesDatum();
        if (nieuweDatum == null) return;

        Tijdslot? nieuwTijdslot = ui.KiesTijdslot(r.AantalGasten, nieuweDatum.Value);
        if (nieuwTijdslot == null) return;

        var tafels = logic.TafelAccess.GetTafelsByMinimaleCapaciteit(r.AantalGasten);

        Tafel? beschikbareTafel = null;

        foreach (var tafel in tafels)
        {
            var overlappende = logic.ReserveringAccess.GetOverlappendeReserveringen(
                tafel.ID,
                nieuwTijdslot.StartTijd,
                nieuwTijdslot.EindTijd
            );

            if (overlappende.Count == 0)
            {
                beschikbareTafel = tafel;
                break;
            }
        }

        if (beschikbareTafel == null)
        {
            Console.WriteLine("Geen tafels beschikbaar op dit nieuwe tijdslot.");
            Console.ReadKey();
            return;
        }

        r.StartTijd = nieuwTijdslot.StartTijd;
        r.EindTijd = nieuwTijdslot.EindTijd;
        r.TafelID = beschikbareTafel.ID;

        logic.ReserveringAccess.UpdateReservering(r);

        Console.WriteLine("Reservering succesvol bijgewerkt!");
        Console.ReadKey();
    }

    //opmerking wijzigen
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

    //verwijderen

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
