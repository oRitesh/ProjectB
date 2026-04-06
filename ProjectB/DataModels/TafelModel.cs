public class Tafel
{
    public int ID { get; set; }
    public int TafelNummer { get; set; }
    public int Capaciteit { get; set; }

    public Tafel(int id, int tafelNummer, int capaciteit)
    {
        ID = id;
        TafelNummer = tafelNummer;
        Capaciteit = capaciteit;
    }
}