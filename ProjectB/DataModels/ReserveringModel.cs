public class Reservering
{
    public int ID { get; set; }
    public int GebruikerID { get; set; }
    public int TafelID { get; set; }
    public string StartTijd { get; set; }
    public string EindTijd { get; set; }
    public int AantalGasten { get; set; }
    public string Opmerking { get; set; }
    public string GemaaktOp { get; set; }

    public Reservering(int id, int gebruikerID, int tafelID, string startTijd, string eindTijd, int aantalGasten, string opmerking, string gemaaktOp)
    {
        ID = id;
        GebruikerID = gebruikerID;
        TafelID = tafelID;
        StartTijd = startTijd;
        EindTijd = eindTijd;
        AantalGasten = aantalGasten;
        Opmerking = opmerking;
        GemaaktOp = gemaaktOp;
    }
}