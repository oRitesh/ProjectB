public class MenuItem
{
    public int ID { get; set; }
    public int MenuCatogorieID { get; set; }
    public string Naam { get; set; }
    public decimal Prijs { get; set; }
    public string Beschrijving { get; set; }
    public string Allergeen { get; set; }
    public int BereidingsTijd { get; set; }

    public MenuItem()
    {
    }
    public MenuItem(int id, int menuCatogorieID, string naam, decimal prijs, string beschrijving, string allergeen, int bereidingsTijd)
    {
        ID = id;
        MenuCatogorieID = menuCatogorieID;
        Naam = naam;
        Prijs = prijs;
        Beschrijving = beschrijving;
        Allergeen = allergeen;
        BereidingsTijd = bereidingsTijd;
    }
}