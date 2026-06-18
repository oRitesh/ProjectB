namespace ProjectB.Tests;

[TestClass]
public sealed class AdminBezettingTijdsslotTests
{
    private readonly TimeSlotLogic _tijdslotLogic;
    private readonly ReservationLogic _reservationLogic;
    private readonly ReserveringAccess _reserveringAccess;

    public AdminBezettingTijdsslotTests()
    {
        _tijdslotLogic = new TimeSlotLogic();
        _reserveringAccess = new ReserveringAccess();
        _reservationLogic = new ReservationLogic();
    }

    // ===== Acceptance Criteria 1: Zoeken op datum - H1 =====

    /// <summary>
    /// Path ID: Happy Path H1
    /// Scenario: Admin zoekt tijdsloten op 15-06-2026 en krijgt tijdsloten op die datum terug
    /// </summary>
    [TestMethod]
    public void MaakTijdsloten_GeldigeDatum_RetourneertTijdslotenOpDieDatum()
    {
        // arrange
        DateTime datum = new(2026, 6, 15);

        // act
        var tijdsloten = _tijdslotLogic.MaakTijdslotenVoorAdmin(datum);

        // assert
        Assert.IsNotEmpty(tijdsloten,
            "Er moeten tijdsloten worden gegenereerd voor een geldige datum");
        Assert.IsTrue(tijdsloten.All(t => t.Datum == "2026-06-15"),
            "Alle tijdsloten moeten de datum 2026-06-15 hebben");
    }

    // ===== Acceptance Criteria 1: Zoeken op datum - S1 =====

    /// <summary>
    /// Path ID: Sad Path S1
    /// Scenario: Admin bekijkt een tijdslot waarop geen reserveringen bestaan
    /// </summary>
    [TestMethod]
    public void GetOverlappendeReserveringen_GeenReserveringenOpTijdslot_RetourneertLegeLijst()
    {
        // arrange
        DateTime datum = new(2026, 6, 25);
        var tijdsloten = _tijdslotLogic.MaakTijdslotenVoorAdmin(datum);
        var slot = tijdsloten.First();

        // act
        var reserveringen = _reserveringAccess.GetOverlappendeReserveringenVoorTijdslot(slot);

        // assert
        Assert.IsEmpty(reserveringen,
            "De lijst moet leeg zijn voor een tijdslot waarop geen reserveringen bestaan; verwachte melding: 'Geen reserveringen op deze datum'");
    }

    // ===== Acceptance Criteria 1: Zoeken op datum - S2 =====

    /// <summary>
    /// Path ID: Sad Path S2
    /// Scenario: Admin zoekt beschikbare klant-tijdsloten voor een datum in het verleden
    /// </summary>
    [TestMethod]
    public void GetBeschikbareTijdsloten_DatumInVerleden_RetourneertLegeLijst()
    {
        // Arrange
        DateTime datum = new(2020, 1, 1);

        // Act
        var tijdsloten = _reservationLogic.GetBeschikbareTijdsloten(2, datum);

        // Assert
        Assert.IsEmpty(tijdsloten,
            "Voor een datum in het verleden mogen geen tijdsloten worden teruggegeven");
    }

    // ===== Acceptance Criteria 1: Zoeken op datum - S3 =====

    /// <summary>
    /// Path ID: Sad Path S3
    /// Scenario: Admin zoekt beschikbare tijdsloten voor een datum buiten het boekingsvenster
    /// </summary>
    [TestMethod]
    public void GetBeschikbareTijdsloten_DatumBuitenBoekingsvenster_RetourneertLegeLijst()
    {
        // Arrange
        DateTime datum = DateTime.Today.AddMonths(2); // buiten het geldige boekingsvenster van 1 maand

        // Act
        var tijdsloten = _reservationLogic.GetBeschikbareTijdsloten(2, datum);

        // Assert
        Assert.IsEmpty(tijdsloten,
            "Een datum buiten het boekingsvenster geeft geen tijdsloten terug; geen resultaten mogen worden getoond");
    }
}
