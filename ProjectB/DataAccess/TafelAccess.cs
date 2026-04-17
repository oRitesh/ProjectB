using Dapper;

public class TafelAccess
{
    private readonly DatabaseContext db;
    public const string Table = "Tafel";

    public TafelAccess(DatabaseContext db)
    {
        this.db = db;
    }

    public void AddTafel(Tafel tafel)
    {
        string sql = $@"
            INSERT INTO {Table}
            (TafelNummer, Capaciteit)
            VALUES
            (@TafelNummer, @Capaciteit);";

        db.Connection.Execute(sql, tafel);
    }

    public void UpdateTafel(Tafel tafel)
    {
        string sql = $@"
            UPDATE {Table}
            SET TafelNummer = @TafelNummer,
                Capaciteit = @Capaciteit
            WHERE ID = @ID;";

        db.Connection.Execute(sql, tafel);
    }

    public void DeleteTafel(int id)
    {
        string sql = $@"DELETE FROM {Table} WHERE ID = @ID;";
        db.Connection.Execute(sql, new { ID = id });
    }

    public List<Tafel> GetAllTafels()
    {
        string sql = $@"SELECT * FROM {Table};";
        return db.Connection.Query<Tafel>(sql).ToList();
    }

    public Tafel? GetTafelByID(int id)
    {
        string sql = $@"SELECT * FROM {Table} WHERE ID = @ID;";
        return db.Connection.QueryFirstOrDefault<Tafel>(sql, new { ID = id });
    }

    public List<Tafel> GetTafelsByMinimaleCapaciteit(int aantalPersonen)
    {
        string sql = $@"
            SELECT * FROM {Table}
            WHERE Capaciteit >= @AantalPersonen
            ORDER BY Capaciteit, TafelNummer;";

        return db.Connection.Query<Tafel>(sql, new { AantalPersonen = aantalPersonen }).ToList();
    }

    public Tafel? GetTafelByNummer(int tafelNummer)
    {
        string sql = $@"SELECT * FROM {Table} WHERE TafelNummer = @TafelNummer;";
        return db.Connection.QueryFirstOrDefault<Tafel>(sql, new { TafelNummer = tafelNummer });
    }

    public List<Tafel> GetTafelsByCapaciteit(int capaciteit)
    {
        string sql = $@"
            SELECT * FROM {Table}
            WHERE Capaciteit = @Capaciteit
            ORDER BY TafelNummer;";

        return db.Connection.Query<Tafel>(sql, new { Capaciteit = capaciteit }).ToList();
    }
}