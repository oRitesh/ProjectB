public class TimeSlotLogic
{
    private readonly OpeningsTijdenAccess openingsTijdenAccess;
    private readonly OpeningsDagAccess openingsDagAccess;

    public TimeSlotLogic(DatabaseContext db)
    {
        this.openingsTijdenAccess = new OpeningsTijdenAccess(db);
        this.openingsDagAccess = new OpeningsDagAccess(db);
    }

    public List<Tijdslot> MaakTijdslotenVoorReservering(DateTime datum)
    {
        List<Tijdslot> tijdsloten = new List<Tijdslot>();

        if (!openingsDagAccess.IsOpenOpDatum(datum))
        {
            return tijdsloten;
        }

        OpeningsTijden? opening = openingsTijdenAccess.GetOpeningsTijden();

        if (opening == null)
        {
            return tijdsloten;
        }

        string datumString = datum.ToString("yyyy-MM-dd");

        DateTime start = DateTimeHelper.CombineDatumEnTijd(datum, opening.OpeningsTijd);
        DateTime sluiting = DateTimeHelper.CombineDatumEnTijd(datum, opening.SluitingsTijd);

        if (sluiting <= start)
        {
            sluiting = sluiting.AddDays(1);
        }

        DateTime laatsteStart = sluiting.AddHours(-2);

        while (start <= laatsteStart)
        {
            DateTime eind = start.AddHours(2);

            tijdsloten.Add(new Tijdslot(
                0,
                datumString,
                start.ToString("yyyy-MM-dd HH:mm:ss"),
                eind.ToString("yyyy-MM-dd HH:mm:ss")
            ));

            start = start.AddMinutes(15);
        }

        return tijdsloten;
    }

    public List<Tijdslot> MaakTijdslotenVoorAdmin(DateTime datum)
    {
        List<Tijdslot> tijdsloten = new List<Tijdslot>();

        string datumString = datum.ToString("yyyy-MM-dd");

        DateTime start = datum.Date.AddHours(17);
        DateTime laatsteStart = datum.Date.AddHours(22);

        while (start <= laatsteStart)
        {
            DateTime eind = start.AddHours(2);

            tijdsloten.Add(new Tijdslot(
                0,
                datumString,
                start.ToString("yyyy-MM-dd HH:mm:ss"),
                eind.ToString("yyyy-MM-dd HH:mm:ss")
            ));

            start = start.AddMinutes(15);
        }

        return tijdsloten;
    }
}
