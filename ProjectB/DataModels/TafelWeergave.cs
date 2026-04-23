public class TafelWeergave
{
    public int TafelID { get; set; }
    public int TafelNummer { get; set; }
    public int Capaciteit { get; set; }
    public bool IsBeschikbaar { get; set; }
    public bool IsToegestaan { get; set; }

    public TafelWeergave() { }

    public TafelWeergave(int tafelID, int tafelNummer, int capaciteit, bool isBeschikbaar, bool isToegestaan)
    {
        TafelID = tafelID;
        TafelNummer = tafelNummer;
        Capaciteit = capaciteit;
        IsBeschikbaar = isBeschikbaar;
        IsToegestaan = isToegestaan;
    }
}