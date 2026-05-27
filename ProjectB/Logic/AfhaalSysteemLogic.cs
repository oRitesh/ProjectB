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
        opties.Add("Zo snel mogelijk");

        var tijd = DateTime.Now.AddMinutes(15);

        for (int i = 0; i < 16; i++)
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
        userAccess.AddUser(gast);
        return gast.ID;
    }   
}