using Microsoft.Data.Sqlite;
using System.IO;

public class DatabaseContext
{
    public SqliteConnection Connection { get; }

    public DatabaseContext()
    {
        string dbPath = Path.Combine("DataSource", "restaurant.db");
        Connection = new SqliteConnection($"Data Source={dbPath}");
        Connection.Open();
    }

    public void Close()
    {
        Connection.Close();
    }
}