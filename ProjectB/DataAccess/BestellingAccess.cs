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

    public void AddBestelling(Bestelling bestelling)
    {
        string sql = $@"
            INSERT INTO {Table} (GebruikerID, MenuItemID, Aantal, TafelNummer, BestelTijd)
            VALUES (@GebruikerID, @MenuItemID, @Aantal, @TafelNummer, @BestelTijd);";

        db.Connection.Execute(sql, bestelling);
    }

    public void UpdateStatus(int bestellingId, string nieuweStatus)
    {
        string sql = $@"
            UPDATE {Table}
            SET Status = @NieuweStatus
            WHERE ID = @BestellingID;";

        db.Connection.Execute(sql, new { NieuweStatus = nieuweStatus, BestellingID = bestellingId });
    }
}