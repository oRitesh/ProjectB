public class AfhaalSysteemLogic
{
    private readonly UserAccess userAccess;
    private readonly OpeningsTijdenAccess openingsTijdenAccess;
    private readonly OpeningsDagAccess openingsDagAccess;

    public AfhaalSysteemLogic(DatabaseContext db)
    {
        userAccess = new UserAccess(db);
        openingsTijdenAccess = new OpeningsTijdenAccess(db);
        openingsDagAccess = new OpeningsDagAccess(db);
    }

    public List<(MenuItem Item, int Aantal)> Winkelwagen { get; private set; } = new();

    public void SlaBestellingOp(DatabaseContext db, int gebruikerID, string ophaalTijd, string opmerking)
    {
        var bestellingAccess = new bestellingAccess(db);
        var bestellingMenuItemAccess = new BestellingMenuItemAccess(db);

        if (ophaalTijd.StartsWith("Zo snel mogelijk"))
        {
            ophaalTijd = BerekenOphaalTijd();
        }

        //maak de bestelling aan en haal het nieuwe ID op
        string status = opmerking.Length > 0 ? $"Ontvangen - {opmerking}" : "Ontvangen";
        var bestelling = new Bestelling(0, gebruikerID, DateTime.Now.ToString("HH:mm"), BerekenTotaal(), ophaalTijd, status);
        int bestellingId = bestellingAccess.AddBestelling(bestelling);

        //sla elke item op dat gekoppeld is aan die nieuwe bestellingId
        foreach (var (item, aantal) in Winkelwagen)
        {
            var bestellingMenuItem = new BestellingMenuItem(item.ID, bestellingId, aantal, item.Prijs);
            bestellingMenuItemAccess.AddBestellingMenuItem(bestellingMenuItem);
        }
    }

    public void VoegToe(MenuItem item)
    {
        for (int i = 0; i < Winkelwagen.Count; i++)
        {
            if (Winkelwagen[i].Item.ID == item.ID)
            {
                var entry = Winkelwagen[i];
                Winkelwagen[i] = (entry.Item, entry.Aantal + 1);
                return;
            }
        }
        Winkelwagen.Add((item, 1));
    }

    public void VerwijderItem(int index)
    {
        if (index >= 0 && index < Winkelwagen.Count)
        {
            var entry = Winkelwagen[index];
            if (entry.Aantal > 1)
                Winkelwagen[index] = (entry.Item, entry.Aantal - 1); // verlaag met 1
            else
                Winkelwagen.RemoveAt(index); // verwijder helemaal als het de laatste is
        }
    }

    public decimal BerekenTotaal()
    {
        return Winkelwagen.Sum(x => x.Item.Prijs * x.Aantal);
    }
    private DateTime CombineDatumEnTijd(DateTime datum, string tijd)
    {
        TimeSpan parsedTijd = TimeSpan.Parse(tijd);
        return datum.Date.Add(parsedTijd);
    }

    private int LangsteBereidingsTijd()
    {
        return Winkelwagen.Count > 0
            ? Winkelwagen.Max(x => x.Item.BereidingsTijd)
            : 0;
    }

    public List<string> GetOphaalTijdOpties()
    {
        var opties = new List<string>();

        DateTime vandaag = DateTime.Today;

        if (!openingsDagAccess.IsOpenOpDatum(vandaag))
        {
            return opties;
        }

        OpeningsTijden? tijden = openingsTijdenAccess.GetOpeningsTijden();

        if (tijden == null)
        {
            return opties;
        }

        DateTime openingstijd = CombineDatumEnTijd(vandaag, tijden.OpeningsTijd);
        DateTime sluitingstijd = CombineDatumEnTijd(vandaag, tijden.SluitingsTijd);

        if (sluitingstijd <= openingstijd)
        {
            sluitingstijd = sluitingstijd.AddDays(1);
        }

        DateTime laatsteOphaalTijd = sluitingstijd.AddHours(-1);

        DateTime snelsteOphaalTijd = DateTime.Now.AddMinutes(LangsteBereidingsTijd() + 15);

        if (snelsteOphaalTijd < openingstijd)
        {
            snelsteOphaalTijd = openingstijd;
        }

        if (snelsteOphaalTijd > laatsteOphaalTijd)
        {
            return opties;
        }

        opties.Add($"Zo snel mogelijk ({snelsteOphaalTijd:HH:mm})");

        DateTime tijd = snelsteOphaalTijd.AddMinutes(15);

        while (tijd <= laatsteOphaalTijd)
        {
            opties.Add(tijd.ToString("HH:mm"));
            tijd = tijd.AddMinutes(15);
        }

        return opties;
    }

    public int VoegGastToe(string naam, string telefoon)
    {
        // Rol 0 = gast (zelfde als bij reserveringen)
        var gast = new Gebruiker(0, naam, telefoon);
        int nieuweGebruikerID = userAccess.AddUser(gast);
        return nieuweGebruikerID;
    }

    public string BerekenOphaalTijd()
    {
        DateTime vandaag = DateTime.Today;
        OpeningsTijden? tijden = openingsTijdenAccess.GetOpeningsTijden();

        DateTime snelsteOphaalTijd = DateTime.Now.AddMinutes(LangsteBereidingsTijd() + 15);

        if (tijden != null)
        {
            DateTime openingstijd = CombineDatumEnTijd(vandaag, tijden.OpeningsTijd);

            if (snelsteOphaalTijd < openingstijd)
            {
                snelsteOphaalTijd = openingstijd;
            }
        }

        return snelsteOphaalTijd.ToString("HH:mm");
    }
}