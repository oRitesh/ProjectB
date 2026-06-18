public class BestellingLogic
{
    private readonly bestellingAccess bestellingAccess;

    public BestellingLogic()
    {
        bestellingAccess = new bestellingAccess();
    }

    public void UpdateStatus(int bestellingId, string nieuweStatus) => bestellingAccess.UpdateStatus(bestellingId, nieuweStatus);

    public void DeleteAllBestellingen() => bestellingAccess.DeleteAllBestellingen();

    public List<Bestelling> PakBestellingenVanVandaag() => bestellingAccess.GetBestellingenVanVandaag();

}
