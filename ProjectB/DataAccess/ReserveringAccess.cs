using Dapper;

public class ReserveringAccess
{
    private readonly DatabaseContext db;
    public const string Table = "Reservering";

    public ReserveringAccess(DatabaseContext db)
    {
        this.db = db;
    }

    public void AddReservering(Reservering reservering)
    {
        string sql = $@"
            INSERT INTO {Table}
            (GebruikerID, TafelID, StartTijd, EindTijd, AantalGasten, Opmerking, GemaaktOp)
            VALUES
            (@GebruikerID, @TafelID, @StartTijd, @EindTijd, @AantalGasten, @Opmerking, @GemaaktOp);";

        db.Connection.Execute(sql, reservering);
    }

    public void UpdateReservering(Reservering reservering)
    {
        string sql = $@"
            UPDATE {Table}
            SET GebruikerID = @GebruikerID,
                TafelID = @TafelID,
                StartTijd = @StartTijd,
                EindTijd = @EindTijd,
                AantalGasten = @AantalGasten,
                Opmerking = @Opmerking,
                GemaaktOp = @GemaaktOp
            WHERE ID = @ID;";

        db.Connection.Execute(sql, reservering);
    }

    public void DeleteReservering(int id)
    {
        string sql = $@"DELETE FROM {Table} WHERE ID = @ID;";
        db.Connection.Execute(sql, new { ID = id });
    }

    public List<Reservering> GetAllReserveringen()
    {
        string sql = $@"SELECT * FROM {Table};";
        return db.Connection.Query<Reservering>(sql).ToList();
    }

    public Reservering? GetReserveringByID(int id)
    {
        string sql = $@"SELECT * FROM {Table} WHERE ID = @ID;";
        return db.Connection.QueryFirstOrDefault<Reservering>(sql, new { ID = id });
    }

    public List<Reservering> GetReserveringenByGebruikerID(int gebruikerID)
    {
        string sql = $@"SELECT * FROM {Table} WHERE GebruikerID = @GebruikerID;";
        return db.Connection.Query<Reservering>(sql, new { GebruikerID = gebruikerID }).ToList();
    }

    public List<Reservering> GetReserveringenByTafelID(int tafelID)
    {
        string sql = $@"SELECT * FROM {Table} WHERE TafelID = @TafelID;";
        return db.Connection.Query<Reservering>(sql, new { TafelID = tafelID }).ToList();
    }

    public List<Reservering> GetReserveringenVoorDatum(string datum)
    {
        string sql = $@"
            SELECT * FROM {Table}
            WHERE date(StartTijd) = date(@Datum);";

        return db.Connection.Query<Reservering>(sql, new { Datum = datum }).ToList();
    }

    public List<Reservering> GetOverlappendeReserveringen(int tafelID, string startTijd, string eindTijd)
    {
        string sql = $@"
            SELECT * FROM {Table}
            WHERE TafelID = @TafelID
            AND NOT (
                datetime(EindTijd) <= datetime(@StartTijd)
                OR datetime(StartTijd) >= datetime(@EindTijd)
            );";

        return db.Connection.Query<Reservering>(sql, new
        {
            TafelID = tafelID,
            StartTijd = startTijd,
            EindTijd = eindTijd
        }).ToList();
    }
}