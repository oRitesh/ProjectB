public class OpeningsTijden
{
    public int ID { get; set; }
    public string OpeningsTijd { get; set; }
    public string SluitingsTijd { get; set; }

    public OpeningsTijden()
    {
        OpeningsTijd = "";
        SluitingsTijd = "";
    }

    public OpeningsTijden(int id, string openingsTijd, string sluitingsTijd)
    {
        ID = id;
        OpeningsTijd = openingsTijd;
        SluitingsTijd = sluitingsTijd;
    }
}