public class Gebruiker
{
    public int ID { get; set; }
    public int Rol { get; set; }
    public string Naam { get; set; }
    public string? Email { get; set; }
    public string Telefoonnummer { get; set; }
    public string Wachtwoord { get; set; }

    public Gebruiker(int id, int rol, string naam, string? email, string telefoonnummer, string wachtwoord)
    {
        ID = id;
        Rol = rol;
        Naam = naam;
        Email = email;
        Telefoonnummer = telefoonnummer;
        Wachtwoord = wachtwoord;
    }

    public Gebruiker(int id, string naam, string telefoonnummer)
        : this(id, 0, naam, null, telefoonnummer, "")
    {
    }
}