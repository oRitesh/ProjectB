using Dapper;

[TestClass]
public class GastReserveringTests
{
    [TestInitialize]
    public void TestInitialize()
    {
        ClearTestData();
        SeedTestData();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        ClearTestData();
    }

    [TestMethod]
    public void VoegGastToe_GeldigeNaamEnTelefoonnummer_ReturnsGebruikerId()
    {
        // Arrange
        ReservationLogic logic = new ReservationLogic();
        string naam = "Test Gast";
        string telefoonnummer = "0612345678";

        // Act
        int gebruikerId = logic.VoegGastToe(naam, telefoonnummer);

        // Assert
        Assert.IsTrue(gebruikerId > 0);
    }

    [TestMethod]
    public void AddReservering_GeldigeGastEnBeschikbareTafel_ReturnsTrue()
    {
        // Arrange
        ReservationLogic logic = new ReservationLogic();

        DateTime datum = DateTime.Today.AddDays(1);

        while ((int)datum.DayOfWeek != 1)
        {
            datum = datum.AddDays(1);
        }

        string datumString = datum.ToString("yyyy-MM-dd");

        int gebruikerId = logic.VoegGastToe("Test Gast", "0612345678");

        Tijdslot tijdslot = new Tijdslot(
            0,
            datumString,
            $"{datumString} 17:00:00",
            $"{datumString} 19:00:00"
        );

        int aantalPersonen = 2;
        int tafelNummer = 1;
        string opmerking = "Test reservering";

        // Act
        bool resultaat = logic.AddReservering(
            gebruikerId,
            aantalPersonen,
            tijdslot,
            tafelNummer,
            opmerking
        );

        // Assert
        Assert.IsTrue(resultaat);
    }

    [TestMethod]
    public void AddReservering_BezetteTafel_ReturnsFalse()
    {
        // Arrange
        ReservationLogic logic = new ReservationLogic();

        int gebruikerId = logic.VoegGastToe("Test Gast", "0612345678");

        Tijdslot tijdslot = new Tijdslot(
            0,
            "2030-01-01",
            "2030-01-01 17:00:00",
            "2030-01-01 19:00:00"
        );

        logic.AddReservering(
            gebruikerId,
            2,
            tijdslot,
            1,
            "Eerste reservering"
        );

        // Act
        bool resultaat = logic.AddReservering(
            gebruikerId,
            2,
            tijdslot,
            1,
            "Tweede reservering"
        );

        // Assert
        Assert.IsFalse(resultaat);
    }

    [TestMethod]
    public void AddReservering_OngeldigAantalPersonen_ReturnsFalse()
    {
        // Arrange
        ReservationLogic logic = new ReservationLogic();

        int gebruikerId = logic.VoegGastToe("Test Gast", "0612345678");

        Tijdslot tijdslot = new Tijdslot(
            0,
            "2030-01-01",
            "2030-01-01 17:00:00",
            "2030-01-01 19:00:00"
        );

        // Act
        bool resultaat = logic.AddReservering(
            gebruikerId,
            7,
            tijdslot,
            1,
            "Ongeldig aantal personen"
        );

        // Assert
        Assert.IsFalse(resultaat);
    }

    [TestMethod]
    public void AddReservering_TafelMetVerkeerdeCapaciteit_ReturnsFalse()
    {
        // Arrange
        ReservationLogic logic = new ReservationLogic();

        int gebruikerId = logic.VoegGastToe("Test Gast", "0612345678");

        Tijdslot tijdslot = new Tijdslot(
            0,
            "2030-01-01",
            "2030-01-01 17:00:00",
            "2030-01-01 19:00:00"
        );

        int aantalPersonen = 4;
        int tafelNummer = 1;

        // Act
        bool resultaat = logic.AddReservering(
            gebruikerId,
            aantalPersonen,
            tijdslot,
            tafelNummer,
            "Verkeerde capaciteit"
        );

        // Assert
        Assert.IsFalse(resultaat);
    }

    private void SeedTestData()
    {
        var connection = DatabaseContext.Instance.Connection;

        connection.Execute(@"
            INSERT INTO OpeningsDag (DagVanWeek, IsOpen)
            VALUES
            (0, 1),
            (1, 1),
            (2, 1),
            (3, 1),
            (4, 1),
            (5, 1),
            (6, 1);
        ");

        connection.Execute(@"
            INSERT INTO OpeningsTijden (OpeningsTijd, SluitingsTijd)
            VALUES ('17:00', '23:00');
        ");

        connection.Execute(@"
            INSERT INTO Tafel (TafelNummer, Capaciteit)
            VALUES
            (1, 2),
            (2, 4),
            (3, 6);
        ");
    }

    private void ClearTestData()
    {
        var connection = DatabaseContext.Instance.Connection;

        connection.Execute("DELETE FROM Reservering;");
        connection.Execute("DELETE FROM Gebruiker;");
        connection.Execute("DELETE FROM Tafel;");
        connection.Execute("DELETE FROM OpeningsTijden;");
        connection.Execute("DELETE FROM OpeningsDag;");
    }
}