using Microsoft.Data.Sqlite;
using System.IO;

public class DatabaseContext
{
    public SqliteConnection Connection { get; }

    public DatabaseContext()
    {
        // Bereken het pad naar de database file
        // Dit werkt zowel wanneer de app normaal draait als wanneer tests draaien
        string baseDirectory = AppContext.BaseDirectory;

        // Navigeer naar het project root directory en vervolgens naar DataSource
        // AppContext.BaseDirectory wijst naar bin/Debug/net10.0 of soortgelijk
        string projectRoot = Path.Combine(baseDirectory, "..", "..", "..");
        string dbPath = Path.Combine(projectRoot, "DataSource", "restaurant.db");

        // Normaliseer het pad (verwijder ..)
        dbPath = Path.GetFullPath(dbPath);

        // Zorg ervoor dat het directory bestaat
        string dbDirectory = Path.GetDirectoryName(dbPath);
        if (!Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }

        Connection = new SqliteConnection($"Data Source={dbPath}");
        Connection.Open();
    }

    public void Close()
    {
        Connection.Close();
    }
}