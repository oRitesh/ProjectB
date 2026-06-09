public class BestellingMenuItemLogic
{
    private readonly BestellingMenuItemAccess bestellingMenuItemAccess;

    public BestellingMenuItemLogic(DatabaseContext db)
    {
        bestellingMenuItemAccess = new BestellingMenuItemAccess(db);
    }

    public List<BestellingMenuItem> GetBestellingMenuItemsByBestellingId(int bestellingId)
        => bestellingMenuItemAccess.GetBestellingMenuItemsByBestellingId(bestellingId);
}
