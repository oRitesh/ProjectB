using Dapper;
namespace ProjectB.Tests;

[TestClass]
public sealed class PersoneelBoekingTests
{
    private readonly DatabaseContext _db = DatabaseContext.Instance;
    private readonly ReserveringAccess _reserveringAccess;
    private readonly bestellingAccess _bestellingAccess;
    private readonly UserAccess _userAccess;

    public PersoneelBoekingTests()
    {
        _db = DatabaseContext.Instance;
        _reserveringAccess = new ReserveringAccess();
        _bestellingAccess = new bestellingAccess();
        _userAccess = new UserAccess();
    }

    [TestInitialize]
    public void Initialize()
    {
        _db.Connection.Execute("DELETE FROM Bestelling;");
        _db.Connection.Execute("DELETE FROM Reservering;");
        _db.Connection.Execute($"DELETE FROM {UserAccess.table} WHERE Email LIKE '%@test.com';");
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

        // assert
        Assert.AreEqual("Bezig met bereiden", bijgewerkt?.Status,
            "Status moet succesvol worden bijgewerkt naar 'Bezig met bereiden'");
    }

    /// <summary>
    /// S2: Personeel bekijkt alle reserveringen; geen reserveringen aanwezig
    /// Scenario: Lege lijst getoond; geen foutmelding
    /// </summary>
    [TestMethod]
    public void GetAllReserveringen_GeenReserveringen_RetourneertLeeg()
    {
        // arrange — Cleanup heeft de tabel al leeggemaakt voor deze test

        // act
        var resultaat = _reserveringAccess.GetAllReserveringen();

        // assert
        Assert.AreEqual(0, resultaat.Count,
            "Een lege lijst moet worden getoond als er geen reserveringen in de database zijn");
    }
}