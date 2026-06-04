using Dapper;

public class OpeningsTijdenAccess
{
    private readonly DatabaseContext db;
    public const string Table = "OpeningsTijden";

    public OpeningsTijdenAccess(DatabaseContext db)
    {
        this.db = db;
    }

    public OpeningsTijden? GetOpeningsTijden()
    {
        string sql = $@"
            SELECT * FROM {Table}
            ORDER BY ID
            LIMIT 1;";

        return db.Connection.QueryFirstOrDefault<OpeningsTijden>(sql);
    }

    public void UpdateOpeningsTijden(OpeningsTijden openingsTijden)
    {
        string sql = $@"
            UPDATE {Table}
            SET OpeningsTijd = @OpeningsTijd,
                SluitingsTijd = @SluitingsTijd
            WHERE ID = @ID;";

        db.Connection.Execute(sql, openingsTijden);
    }
}