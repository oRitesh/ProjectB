using Microsoft.Data.Sqlite;
using System.IO;

public class DatabaseContext
{
    private static DatabaseContext? _instance;

    public static DatabaseContext Instance
    {
        get
        {
            _instance ??= new DatabaseContext(); // als _instance null is, maakt hij een nieuw object aan en slaat het op.
            return _instance;
        }
    }

    public SqliteConnection Connection { get; }

    private DatabaseContext()
    {
        // Bereken het pad naar de database file
        // Dit werkt zowel wanneer de app normaal draait als wanneer tests draaien


        string baseDirectory = AppContext.BaseDirectory; // nu zit je in ProjectB/ProjectB/bin/Debug/net10.0
        // hier staat namelijk de pad nadat de project is gebuild bij runtime

        // Navigeer naar het project root directory en vervolgens naar DataSource
        string projectRoot = Path.Combine(baseDirectory, "..", "..", ".."); // nu zit je in ProjectB/ProjectB
        string dbPath = Path.Combine(projectRoot, "DataSource", "restaurant.db"); // nu heb je de database path

        // Normaliseer het pad (verwijder ..)
        dbPath = Path.GetFullPath(dbPath); // hiervoor was het ProjectB/ProjectB/bin/Debug/net10.0/../../../DataSource/restaurant.db
        // nu is het simpeler geworden, nu is het projectB/ProjectB/DataSource/restaurant.db

        // Zorg ervoor dat het directory bestaat
        string dbDirectory = Path.GetDirectoryName(dbPath); // dit hoort te zijn /project/DataSource/
        if (!Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        } // dit is een check zodat het programma niet crasht

        Connection = new SqliteConnection($"Data Source={dbPath}");
        Connection.Open();
    }

    public void Close()
    {
        Connection.Close();
    }
}