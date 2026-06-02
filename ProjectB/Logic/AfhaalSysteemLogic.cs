public class AfhaalSysteemLogic
{
    private readonly UserAccess userAccess;
    public AfhaalSysteemLogic(DatabaseContext db)
    {
        userAccess = new UserAccess(db);
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
            Winkelwagen.RemoveAt(index);
    }

    public decimal BerekenTotaal()
    {
        return Winkelwagen.Sum(x => x.Item.Prijs * x.Aantal);
    }

    public List<string> GetOphaalTijdOpties()
    {
        var opties = new List<string>();

        DateTime vandaag = DateTime.Today;
        DateTime openingstijd = vandaag.AddHours(17);      // 17:00
        DateTime laatsteOphaalTijd = vandaag.AddHours(23); // 23:00

        DateTime snelsteOphaalTijd = DateTime.Parse(BerekenOphaalTijd());

        if (snelsteOphaalTijd > laatsteOphaalTijd)
        {
            return opties;
        }

        if (snelsteOphaalTijd < openingstijd)
        {
            snelsteOphaalTijd = openingstijd;
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
        int langsteBereidingsTijd = Winkelwagen.Count > 0
            ? Winkelwagen.Max(x => x.Item.BereidingsTijd)
            : 0;

        return DateTime.Now
            .AddMinutes(langsteBereidingsTijd + 15)
            .ToString("HH:mm");
    }
}