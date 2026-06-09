public class OpeningsDagLogic
{
    private readonly OpeningsDagAccess openingsDagAccess;

    public OpeningsDagLogic(DatabaseContext db)
    {
        openingsDagAccess = new OpeningsDagAccess(db);
    }

    public List<OpeningsDag> GetAllOpeningsDagen() => openingsDagAccess.GetAllOpeningsDagen();

    public void UpdateOpeningsDag(OpeningsDag dag) => openingsDagAccess.UpdateOpeningsDag(dag);

    public bool IsOpenOpDatum(DateTime datum) => openingsDagAccess.IsOpenOpDatum(datum);

    public string GetOpeningsDagenTekst(List<OpeningsDag> dagen)
    {
        List<int> openDagen = dagen
            .Where(d => d.IsOpen == 1)
            .Select(d => d.DagVanWeek)
            .OrderBy(d => d)
            .ToList();

        if (openDagen.Count == 0) return "Gesloten";
        if (openDagen.Count == 7) return "Elke dag";

        return string.Join(", ", openDagen.Select(GetDagNaam));
    }

    public string GetDagNaam(int dagVanWeek)
    {
        return dagVanWeek switch
        {
            0 => "Zondag",
            1 => "Maandag",
            2 => "Dinsdag",
            3 => "Woensdag",
            4 => "Donderdag",
            5 => "Vrijdag",
            6 => "Zaterdag",
            _ => "Onbekend"
        };
    }
}
