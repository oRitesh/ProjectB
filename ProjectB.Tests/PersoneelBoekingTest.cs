using Dapper;

namespace ProjectB.Tests;

[TestClass]
public sealed class PersoneelBoekingTests
{
    private readonly DatabaseContext _db;
    private readonly ReserveringAccess _reserveringAccess;
    private readonly bestellingAccess _bestellingAccess;
    private readonly UserAccess _userAccess;

    public PersoneelBoekingTests()
    {
        _db = new DatabaseContext();
        _reserveringAccess = new ReserveringAccess(_db);
        _bestellingAccess = new bestellingAccess(_db);
        _userAccess = new UserAccess(_db);
    }

    /// <summary>
    /// H3: Personeel wijzigt status van een afhaalbestelling
    /// Scenario: Status succesvol bijgewerkt naar "Bezig met bereiden"
    /// </summary>
    [TestMethod]
    public void UpdateStatus_GeldigeStatus_StatusBijgewerkt()
    {
        // arrange
        int gebruikerID = _userAccess.AddUser(new Gebruiker(0, 1, "Test", "h3@test.com", "0612345670", "Test123!"));
        int id = _bestellingAccess.AddBestelling(new Bestelling(0, gebruikerID, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 10.00m, "18:00", "Ontvangen"));

        // act
        _bestellingAccess.UpdateStatus(id, "Bezig met bereiden");
        var bijgewerkt = _bestellingAccess.GetBestellingenVanVandaag()
            .FirstOrDefault(b => b.ID == id);

        // cleanup
        _bestellingAccess.DeleteBestelling(id);
        _db.Connection.Execute($"DELETE FROM {UserAccess.table} WHERE ID = @ID", new { ID = gebruikerID });

        // assert
        Assert.AreEqual("Bezig met bereiden", bijgewerkt?.Status,
            "Status moet succesvol worden bijgewerkt naar 'Bezig met bereiden'");
    }

    /// <summary>
    /// S2: Personeel bekijkt alle reserveringen; database is leeg
    /// Scenario: Lege lijst getoond; geen foutmelding
    /// </summary>
    [TestMethod]
    public void GetAllReserveringen_LegeDatabase_RetourneertLeeg()
    {
        // arrange
        var alleReserveringen = _reserveringAccess.GetAllReserveringen();
        foreach (var r in alleReserveringen)
            _reserveringAccess.DeleteReservering(r.ID);

        // act
        var resultaat = _reserveringAccess.GetAllReserveringen();

        // assert
        Assert.IsEmpty(resultaat,
            "Een lege lijst moet worden getoond als er geen reserveringen in de database zijn");
    }

}