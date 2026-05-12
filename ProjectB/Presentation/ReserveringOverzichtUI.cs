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
            var reserveringen = logic.ReserveringAccess.GetReserveringenByGebruikerID(gebruiker.ID);

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
            "Aantal gasten wijzigen",
            "Datum + tijdslot wijzigen",
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
                case "Aantal gasten wijzigen":
                    WijzigAantalGasten(r);
                    break;

                case "Datum + tijdslot wijzigen":
                    WijzigDatumEnTijdslot(r);
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
        List<int?> opties = logic.GetAantalPersonenOpties().Cast<int?>().ToList();
        Console.CursorVisible = false;
        try
        {
            return ArrowMenu.ShowMenu(
             "NIEUW AANTAL PERSONEN",
             opties,
             x => x == huidigAantal ? $"{x} personen (huidig)" : $"{x} personen"
         );
        }
        finally
        {
            Console.CursorVisible = true;
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
