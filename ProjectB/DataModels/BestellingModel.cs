public class Bestelling
{
    public int ID { get; set; }
    public int GebruikerID { get; set; }
    public int GemaaktOp { get; set; }
    public decimal TotaalPrijs { get; set; }
    public string OphaalTijd { get; set; }

    public Bestelling(int id, int gebruikerID, int gemaaktOp, decimal totaalPrijs, string ophaalTijd)
    {
        ID = id;
        GebruikerID = gebruikerID;
        GemaaktOp = gemaaktOp;
        TotaalPrijs = totaalPrijs;
        OphaalTijd = ophaalTijd;
    }
}