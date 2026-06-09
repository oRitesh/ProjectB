public class BestellingMenuItemLogic
{
    private readonly BestellingMenuItemAccess bestellingMenuItemAccess;

    public BestellingMenuItemLogic()
    {
        bestellingMenuItemAccess = new BestellingMenuItemAccess();
    }

    public List<BestellingMenuItem> GetBestellingMenuItemsByBestellingId(int bestellingId)
        => bestellingMenuItemAccess.GetBestellingMenuItemsByBestellingId(bestellingId);
}
