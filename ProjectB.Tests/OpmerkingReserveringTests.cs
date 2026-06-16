// namespace ProjectB.Tests;

// using Dapper;

// [TestClass]
// public sealed class OpmerkingReserveringTests
// {
//     private readonly DatabaseContext _db = new();
//     private readonly ReservationLogic _logic;
//     private readonly TijdslotAccess _tijdslotAccess;

//     public OpmerkingReserveringTests()
//     {
//         var reserveringAccess = new ReserveringAccess(_db);
//         var tafelAccess = new TafelAccess(_db);
//         _tijdslotAccess = new TijdslotAccess(_db);
//         var userAccess = new UserAccess(_db);
//         _logic = new ReservationLogic(reserveringAccess, tafelAccess, userAccess);
//     }

//     /// <summary>
//     /// Path: Happy Path H2
//     /// Input: Opmerking: leeg, Reservering: tafel 5, 20:00, Klant: Emma de Wit
//     /// Scenario: Klant laat het opmerkingsveld leeg; reservering moet toch worden geplaatst
//     /// </summary>
//     [TestMethod]
//     public void AddReservering_LegeOpmerking_ReserveringSuccesvol()
//     {
//         // opzet
//         string naam = "Emma de Wit";
//         string telefoonnummer = "0612000001";
//         int aantalPersonen = 2;
//         DateTime datum = DateTime.Today.AddDays(1);

//         // Tijdsloten bestaan niet automatisch
//         // genereer ze voor de testdatum als ze ontbreken
//         var tijdsloten = _tijdslotAccess.GetTijdslotenByDatum(datum.ToString("yyyy-MM-dd"));
//         if (tijdsloten.Count == 0)
//             foreach (var ts in _logic.MaakTijdslotenVoorDatum(datum))
//                 _tijdslotAccess.AddTijdslot(ts);

//         tijdsloten = _tijdslotAccess.GetTijdslotenByDatum(datum.ToString("yyyy-MM-dd"));

//         Tijdslot tijdslot = tijdsloten.First();

//         // Zorg dat er minstens 1 tafel met juiste capaciteit bestaat in de testdb
//         int benodigdeCapaciteit = _logic.GetBenodigdeCapaciteit(aantalPersonen);
//         if (_logic.TafelAccess.GetTafelsByCapaciteit(benodigdeCapaciteit).Count == 0)
//             _logic.TafelAccess.AddTafel(new Tafel(0, 99, benodigdeCapaciteit));

//         int tafelNummer = _logic.GetBeschikbareTafelNummers(aantalPersonen, tijdslot).First();
//         int gastID = _logic.VoegGastToe(naam, telefoonnummer);

//         // act
//         bool resultaat = _logic.AddReservering(gastID, aantalPersonen, tijdslot, tafelNummer, "");

//         // cleanup
//         var reserveringen = _logic.ReserveringAccess.GetReserveringenByGebruikerID(gastID);
//         foreach (var r in reserveringen)
//             _logic.ReserveringAccess.DeleteReservering(r.ID);
//         _db.Connection.Execute($"DELETE FROM {UserAccess.table} WHERE ID = @ID", new { ID = gastID });
//         foreach (var ts in tijdsloten)
//             _tijdslotAccess.DeleteTijdslot(ts.ID);
//         var testtafel = _logic.TafelAccess.GetTafelByNummer(99);
//         if (testtafel != null)
//             _logic.TafelAccess.DeleteTafel(testtafel.ID);

//         // assert
//         Assert.IsTrue(resultaat,
//             "AddReservering moet true retourneren als de opmerking leeg is");
//     }
// }