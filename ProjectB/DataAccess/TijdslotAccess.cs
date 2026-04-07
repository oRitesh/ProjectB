using System.Collections.Generic;
using System.Linq;
using Dapper;

public class TijdslotAccess
{
    private readonly DatabaseContext db;
    public const string Table = "Tijdslot";

    public TijdslotAccess(DatabaseContext db)
    {
        this.db = db;
    }

    public void AddTijdslot(Tijdslot tijdslot)
    {
        string sql = $@"
INSERT INTO {Table}
(Datum, StartTijd, EindTijd)
VALUES
(@Datum, @StartTijd, @EindTijd);";

        db.Connection.Execute(sql, tijdslot);
    }

    public void UpdateTijdslot(Tijdslot tijdslot)
    {
        string sql = $@"
UPDATE {Table}
SET Datum = @Datum,
    StartTijd = @StartTijd,
    EindTijd = @EindTijd
WHERE ID = @ID;";

        db.Connection.Execute(sql, tijdslot);
    }

    public void DeleteTijdslot(int id)
    {
        string sql = $@"DELETE FROM {Table} WHERE ID = @ID;";
        db.Connection.Execute(sql, new { ID = id });
    }

    public List<Tijdslot> GetAllTijdsloten()
    {
        string sql = $@"SELECT * FROM {Table};";
        return db.Connection.Query<Tijdslot>(sql).ToList();
    }

    public List<Tijdslot> GetTijdslotenByDatum(string datum)
    {
        string sql = $@"
SELECT * FROM {Table}
WHERE Datum = @Datum
ORDER BY StartTijd;";

        return db.Connection.Query<Tijdslot>(sql, new { Datum = datum }).ToList();
    }

    public Tijdslot? GetTijdslotByID(int id)
    {
        string sql = $@"SELECT * FROM {Table} WHERE ID = @ID;";
        return db.Connection.QueryFirstOrDefault<Tijdslot>(sql, new { ID = id });
    }
}