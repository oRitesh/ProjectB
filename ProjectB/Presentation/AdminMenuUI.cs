using System.Globalization;

public class AdminMenuUI
{
    private readonly MenuItemLogic menuItemLogic;
    private readonly ReservationLogic reservationLogic;
    private readonly MenuCategorieLogic menuCategorieLogic;
    private readonly BestellingLogic bestellingLogic;
    private readonly BestellingMenuItemLogic bestellingMenuItemLogic;
    private readonly OpeningsTijdenLogic openingsTijdenLogic;
    private readonly OpeningsDagLogic openingsDagLogic;
    private readonly TimeSlotLogic timeSlotLogic;

    public AdminMenuUI()
    {
        this.menuItemLogic = new MenuItemLogic();
        this.reservationLogic = new ReservationLogic();
        this.menuCategorieLogic = new MenuCategorieLogic();
        this.bestellingLogic = new BestellingLogic();
        this.bestellingMenuItemLogic = new BestellingMenuItemLogic();
        this.openingsTijdenLogic = new OpeningsTijdenLogic();
        this.openingsDagLogic = new OpeningsDagLogic();
        this.timeSlotLogic = new TimeSlotLogic();
    }

    private void ToonReserveringKaart(Reservering r, int nummer)
    {
        CultureInfo nl = new CultureInfo("nl-NL");
        string startTijd = DateTime.TryParse(r.StartTijd, out DateTime st) ? st.ToString("HH:mm") : r.StartTijd;
        string eindTijd = DateTime.TryParse(r.EindTijd, out DateTime et) ? et.ToString("HH:mm") : r.EindTijd;
        string datum = DateTime.TryParse(r.StartTijd, out DateTime sd) ? sd.ToString("dd MMMM yyyy", nl) : "";
        string opmerking = string.IsNullOrWhiteSpace(r.Opmerking) ? "–" : r.Opmerking;

        Console.WriteLine($"  ┌─ Reservering #{nummer} ──────────────────────────┐");
        Console.WriteLine($"  │  Datum      : {datum,-28}│");
        Console.WriteLine($"  │  Tijdslot   : {startTijd} – {eindTijd,-20}│");
        Console.WriteLine($"  │  Tafel      : #{r.TafelID,-27}│");
        Console.WriteLine($"  │  Gasten     : {r.AantalGasten,-28}│");
        Console.WriteLine($"  │  Opmerking  : {opmerking,-28}│");
        Console.WriteLine($"  │  Geboekt op : {r.GemaaktOp,-28}│");
        Console.WriteLine($"  └───────────────────────────────────────────┘");
        Console.WriteLine();
    }

    // ─────────────────────────────────────────────
    //  Hoofdmenu
    // ─────────────────────────────────────────────
    public void ShowAdminMenu(int rol = 2)
    {
        List<string> opties = new();

        if (rol == 2)
            opties.Add("Wijzig menukaart");

        opties.Add("Bekijk alle reserveringen");
        opties.Add("Bekijk reserveringen per tijdslot");

        if (rol == 2)
            opties.Add("Wis bestelling geheugen");

        opties.Add("Bekijk alle bestellingen");
        opties.Add("Wijzig bestelling status");

        if (rol == 2)
            opties.Add("Wijzig openingstijden");
        opties.Add("Terug naar hoofdmenu");


        while (true)
        {
            string? keuze = ArrowMenu.ShowMenu(@"                  
  ▄▄▄▄   ▄▄▄▄▄▄   ▄▄▄      ▄▄▄ ▄▄▄▄▄ ▄▄▄    ▄▄▄   ▄▄▄      ▄▄▄  ▄▄▄▄▄▄▄ ▄▄▄    ▄▄▄ ▄▄▄  ▄▄▄ 
▄██▀▀██▄ ███▀▀██▄ ████▄  ▄████  ███  ████▄  ███   ████▄  ▄████ ███▀▀▀▀▀ ████▄  ███ ███  ███ 
███  ███ ███  ███ ███▀████▀███  ███  ███▀██▄███   ███▀████▀███ ███▄▄    ███▀██▄███ ███  ███ 
███▀▀███ ███  ███ ███  ▀▀  ███  ███  ███  ▀████   ███  ▀▀  ███ ███      ███  ▀████ ███▄▄███ 
███  ███ ██████▀  ███      ███ ▄███▄ ███    ███   ███      ███ ▀███████ ███    ███ ▀██████▀ 
", opties, x => x);

            switch (keuze)
            {
                case "Wijzig menukaart": EditMenu(); break;
                case "Bekijk alle reserveringen": ViewReservations(); break;
                case "Bekijk reserveringen per tijdslot": ViewReservationsPerTimeSlot(); break;
                case "Wis bestelling geheugen": WisBestellingGeheugen(); break;
                case "Bekijk alle bestellingen": BekijkBestellingen(); break;
                case "Wijzig bestelling status": AanpassenBestellingStatus(); break;
                case "Wijzig openingstijden": WijzigOpeningstijden(); break;
                case "Terug naar hoofdmenu": return;
                case null: return;
            }
        }
    }

    public void EditMenu()
    {
        List<string> opties = new()
        {
            "Voeg menu-item toe",
            "Werk menu-item bij",
            "Verwijder menu-item",
            "Terug naar Admin Menu"
        };

        while (true)
        {
            string? keuze = ArrowMenu.ShowMenu("WIJZIG MENUKAART", opties, x => x);

            switch (keuze)
            {
                case "Voeg menu-item toe": VoegMenuItemToe(); break;
                case "Werk menu-item bij": WerkMenuItemBij(); break;
                case "Verwijder menu-item": VerwijderMenuItem(); break;
                case "Terug naar Admin Menu": return;
                case null: return;
            }
        }
    }

    private void VoegMenuItemToe()
    {
        var categorieen = menuCategorieLogic.GetAllCategories();

        if (categorieen.Count == 0)
        {
            Console.Clear();
            Console.WriteLine("Er zijn geen categorieën beschikbaar.");
            Console.WriteLine("Druk op een toets om verder te gaan...");
            Console.ReadKey(true);
            return;
        }

        MenuCategorie? gekozenCat = ArrowMenu.ShowMenu(
            "KIES CATEGORIE",
            categorieen,
            c => c.Naam
        );

        if (gekozenCat == null) return;

        Console.Clear();
        Console.WriteLine("== Voeg menu-item toe ==");
        Console.Write("Naam         : ");
        string naam = Console.ReadLine() ?? "";

        Console.Write("Prijs        : ");
        string prijsInput = Console.ReadLine() ?? "";

        if (!decimal.TryParse(prijsInput, out decimal prijs) || !menuItemLogic.IsGeldigePrijs(prijs))
        {
            Console.WriteLine("Je kan geen negatieve prijs invoeren.");
            Console.ReadKey(true);
            return;
        }


        Console.Write("Beschrijving : ");
        string beschrijving = Console.ReadLine() ?? "";

        Console.Write("Allergenen   : ");
        string allergeen = Console.ReadLine() ?? "";

        Console.Write("Bereidingstijd: ");
        int bereidingsTijd = int.Parse(Console.ReadLine() ?? "0");

        menuItemLogic.AddMenuItem(new MenuItem
        {
            Naam = naam,
            Prijs = prijs,
            MenuCatogorieID = gekozenCat.ID,
            Beschrijving = beschrijving,
            Allergeen = allergeen,
            BereidingsTijd = bereidingsTijd
        });

        Console.Clear();
        Console.WriteLine("Item toegevoegd.");
        Console.WriteLine($"  Naam        : {naam}");
        Console.WriteLine($"  Prijs       : €{prijs:F2}");
        Console.WriteLine($"  Categorie   : {gekozenCat.Naam}");
        Console.WriteLine($"  Beschrijving: {beschrijving}");
        Console.WriteLine($"  Allergenen  : {allergeen}");
        Console.WriteLine($"  Bereidingstijd: {bereidingsTijd} minuten");
        Console.WriteLine();
        Console.WriteLine("Druk op een toets om verder te gaan...");
        Console.ReadKey(true);
    }

    private void WerkMenuItemBij()
    {
        var items = menuItemLogic.GetAllMenuItems();

        if (items.Count == 0)
        {
            Console.Clear();
            Console.WriteLine("Er zijn geen menu-items beschikbaar.");
            Console.WriteLine("Druk op een toets om verder te gaan...");
            Console.ReadKey(true);
            return;
        }

        MenuItem? gekozenItem = ArrowMenu.ShowMenu(
            "KIES ITEM OM TE WIJZIGEN",
            items,
            i => $"{i.Naam}  (€{i.Prijs:F2})"
        );

        if (gekozenItem == null) return;

        var categorieen = menuCategorieLogic.GetAllCategories();

        MenuCategorie? gekozenCat = ArrowMenu.ShowMenu(
            "KIES NIEUWE CATEGORIE",
            categorieen,
            c => c.Naam
        );

        if (gekozenCat == null) return;

        Console.Clear();
        Console.WriteLine($"== Wijzig: {gekozenItem.Naam} ==");
        Console.WriteLine("(Laat leeg om de huidige waarde te bewaren)");
        Console.WriteLine();

        Console.Write($"Naam         [{gekozenItem.Naam}]: ");
        string input = Console.ReadLine() ?? "";
        string updateNaam = string.IsNullOrWhiteSpace(input) ? gekozenItem.Naam : input;

        Console.Write($"Prijs        [€{gekozenItem.Prijs:F2}]: ");
        string prijsInput = Console.ReadLine() ?? "";

        decimal updatePrijs;

        if (string.IsNullOrWhiteSpace(prijsInput))
        {
            updatePrijs = gekozenItem.Prijs;
        }
        else if (!decimal.TryParse(prijsInput, out updatePrijs) || !menuItemLogic.IsGeldigePrijs(updatePrijs))
        {
            Console.WriteLine("Je kan geen negatieve prijs invoeren.");
            Console.ReadKey(true);
            return;
        }


        Console.Write($"Beschrijving [{gekozenItem.Beschrijving}]: ");
        input = Console.ReadLine() ?? "";
        string updateBeschrijving = string.IsNullOrWhiteSpace(input) ? gekozenItem.Beschrijving : input;

        Console.Write($"Allergenen   [{gekozenItem.Allergeen}]: ");
        input = Console.ReadLine() ?? "";
        string updateAllergeen = string.IsNullOrWhiteSpace(input) ? gekozenItem.Allergeen : input;

        Console.Write($"Bereidingstijd [{gekozenItem.BereidingsTijd}]: ");
        input = Console.ReadLine() ?? "";
        int updateBereidingsTijd = string.IsNullOrWhiteSpace(input) ? gekozenItem.BereidingsTijd : int.Parse(input);

        menuItemLogic.UpdateMenuItem(new MenuItem
        {
            Naam = updateNaam,
            Prijs = updatePrijs,
            MenuCatogorieID = gekozenCat.ID,
            Beschrijving = updateBeschrijving,
            Allergeen = updateAllergeen,
            BereidingsTijd = updateBereidingsTijd,
            ID = gekozenItem.ID
        });

        Console.Clear();
        Console.WriteLine("Item bijgewerkt.");
        Console.WriteLine($"  Naam        : {updateNaam}");
        Console.WriteLine($"  Prijs       : €{updatePrijs:F2}");
        Console.WriteLine($"  Categorie   : {gekozenCat.Naam}");
        Console.WriteLine($"  Beschrijving: {updateBeschrijving}");
        Console.WriteLine($"  Allergenen  : {updateAllergeen}");
        Console.WriteLine($"  Bereidingstijd: {updateBereidingsTijd} minuten");
        Console.WriteLine();
        Console.WriteLine("Druk op een toets om verder te gaan...");
        Console.ReadKey(true);
    }

    private void VerwijderMenuItem()
    {
        var items = menuItemLogic.GetAllMenuItems();

        if (items.Count == 0)
        {
            Console.Clear();
            Console.WriteLine("Er zijn geen menu-items beschikbaar.");
            Console.WriteLine("Druk op een toets om verder te gaan...");
            Console.ReadKey(true);
            return;
        }

        MenuItem? teVerwijderen = ArrowMenu.ShowMenu(
            "KIES ITEM OM TE VERWIJDEREN",
            items,
            i => $"{i.Naam}  (€{i.Prijs:F2})"
        );

        if (teVerwijderen == null) return;

        List<string> bevestigOpties = new() { "Ja, verwijderen", "Nee, annuleren" };

        string? bevestig = ArrowMenu.ShowMenu(
            $"VERWIJDER '{teVerwijderen.Naam}'?",
            bevestigOpties,
            x => x
        );

        if (bevestig != "Ja, verwijderen") return;

        menuItemLogic.DeleteMenuItem(teVerwijderen.ID);

        Console.Clear();
        Console.WriteLine($"'{teVerwijderen.Naam}' verwijderd.");
        Console.WriteLine();
        Console.WriteLine("Druk op een toets om verder te gaan...");
        Console.ReadKey(true);
    }

    public void ViewReservations()
    {
        Console.Clear();
        Console.WriteLine("==================================");
        Console.WriteLine("        ALLE RESERVERINGEN        ");
        Console.WriteLine("==================================");
        Console.WriteLine();

        var reserveringen = reservationLogic.GetAllReserveringen();

        if (reserveringen.Count == 0)
        {
            Console.WriteLine("  Er zijn nog geen reserveringen.");
        }
        else
        {
            for (int i = 0; i < reserveringen.Count; i++)
            {
                ToonReserveringKaart(reserveringen[i], i + 1);
            }
        }

        Console.WriteLine("Druk op een toets om terug te gaan...");
        Console.ReadKey(true);
    }

    public void ViewReservationsPerTimeSlot()
    {
        List<string> datums = reservationLogic.GetAllReserveringen()
            .Select(r => DateTime.Parse(r.StartTijd).ToString("yyyy-MM-dd"))
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        if (datums.Count == 0)
        {
            Console.Clear();
            Console.WriteLine("Er zijn geen datums beschikbaar.");
            Console.WriteLine("Druk op een toets om terug te gaan...");
            Console.ReadKey(true);
            return;
        }

        string? gekozenDatum = ArrowMenu.ShowMenu("KIES DATUM", datums, x => x);

        if (gekozenDatum == null)
        {
            return;
        }

        List<Tijdslot> tijdsloten = timeSlotLogic.MaakTijdslotenVoorAdmin(DateTime.Parse(gekozenDatum));

        Tijdslot? geselecteerd = ArrowMenu.ShowMenu(
            $"TIJDSLOT  ({gekozenDatum})",
            tijdsloten,
            ts =>
            {
                string start = DateTime.TryParse(ts.StartTijd, out DateTime s) ? s.ToString("HH:mm") : ts.StartTijd;
                string eind = DateTime.TryParse(ts.EindTijd, out DateTime e) ? e.ToString("HH:mm") : ts.EindTijd;
                return $"{start} – {eind}";
            }
        );

        if (geselecteerd == null)
        {
            return;
        }

        string tijdslotLabel = DateTime.TryParse(geselecteerd.StartTijd, out DateTime sl) ? sl.ToString("HH:mm") : geselecteerd.StartTijd;
        tijdslotLabel += " – " + (DateTime.TryParse(geselecteerd.EindTijd, out DateTime el) ? el.ToString("HH:mm") : geselecteerd.EindTijd);

        Console.Clear();
        Console.WriteLine("==================================");
        Console.WriteLine($"  RESERVERINGEN  {tijdslotLabel,-18}");
        Console.WriteLine($"  Datum: {geselecteerd.Datum,-27}");
        Console.WriteLine("==================================");
        Console.WriteLine();

        var reserveringen = reservationLogic.GetOverlappendeReserveringenVoorTijdslot(geselecteerd);

        if (reserveringen.Count == 0)
        {
            Console.WriteLine("  Geen reserveringen voor dit tijdslot.");
        }
        else
        {
            for (int i = 0; i < reserveringen.Count; i++)
            {
                ToonReserveringKaart(reserveringen[i], i + 1);
            }
        }

        Console.WriteLine("Druk op een toets om terug te gaan...");
        Console.ReadKey(true);
    }

    public void BekijkBestellingen()
    {
        Console.Clear();
        Console.WriteLine("=== ALLE BESTELLINGEN ===\n");

        var bestellingen = bestellingLogic.GetAllBestellingen();

        if (bestellingen.Count == 0)
        {
            Console.WriteLine("Er zijn nog geen bestellingen.");
            Console.ReadKey(true);
            return;
        }

        var gekozen = ArrowMenu.ShowMenu(
           "KIES BESTELLING",
           bestellingen,
           b => $"Bestelling: {b.ID}  |  Gebruiker: {b.GebruikerID}  |  Status: {b.Status}"
       );

        if (gekozen == null) return;

        var items = bestellingMenuItemLogic.GetBestellingMenuItemsByBestellingId(gekozen.ID);

        Console.Clear();
        Console.WriteLine($"  ┌─ Bestelling #{gekozen.ID} ────────────────────────────");
        Console.WriteLine($"  │  Gebruiker  : {gekozen.GebruikerID,-28}");
        Console.WriteLine($"  │  Status     : {gekozen.Status,-28}");
        Console.WriteLine($"  │  Ophaaltijd : {gekozen.OphaalTijd,-28}");
        Console.WriteLine($"  │  Totaalprijs: €{gekozen.TotaalPrijs,-27}");
        Console.WriteLine("   ----------------------------------");

        if (items.Count == 0)
        {
            Console.WriteLine($"  │  {"Geen items gevonden.",-45}");
        }
        else
        {
            foreach (var item in items)
            {
                string naam = menuItemLogic.GetMenuItemNameById(item.MenuItemID);
                string regel = $"{naam}:  x{item.Aantal} - €{item.PrijsPerStuk}";
                Console.WriteLine($"  │  {regel,-45}");
            }
        }
        Console.WriteLine($"  └──────────────────────────────────────────────");
        Console.WriteLine("\nDruk op een toets om terug te gaan...");
        Console.ReadKey(true);
    }

    public void AanpassenBestellingStatus()
    {
        Console.Clear();
        Console.WriteLine("=== BESTELLING STATUS WIJZIGEN ===\n");

        var bestellingen = bestellingLogic.GetAllBestellingen();

        if (bestellingen.Count == 0)
        {
            Console.WriteLine("Er zijn geen bestellingen.");
            Console.ReadKey(true);
            return;
        }

        var gekozen = ArrowMenu.ShowMenu(
           "KIES BESTELLING",
           bestellingen,
           b => $"Bestelling: {b.ID}  |  Gebruiker: {b.GebruikerID}  |  Status: {b.Status}"
       );

        List<string> statusOpties = new()
        {
            "Bezig met bereiden",
            "Bestelling bereid",
            "Bestelling afgerond",
        };

        string? statusKeuze = ArrowMenu.ShowMenu(
            $"STATUS WIJZIGEN VOOR BESTELLING #{gekozen.ID}",
            statusOpties,
            x => x
        );

        switch (statusKeuze)
        {
            case "Bezig met bereiden":
                bestellingLogic.UpdateStatus(gekozen.ID, "Bezig met bereiden");
                break;

            case "Bestelling bereid":
                bestellingLogic.UpdateStatus(gekozen.ID, "Bestelling bereid");
                break;

            case "Bestelling afgerond":
                Console.Clear();
                Console.WriteLine($"Bestelling #{gekozen.ID} is afgerond.");
                Console.ReadKey(true);
                return;

            default:
                return;
        }

        Console.Clear();
        Console.WriteLine($"Status van bestelling #{gekozen.ID} is bijgewerkt.");
        Console.ReadKey(true);
    }

    public void WisBestellingGeheugen()
    {
        List<string> bevestigOpties = new() { "Ja, wis alle bestellingen", "Nee, annuleer" };

        string? keuze = ArrowMenu.ShowMenu(
            "BESTELLINGSGEHEUGEN WISSEN",
            bevestigOpties,
            x => x
        );

        if (keuze == "Ja, wis alle bestellingen")
        {
            bestellingLogic.DeleteAllBestellingen();
            Console.Clear();
            Console.WriteLine("Alle bestellingen zijn verwijderd.");
            Console.ReadKey(true);
        }
    }

    private void WijzigOpeningstijden()
    {
        List<string> opties = new()
        {
            "Wijzig openingstijd en sluitingstijd",
            "Wijzig openingsdagen",
            "Terug"
        };

        while (true)
        {
            string? keuze = ArrowMenu.ShowMenu("OPENINGSTIJDEN BEHEREN", opties, x => x);

            switch (keuze)
            {
                case "Wijzig openingstijd en sluitingstijd":
                    WijzigOpeningsEnSluitingsTijd();
                    break;

                case "Wijzig openingsdagen":
                    WijzigOpeningsDagen();
                    break;

                case "Terug":
                case null:
                    return;
            }
        }
    }

    private void WijzigOpeningsEnSluitingsTijd()
    {
        OpeningsTijden? tijden = openingsTijdenLogic.GetOpeningsTijden();

        if (tijden == null)
        {
            Console.Clear();
            Console.WriteLine("Geen openingstijden gevonden in de database.");
            Console.WriteLine("Druk op een toets om terug te gaan...");
            Console.ReadKey(true);
            return;
        }

        Console.Clear();
        Console.WriteLine("==================================");
        Console.WriteLine("     OPENINGSTIJDEN WIJZIGEN      ");
        Console.WriteLine("==================================");
        Console.WriteLine();
        Console.WriteLine($"Huidige openingstijd : {tijden.OpeningsTijd}");
        Console.WriteLine($"Huidige sluitingstijd: {tijden.SluitingsTijd}");
        Console.WriteLine();
        Console.Write("Nieuwe openingstijd (HH:mm): ");
        string nieuweOpening = Console.ReadLine() ?? "";

        Console.Write("Nieuwe sluitingstijd (HH:mm): ");
        string nieuweSluiting = Console.ReadLine() ?? "";

        if (!openingsTijdenLogic.ZijnGeldigeTijden(nieuweOpening, nieuweSluiting))
        {
            Console.WriteLine("Ongeldige tijd. Gebruik bijvoorbeeld 17:00 of 00:00.");
            Console.ReadKey(true);
            return;
        }

        tijden.OpeningsTijd = nieuweOpening;
        tijden.SluitingsTijd = nieuweSluiting;

        openingsTijdenLogic.UpdateOpeningsTijden(tijden);

        Console.WriteLine("Openingstijden bijgewerkt.");
        Console.ReadKey(true);
    }

    private void WijzigOpeningsDagen()
    {
        while (true)
        {
            List<OpeningsDag> dagen = openingsDagLogic.GetAllOpeningsDagen();

            OpeningsDag? gekozenDag = ArrowMenu.ShowMenu(
                "OPENINGSDAGEN WIJZIGEN",
                dagen,
                d => $"{openingsDagLogic.GetDagNaam(d.DagVanWeek)} - {(d.IsOpen == 1 ? "Open" : "Gesloten")}"
            );

            if (gekozenDag == null)
            {
                return;
            }

            gekozenDag.IsOpen = gekozenDag.IsOpen == 1 ? 0 : 1;
            openingsDagLogic.UpdateOpeningsDag(gekozenDag);
        }
    }

}
