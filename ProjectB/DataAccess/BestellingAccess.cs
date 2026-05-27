using System.Collections.Generic;
using System.Linq;
using Dapper;

public class bestellingAccess
{
    private readonly DatabaseContext db;
    public const string Table = "Bestelling";

    public bestellingAccess(DatabaseContext db)
    {
        this.db = db;
    }

    public List<Bestelling> GetAllBestellingen()
    {
        string sql = $@"SELECT * FROM {Table};";
        return db.Connection.Query<Bestelling>(sql).ToList();
    }

    public int AddBestelling(Bestelling bestelling)
    {
        string sql = $@"
            INSERT INTO {Table} (GebruikerID, GemaaktOp, TotaalPrijs, OphaalTijd, Status)
            VALUES (@GebruikerID, @GemaaktOp, @TotaalPrijs, @OphaalTijd, @Status);
            SELECT last_insert_rowid();";

        return db.Connection.ExecuteScalar<int>(sql, bestelling);
    }

    public void UpdateStatus(int bestellingId, string nieuweStatus)
    {
        string sql = $@"
            UPDATE {Table}
            SET Status = @NieuweStatus
            WHERE ID = @ID;";

        db.Connection.Execute(sql, new { NieuweStatus = nieuweStatus, ID = bestellingId });
    }

    public void DeleteBestelling(int bestellingId)
    {
        string sql = $@"DELETE FROM {Table} WHERE ID = @ID;";
        db.Connection.Execute(sql, new { ID = bestellingId });
    }

}