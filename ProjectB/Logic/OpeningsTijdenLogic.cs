public class OpeningsTijdenLogic
{
    private readonly OpeningsTijdenAccess openingsTijdenAccess;

    public OpeningsTijdenLogic()
    {
        openingsTijdenAccess = new OpeningsTijdenAccess();
    }

    public OpeningsTijden? GetOpeningsTijden() => openingsTijdenAccess.GetOpeningsTijden();

    public void UpdateOpeningsTijden(OpeningsTijden tijden) => openingsTijdenAccess.UpdateOpeningsTijden(tijden);

    public bool ZijnGeldigeTijden(string opening, string sluiting)
    {
        return TimeSpan.TryParse(opening, out _) && TimeSpan.TryParse(sluiting, out _);
    }
}
