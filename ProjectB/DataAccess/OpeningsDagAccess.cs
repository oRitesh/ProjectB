using Dapper;

public class OpeningsDagAccess
{
    private readonly DatabaseContext db;
    public const string Table = "OpeningsDag";

    public OpeningsDagAccess()
    {
        this.db = DatabaseContext.Instance;
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

    public bool IsOpenOpDatum(DateTime datum)
    {
        int dagVanWeek = (int)datum.DayOfWeek;
        OpeningsDag? dag = GetOpeningsDagByDagVanWeek(dagVanWeek);

        return dag != null && dag.IsOpen == 1;
    }

    public void UpdateOpeningsDag(OpeningsDag dag)
    {
        string sql = $@"
            UPDATE {Table}
            SET IsOpen = @IsOpen
            WHERE ID = @ID;";

        db.Connection.Execute(sql, dag);
    }
}