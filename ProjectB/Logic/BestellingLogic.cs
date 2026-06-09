public class BestellingLogic
{
    private readonly bestellingAccess bestellingAccess;

    public BestellingLogic(DatabaseContext db)
    {
        bestellingAccess = new bestellingAccess(db);
    }

    public List<Bestelling> GetAllBestellingen() => bestellingAccess.GetAllBestellingen();

    public void UpdateStatus(int bestellingId, string nieuweStatus) => bestellingAccess.UpdateStatus(bestellingId, nieuweStatus);

    public void DeleteAllBestellingen() => bestellingAccess.DeleteAllBestellingen();
}
