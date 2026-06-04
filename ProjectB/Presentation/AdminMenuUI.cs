using System.Globalization;
using Dapper;

public class AdminMenuUI
{
    private readonly MenuItemAccess menuItemAccess;
    private readonly ReserveringAccess reserveringAccess;
    private readonly MenuCategorieAccess menuCategorieAccess;
    private readonly bestellingAccess BestellingAccess;
    private readonly OpeningsTijdenAccess openingsTijdenAccess;
    private readonly OpeningsDagAccess openingsDagAccess;

    private readonly DatabaseContext menuItemDb;
    private readonly DatabaseContext reserveringDb;
    private readonly DatabaseContext menuCategorieDb;
    private readonly DatabaseContext bestellingDb;
    private readonly DatabaseContext openingsDb;

    public AdminMenuUI()
    {
        this.menuItemDb = new DatabaseContext();
        this.reserveringDb = new DatabaseContext();
        this.menuCategorieDb = new DatabaseContext();
        this.bestellingDb = new DatabaseContext();
        this.openingsDb = new DatabaseContext();

        this.menuItemAccess = new MenuItemAccess(menuItemDb);
        this.reserveringAccess = new ReserveringAccess(reserveringDb);
        this.menuCategorieAccess = new MenuCategorieAccess(menuCategorieDb);
        this.BestellingAccess = new bestellingAccess(bestellingDb);
        this.openingsTijdenAccess = new OpeningsTijdenAccess(openingsDb);
        this.openingsDagAccess = new OpeningsDagAccess(openingsDb);
    }

    ~AdminMenuUI()
    {
        menuItemDb?.Close();
        reserveringDb?.Close();
        menuCategorieDb?.Close();
        bestellingDb?.Close();
        openingsDb?.Close();
    }

    private List<Tijdslot> MaakTijdslotenVoorDatum(DateTime datum)
    {
        List<Tijdslot> tijdsloten = new List<Tijdslot>();

        string datumString = datum.ToString("yyyy-MM-dd");

        DateTime start = datum.Date.AddHours(17);
        DateTime laatsteStart = datum.Date.AddHours(22);

        while (start <= laatsteStart)
        {
            DateTime eind = start.AddHours(2);

            tijdsloten.Add(new Tijdslot(
                0,
                datumString,
                start.ToString("yyyy-MM-dd HH:mm:ss"),
                eind.ToString("yyyy-MM-dd HH:mm:ss")
            ));

            start = start.AddMinutes(15);
        }

        return tijdsloten;
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

    public void ShowAdminMenu()
    {
        List<string> opties = new()
        {
            "Wijzig menukaart",
            "Bekijk alle reserveringen",
            "Bekijk reserveringen per tijdslot",
            "Wis bestelling geheugen",
            "Bekijk alle bestellingen",
            "Wijzig bestelling status",
            "Wijzig openingstijden",
            "Terug naar hoofdmenu"
        };

        while (true)
        {
            string? keuze = ArrowMenu.ShowMenu("ADMIN MENU", opties, x => x);

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
        var categorieen = menuCategorieAccess.GetAllCategories();

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
        decimal prijs = decimal.Parse(Console.ReadLine() ?? "0");

        Console.Write("Beschrijving : ");
        string beschrijving = Console.ReadLine() ?? "";

        Console.Write("Allergenen   : ");
        string allergeen = Console.ReadLine() ?? "";

        Console.Write("Bereidingstijd: ");
        int bereidingsTijd = int.Parse(Console.ReadLine() ?? "0");

        menuItemAccess.AddMenuItem(new MenuItem
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
        var items = menuItemAccess.GetAllMenuItems();

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

        var categorieen = menuCategorieAccess.GetAllCategories();

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
        input = Console.ReadLine() ?? "";
        decimal updatePrijs = string.IsNullOrWhiteSpace(input) ? gekozenItem.Prijs : decimal.Parse(input);

        Console.Write($"Beschrijving [{gekozenItem.Beschrijving}]: ");
        input = Console.ReadLine() ?? "";
        string updateBeschrijving = string.IsNullOrWhiteSpace(input) ? gekozenItem.Beschrijving : input;

        Console.Write($"Allergenen   [{gekozenItem.Allergeen}]: ");
        input = Console.ReadLine() ?? "";
        string updateAllergeen = string.IsNullOrWhiteSpace(input) ? gekozenItem.Allergeen : input;

        Console.Write($"Bereidingstijd [{gekozenItem.BereidingsTijd}]: ");
        input = Console.ReadLine() ?? "";
        int updateBereidingsTijd = string.IsNullOrWhiteSpace(input) ? gekozenItem.BereidingsTijd : int.Parse(input);

        menuItemAccess.UpdateMenuItem(new MenuItem
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
        var items = menuItemAccess.GetAllMenuItems();

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

        menuItemAccess.DeleteMenuItem(teVerwijderen.ID);

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

        var reserveringen = reserveringAccess.GetAllReserveringen();

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
        List<string> datums = reserveringAccess.GetAllReserveringen()
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

        List<Tijdslot> tijdsloten = MaakTijdslotenVoorDatum(DateTime.Parse(gekozenDatum));

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

        var reserveringen = reserveringAccess.GetOverlappendeReserveringenVoorTijdslot(geselecteerd);

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

        var bestellingen = BestellingAccess.GetAllBestellingen();

        if (bestellingen.Count == 0)
        {
            Console.WriteLine("Er zijn nog geen bestellingen.");
            Console.ReadKey(true);
            return;
        }

        for (int i = 0; i < bestellingen.Count; i++)
        {
            var b = bestellingen[i];
            Console.WriteLine($"{i + 1}.\nBestelling: {b.ID}\nGebruiker: {b.GebruikerID}\nStatus: {b.Status}\n");
        }

        Console.WriteLine("\nKies een bestelling (nummer): ");
        string? input = Console.ReadLine();

        if (!int.TryParse(input, out int keuze))
        {
            Console.WriteLine("Ongeldige invoer.");
            return;
        }
        else if (keuze < 1)
        {
            Console.WriteLine("Nummer moet minimaal 1 zijn.");
            return;
        }
        else if (keuze > bestellingen.Count)
        {
            Console.WriteLine("Nummer bestaat niet in de lijst.");
            return;
        }

        var gekozen = bestellingen[keuze - 1];

        var itemAccess = new BestellingMenuItemAccess(new DatabaseContext());
        var items = itemAccess.GetBestellingMenuItemsByBestellingId(gekozen.ID);

        Console.Clear();
        Console.WriteLine($"=== BESTELLING {gekozen.ID} ===\n");
        Console.WriteLine($"Gebruiker ID : {gekozen.GebruikerID}");
        Console.WriteLine($"Status       : {gekozen.Status}");
        Console.WriteLine($"Ophaaltijd   : {gekozen.OphaalTijd}");
        Console.WriteLine($"Prijs  : €{gekozen.TotaalPrijs}\n");

        Console.WriteLine("Bestelde items:");
        Console.WriteLine("----------------------------------");

        if (items.Count == 0)
        {
            Console.WriteLine("Geen items gevonden.");
        }
        else
        {
            foreach (var item in items)
            {
                Console.WriteLine($"Item naam: {menuItemAccess.GetMenuItemNameById(item.MenuItemID)}\nAantal: {item.Aantal}\nPrijs: €{item.PrijsPerStuk}\n");
            }
        }

        Console.WriteLine("\nDruk op een toets om terug te gaan...");
        Console.ReadKey(true);
    }

    public void AanpassenBestellingStatus()
    {
        Console.Clear();
        Console.WriteLine("=== BESTELLING STATUS WIJZIGEN ===\n");

        var bestellingen = BestellingAccess.GetAllBestellingen();

        if (bestellingen.Count == 0)
        {
            Console.WriteLine("Er zijn geen bestellingen.");
            Console.ReadKey(true);
            return;
        }

        for (int i = 0; i < bestellingen.Count; i++)
        {
            var b = bestellingen[i];
            Console.WriteLine($"{i + 1}.\nBestelling: {b.ID}\nGebruiker: {b.GebruikerID}\nStatus: {b.Status}\n");
        }

        Console.WriteLine("\nKies een bestelling (nummer): ");
        string? input = Console.ReadLine();

        if (!int.TryParse(input, out int keuze))
        {
            Console.WriteLine("Ongeldige invoer.");
            return;
        }
        else if (keuze < 1)
        {
            Console.WriteLine("Nummer moet minimaal 1 zijn.");
            return;
        }
        else if (keuze > bestellingen.Count)
        {
            Console.WriteLine("Nummer bestaat niet in de lijst.");
            return;
        }

        var gekozen = bestellingen[keuze - 1];

        Console.Clear();
        Console.WriteLine($"=== STATUS WIJZIGEN VOOR BESTELLING #{gekozen.ID} ===\n");

        Console.WriteLine("1. Bezig met bereiden");
        Console.WriteLine("2. Bestelling bereid");
        Console.WriteLine("3. Bestelling afgerond (verwijderen)");
        Console.WriteLine("4. Annuleren\n");

        Console.Write("Kies een optie: ");
        string? statusInput = Console.ReadLine();

        if (!int.TryParse(statusInput, out int statusKeuze))
        {
            Console.WriteLine("Ongeldige invoer.");
            return;
        }

        switch (statusKeuze)
        {
            case 1:
                BestellingAccess.UpdateStatus(gekozen.ID, "Bezig met bereiden");
                break;

            case 2:
                BestellingAccess.UpdateStatus(gekozen.ID, "Bestelling bereid");
                break;

            case 3:
                BestellingAccess.DeleteBestelling(gekozen.ID);

                Console.Clear();
                Console.WriteLine($"Bestelling #{gekozen.ID} is verwijderd.");
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
        Console.Clear();
        Console.WriteLine("=== BESTELLINGSGEHEUGEN WISSEN ===\n");
        Console.WriteLine("Weet je zeker dat je alle bestellingen wilt verwijderen?");
        Console.WriteLine("Deze actie kan niet ongedaan worden gemaakt.\n");
        Console.WriteLine("1. Ja, wis alle bestellingen");
        Console.WriteLine("2. Nee, annuleer\n");
        Console.Write("Kies een optie: ");
        string? input = Console.ReadLine();

        if (input == "1")
        {
            BestellingAccess.DeleteAllBestellingen();
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
        OpeningsTijden? tijden = openingsTijdenAccess.GetOpeningsTijden();

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

        if (!TimeSpan.TryParse(nieuweOpening, out _) || !TimeSpan.TryParse(nieuweSluiting, out _))
        {
            Console.WriteLine("Ongeldige tijd. Gebruik bijvoorbeeld 17:00 of 00:00.");
            Console.ReadKey(true);
            return;
        }

        tijden.OpeningsTijd = nieuweOpening;
        tijden.SluitingsTijd = nieuweSluiting;

        openingsTijdenAccess.UpdateOpeningsTijden(tijden);

        Console.WriteLine("Openingstijden bijgewerkt.");
        Console.ReadKey(true);
    }

    private void WijzigOpeningsDagen()
    {
        while (true)
        {
            List<OpeningsDag> dagen = openingsDagAccess.GetAllOpeningsDagen();

            OpeningsDag? gekozenDag = ArrowMenu.ShowMenu(
                "OPENINGSDAGEN WIJZIGEN",
                dagen,
                d => $"{DagNaam(d.DagVanWeek)} - {(d.IsOpen == 1 ? "Open" : "Gesloten")}"
            );

            if (gekozenDag == null)
            {
                return;
            }

            gekozenDag.IsOpen = gekozenDag.IsOpen == 1 ? 0 : 1;
            openingsDagAccess.UpdateOpeningsDag(gekozenDag);
        }
    }

    private string DagNaam(int dagVanWeek)
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
}