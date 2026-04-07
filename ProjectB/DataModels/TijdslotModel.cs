public class Tijdslot
{
    public int ID { get; set; }
    public string Datum { get; set; }
    public string StartTijd { get; set; }
    public string EindTijd { get; set; }

    public Tijdslot()
    {
    }

    public Tijdslot(int id, string datum, string startTijd, string eindTijd)
    {
        ID = id;
        Datum = datum;
        StartTijd = startTijd;
        EindTijd = eindTijd;
    }
}