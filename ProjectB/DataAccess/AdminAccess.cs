using Dapper;

public class AdminAccess
{
    private readonly DatabaseContext db;

    public AdminAccess()
    {
        this.db = DatabaseContext.Instance;
    }
}