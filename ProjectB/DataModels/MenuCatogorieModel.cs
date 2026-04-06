public class MenuCategorie
{
    public int ID { get; set; }
    public string Naam { get; set; }
    public string Beschrijving { get; set; }

    public MenuCategorie(int id, string naam, string beschrijving)
    {
        ID = id;
        Naam = naam;
        Beschrijving = beschrijving;
    }
}