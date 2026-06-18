using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;

[TestClass]
public class ReserveringOverzichtTests
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
    public void GetReserveringenByGebruikerID_GebruikerHeeftReservering_ReturnsReserveringMetTijdTafelEnAantalGasten()
    {
        // Arrange
        ReservationLogic logic = new ReservationLogic();

        int gebruikerId = logic.VoegGastToe("Test Klant", "0612345678");

        Tijdslot tijdslot = MaakTijdslotOverMeerDan24Uur();

        logic.AddReservering(
            gebruikerId,
            2,
            tijdslot,
            1,
            "Test reservering"
        );

        int verwachteTafelId = GetTafelIdByNummer(1);

        // Act
        List<Reservering> reserveringen =
            logic.GetReserveringenByGebruikerID(gebruikerId);

        // Assert
        Assert.AreEqual(1, reserveringen.Count);

        Reservering reservering = reserveringen.First();

        Assert.AreEqual(gebruikerId, reservering.GebruikerID);
        Assert.AreEqual(verwachteTafelId, reservering.TafelID);
        Assert.AreEqual(2, reservering.AantalGasten);
        Assert.AreEqual(tijdslot.StartTijd, reservering.StartTijd);
        Assert.AreEqual(tijdslot.EindTijd, reservering.EindTijd);
    }

    [TestMethod]
    public void MagReserveringNogWijzigen_StartTijdMeerDan24UurInToekomst_ReturnsTrue()
    {
        // Arrange
        ReservationLogic logic = new ReservationLogic();

        Reservering reservering = new Reservering(
            0,
            1,
            1,
            DateTime.Now.AddHours(25).ToString("yyyy-MM-dd HH:mm:ss"),
            DateTime.Now.AddHours(27).ToString("yyyy-MM-dd HH:mm:ss"),
            2,
            "Test",
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        );

        // Act
        bool resultaat = logic.MagReserveringNogWijzigen(reservering);

        // Assert
        Assert.IsTrue(resultaat);
    }

    [TestMethod]
    public void MagReserveringNogWijzigen_StartTijdMinderDan24UurInToekomst_ReturnsFalse()
    {
        // Arrange
        ReservationLogic logic = new ReservationLogic();

        Reservering reservering = new Reservering(
            0,
            1,
            1,
            DateTime.Now.AddHours(23).ToString("yyyy-MM-dd HH:mm:ss"),
            DateTime.Now.AddHours(25).ToString("yyyy-MM-dd HH:mm:ss"),
            2,
            "Test",
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        );

        // Act
        bool resultaat = logic.MagReserveringNogWijzigen(reservering);

        // Assert
        Assert.IsFalse(resultaat);
    }

    [TestMethod]
    public void UpdateReservering_GeldigeNieuweTafelGrootteTijdEnDag_UpdatesReservering()
    {
        // Arrange
        ReservationLogic logic = new ReservationLogic();

        int gebruikerId = logic.VoegGastToe("Test Klant", "0612345678");

        Tijdslot oudTijdslot = MaakTijdslotOverMeerDan24Uur();

        logic.AddReservering(
            gebruikerId,
            2,
            oudTijdslot,
            1,
            "Oude reservering"
        );

        Reservering reservering =
            logic.GetReserveringenByGebruikerID(gebruikerId).First();

        Tijdslot nieuwTijdslot = MaakNieuwTijdslotOverMeerDan24Uur();

        int nieuweTafelId = GetTafelIdByNummer(2);

        reservering.AantalGasten = 4;
        reservering.StartTijd = nieuwTijdslot.StartTijd;
        reservering.EindTijd = nieuwTijdslot.EindTijd;
        reservering.TafelID = nieuweTafelId;

        // Act
        logic.UpdateReservering(reservering);

        Reservering aangepasteReservering =
            logic.GetReserveringenByGebruikerID(gebruikerId).First();

        // Assert
        Assert.AreEqual(4, aangepasteReservering.AantalGasten);
        Assert.AreEqual(nieuweTafelId, aangepasteReservering.TafelID);
        Assert.AreEqual(nieuwTijdslot.StartTijd, aangepasteReservering.StartTijd);
        Assert.AreEqual(nieuwTijdslot.EindTijd, aangepasteReservering.EindTijd);
    }

    [TestMethod]
    public void UpdateReservering_StartTijdMinderDan24UurInToekomst_WordtNietUitgevoerdDoorControle()
    {
        // Arrange
        ReservationLogic logic = new ReservationLogic();

        Reservering reservering = new Reservering(
            1,
            1,
            1,
            DateTime.Now.AddHours(23).ToString("yyyy-MM-dd HH:mm:ss"),
            DateTime.Now.AddHours(25).ToString("yyyy-MM-dd HH:mm:ss"),
            2,
            "Test",
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        );

        // Act
        bool magWijzigen = logic.MagReserveringNogWijzigen(reservering);

        // Assert
        Assert.IsFalse(magWijzigen);
    }

    private Tijdslot MaakTijdslotOverMeerDan24Uur()
    {
        DateTime datum = DateTime.Today.AddDays(2);
        string datumString = datum.ToString("yyyy-MM-dd");

        return new Tijdslot(
            0,
            datumString,
            $"{datumString} 17:00:00",
            $"{datumString} 19:00:00"
        );
    }

    private Tijdslot MaakNieuwTijdslotOverMeerDan24Uur()
    {
        DateTime datum = DateTime.Today.AddDays(3);
        string datumString = datum.ToString("yyyy-MM-dd");

        return new Tijdslot(
            0,
            datumString,
            $"{datumString} 19:00:00",
            $"{datumString} 21:00:00"
        );
    }

    private int GetTafelIdByNummer(int tafelNummer)
    {
        var connection = DatabaseContext.Instance.Connection;

        return connection.QuerySingle<int>(
            "SELECT ID FROM Tafel WHERE TafelNummer = @TafelNummer;",
            new { TafelNummer = tafelNummer }
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

        connection.Execute("DELETE FROM Reservering;");
        connection.Execute("DELETE FROM Gebruiker;");
        connection.Execute("DELETE FROM Tafel;");
        connection.Execute("DELETE FROM OpeningsTijden;");
        connection.Execute("DELETE FROM OpeningsDag;");
    }
}