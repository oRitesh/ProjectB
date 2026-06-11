using Dapper;

namespace ProjectB.Tests;

[TestClass]
public sealed class ReserveringsProcesTests
{
    private readonly DatabaseContext _db;
    private readonly ReservationLogic _logic;

    public ReserveringsProcesTests()
    {
        _db = new DatabaseContext();
        var reserveringAccess = new ReserveringAccess(_db);
        var tafelAccess = new TafelAccess(_db);
        var tijdslotAccess = new TijdslotAccess(_db);
        var userAccess = new UserAccess(_db);
        _logic = new ReservationLogic(reserveringAccess, tafelAccess, userAccess);
    }

    /// <summary>
    /// Path: Happy Path H3
    /// Input: Datum vandaag + precies 1 maand vooruit
    /// Scenario: Klant reserveert op de maximaal toegestane datum
    /// </summary>
    [TestMethod]
    public void IsGeldigeDatum_PreciesEenMaandVooruit_IsGeldig()
    {
        // arrange
        DateTime datum = DateTime.Today.AddMonths(1);

        // act
        bool resultaat = _logic.IsGeldigeDatum(datum);

        // assert
        Assert.IsTrue(resultaat,
            "Een datum van precies 1 maand vooruit moet worden geaccepteerd");
    }

    /// <summary>
    /// Path: Sad Path S1
    /// Input: Klant op stap 1 (personen); klikt "Terug"
    /// Scenario: Klant probeert terug te gaan op de eerste stap; 0 personen is ongeldig
    /// </summary>
    [TestMethod]
    public void IsGeldigAantalPersonen_NulPersonen_IsOngeldig()
    {
        // arrange
        int aantalPersonen = 0;

        // act
        bool resultaat = _logic.IsGeldigAantalPersonen(aantalPersonen);

        // assert
        Assert.IsFalse(resultaat,
            "0 personen is ongeldig; de eerste stap mag niet worden overgeslagen");
    }

    /// <summary>
    /// Path: Sad Path S2
    /// Input: Klant kiest 8 personen
    /// Scenario: Klant probeert meer dan 6 personen te kiezen
    /// </summary>
    [TestMethod]
    public void IsGeldigAantalPersonen_AchtPersonen_IsOngeldig()
    {
        // setup
        int aantalPersonen = 8;

        // act
        bool resultaat = _logic.IsGeldigAantalPersonen(aantalPersonen);

        // assert
        Assert.IsFalse(resultaat,
            "8 personen overschrijdt het maximum van 6; invoer moet worden geweigerd");
    }

    /// <summary>
    /// Path: Sad Path S3
    /// Input: Klant kiest datum 01-05-2026 (verleden datum)
    /// Scenario: Klant probeert een datum in het verleden te kiezen
    /// </summary>
    [TestMethod]
    public void IsGeldigeDatum_DatumInVerleden_IsOngeldig()
    {
        // arrange
        DateTime datum = DateTime.Today.AddDays(-1);

        // act
        bool resultaat = _logic.IsGeldigeDatum(datum);

        // assert
        Assert.IsFalse(resultaat,
            "Een datum in het verleden mag niet worden geaccepteerd");
    }

    /// <summary>
    /// Path: Sad Path S4
    /// Input: Klant kiest datum meer dan 1 maand vooruit
    /// Scenario: Klant probeert meer dan 1 maand vooruit te reserveren
    /// </summary>
    [TestMethod]
    public void IsGeldigeDatum_MeerDanEenMaandVooruit_IsOngeldig()
    {
        // setup
        DateTime datum = DateTime.Today.AddMonths(1).AddDays(1);

        // act
        bool resultaat = _logic.IsGeldigeDatum(datum);

        // assert
        Assert.IsFalse(resultaat,
            "Een datum meer dan 1 maand vooruit mag niet worden geaccepteerd");
    }

    /// <summary>
    /// Path: Sad Path S5
    /// Input: Klant probeert direct tijdslot te kiezen zonder personen en datum
    /// Scenario: Klant slaat personen en datum over
    /// </summary>
    [TestMethod]
    public void GetBeschikbareTijdsloten_ZonderPersonenEnDatum_RetourneertLeeg()
    {
        // setup
        int aantalPersonen = 0;
        DateTime datum = DateTime.MinValue;

        // act
        var tijdsloten = _logic.GetBeschikbareTijdsloten(aantalPersonen, datum);

        // assert
        Assert.IsEmpty(tijdsloten,
        "Zonder geldig aantal personen en datum mogen geen tijdsloten worden getoond");
    }
}