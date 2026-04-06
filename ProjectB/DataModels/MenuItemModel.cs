public class MenuItem
{
    public int ID { get; set; }
    public int MenuCategorieID { get; set; }
    public string Naam { get; set; }
    public decimal Prijs { get; set; }
    public string Beschrijving { get; set; }
    public string Allergenen { get; set; }

    public MenuItem(int id, int menuCategorieID, string naam, int prijs, string beschrijving, string allergenen)
    {
        ID = id;
        MenuCategorieID = menuCategorieID;
        Naam = naam;
        Prijs = prijs;
        Beschrijving = beschrijving;
        Allergenen = allergenen;
    }
}