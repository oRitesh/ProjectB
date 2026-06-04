using Dapper;

public class OpeningsDagAccess
{
    private readonly DatabaseContext db;
    public const string Table = "OpeningsDag";

    public OpeningsDagAccess(DatabaseContext db)
    {
        this.db = db;
    }

    public List<OpeningsDag> GetAllOpeningsDagen()
    {
        string sql = $@"
            SELECT * FROM {Table}
            ORDER BY DagVanWeek;";

        return db.Connection.Query<OpeningsDag>(sql).ToList();
    }

    public OpeningsDag? GetOpeningsDagByDagVanWeek(int dagVanWeek)
    {
        string sql = $@"
            SELECT * FROM {Table}
            WHERE DagVanWeek = @DagVanWeek;";

        return db.Connection.QueryFirstOrDefault<OpeningsDag>(
            sql,
            new { DagVanWeek = dagVanWeek }
        );
    }

    public bool IsRestaurantOpenOpDag(DateTime datum)
    {
        int dagVanWeek = (int)datum.DayOfWeek;

        OpeningsDag? openingsDag = GetOpeningsDagByDagVanWeek(dagVanWeek);

        if (openingsDag == null)
        {
            return false;
        }

        return openingsDag.IsOpen == 1;
    }

    public void UpdateOpeningsDag(OpeningsDag openingsDag)
    {
        string sql = $@"
            UPDATE {Table}
            SET IsOpen = @IsOpen
            WHERE ID = @ID;";

        db.Connection.Execute(sql, openingsDag);
    }
}