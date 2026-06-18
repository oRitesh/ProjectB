using Dapper;

[TestClass]
public class GastPlattegrondTests
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
    public void GetTafelWeergaveVoorTijdslot_VrijeEnBezetteTafels_ReturnsJuisteBeschikbaarheid()
    {
        // Arrange
        ReservationLogic logic = new ReservationLogic();

        int gebruikerId =
            logic.VoegGastToe("Test Gast", "0612345678");

        Tijdslot tijdslot = MaakTijdslot();

        logic.AddReservering(
            gebruikerId,
            2,
            tijdslot,
            1,
            "Test reservering"
        );

        // Act
        List<TafelWeergave> resultaat =
            logic.GetTafelWeergaveVoorTijdslot(2, tijdslot);

        // Assert
        TafelWeergave tafel1 =
            resultaat.First(t => t.TafelNummer == 1);

        TafelWeergave tafel2 =
            resultaat.First(t => t.TafelNummer == 2);

        TafelWeergave tafel3 =
            resultaat.First(t => t.TafelNummer == 3);

        Assert.IsFalse(tafel1.IsBeschikbaar);
        Assert.IsTrue(tafel1.IsToegestaan);

        Assert.IsTrue(tafel2.IsBeschikbaar);
        Assert.IsFalse(tafel2.IsToegestaan);

        Assert.IsTrue(tafel3.IsBeschikbaar);
        Assert.IsFalse(tafel3.IsToegestaan);
    }

    [TestMethod]
    public void GetBeschikbareTafelNummers_GeenBeschikbareTafelMetJuisteCapaciteit_ReturnsLegeLijst()
    {
        // Arrange
        ReservationLogic logic = new ReservationLogic();

        int gebruikerId =
            logic.VoegGastToe("Test Gast", "0612345678");

        Tijdslot tijdslot = MaakTijdslot();

        logic.AddReservering(
            gebruikerId,
            2,
            tijdslot,
            1,
            "Test reservering"
        );

        // Act
        List<int> resultaat =
            logic.GetBeschikbareTafelNummers(2, tijdslot);

        // Assert
        Assert.AreEqual(0, resultaat.Count);
    }

    [TestMethod]
    public void IsTafelBeschikbaarVoorKeuze_BeschikbareTafelMetJuisteCapaciteit_ReturnsTrue()
    {
        // Arrange
        ReservationLogic logic = new ReservationLogic();

        Tijdslot tijdslot = MaakTijdslot();

        // Act
        bool resultaat =
            logic.IsTafelBeschikbaarVoorKeuze(
                1,
                2,
                tijdslot
            );

        // Assert
        Assert.IsTrue(resultaat);
    }

    [TestMethod]
    public void IsTafelBeschikbaarVoorKeuze_BezetteTafel_ReturnsFalse()
    {
        // Arrange
        ReservationLogic logic = new ReservationLogic();

        int gebruikerId =
            logic.VoegGastToe("Test Gast", "0612345678");

        Tijdslot tijdslot = MaakTijdslot();

        logic.AddReservering(
            gebruikerId,
            2,
            tijdslot,
            1,
            "Test reservering"
        );

        // Act
        bool resultaat =
            logic.IsTafelBeschikbaarVoorKeuze(
                1,
                2,
                tijdslot
            );

        // Assert
        Assert.IsFalse(resultaat);
    }

    [TestMethod]
    public void IsTafelBeschikbaarVoorKeuze_TafelMetVerkeerdeCapaciteit_ReturnsFalse()
    {
        // Arrange
        ReservationLogic logic = new ReservationLogic();

        Tijdslot tijdslot = MaakTijdslot();

        // Act
        bool resultaat =
            logic.IsTafelBeschikbaarVoorKeuze(
                2,
                2,
                tijdslot
            );

        // Assert
        Assert.IsFalse(resultaat);
    }

    private Tijdslot MaakTijdslot()
    {
        DateTime datum = DateTime.Today.AddDays(1);

        string datumString =
            datum.ToString("yyyy-MM-dd");

        return new Tijdslot(
            0,
            datumString,
            $"{datumString} 17:00:00",
            $"{datumString} 19:00:00"
        );
    }

    private void SeedTestData()
    {
        var connection = DatabaseContext.Instance.Connection;

        connection.Execute(@"
            INSERT INTO OpeningsDag
            (DagVanWeek, IsOpen)
            VALUES
            (0,1),
            (1,1),
            (2,1),
            (3,1),
            (4,1),
            (5,1),
            (6,1);
        ");

        connection.Execute(@"
            INSERT INTO OpeningsTijden
            (OpeningsTijd, SluitingsTijd)
            VALUES
            ('17:00','23:00');
        ");

        connection.Execute(@"
            INSERT INTO Tafel
            (TafelNummer, Capaciteit)
            VALUES
            (1,2),
            (2,4),
            (3,6);
        ");
    }

    private void ClearTestData()
    {
        var connection = DatabaseContext.Instance.Connection;

        connection.Execute("DELETE FROM Reservering");
        connection.Execute("DELETE FROM Gebruiker");
        connection.Execute("DELETE FROM Tafel");
        connection.Execute("DELETE FROM OpeningsTijden");
        connection.Execute("DELETE FROM OpeningsDag");
    }
}