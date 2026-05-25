public class AfhaalSysteemLogic
{
    public List<(MenuItem Item, int Aantal)> Winkelwagen { get; private set; } = new();

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
}