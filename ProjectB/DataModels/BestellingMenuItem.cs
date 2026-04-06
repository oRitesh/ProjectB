public class BestellingMenuItem
{
    public int MenuItemID { get; set; }
    public int BestellingID { get; set; }
    public int Aantal { get; set; }
    public decimal PrijsPerStuk { get; set; }

    public BestellingMenuItem(int menuItemID, int bestellingID, int aantal, decimal prijsPerStuk)
    {
        MenuItemID = menuItemID;
        BestellingID = bestellingID;
        Aantal = aantal;
        PrijsPerStuk = prijsPerStuk;
    }
}