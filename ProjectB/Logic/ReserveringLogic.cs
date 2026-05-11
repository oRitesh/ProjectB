public class ReservationLogic
{
    private readonly ReserveringAccess reserveringAccess;
    private readonly TafelAccess tafelAccess;
    private readonly TijdslotAccess tijdslotAccess;
    private readonly UserAccess userAccess;

    public ReservationLogic(
        ReserveringAccess reserveringAccess,
        TafelAccess tafelAccess,
        TijdslotAccess tijdslotAccess,
        UserAccess userAccess)
    {
        this.reserveringAccess = reserveringAccess;
        this.tafelAccess = tafelAccess;
        this.tijdslotAccess = tijdslotAccess;
        this.userAccess = userAccess;
    }

    //publieke getters voor access van andere classes
    public ReserveringAccess ReserveringAccess => reserveringAccess;
    public TafelAccess TafelAccess => tafelAccess;
    public TijdslotAccess TijdslotAccess => tijdslotAccess;
    public UserAccess UserAccess => userAccess;



    public List<int> GetAantalPersonenOpties()
    {
        List<int> opties = new List<int>();

        for (int i = 1; i <= 6; i++)
        {
            opties.Add(i);
        }

        return opties;
    }

    public List<DateTime> GetBeschikbareDatums()
    {
        List<DateTime> datums = new List<DateTime>();

        DateTime vandaag = DateTime.Today;
        DateTime eindDatum = vandaag.AddMonths(1);

        for (DateTime datum = vandaag; datum <= eindDatum; datum = datum.AddDays(1))
        {
            datums.Add(datum);
        }

        return datums;
    }

    public bool IsGeldigAantalPersonen(int aantalPersonen)
    {
        return aantalPersonen >= 1 && aantalPersonen <= 6;
    }

    public bool IsGeldigeDatum(DateTime datum)
    {
        DateTime vandaag = DateTime.Today;
        DateTime eindDatum = vandaag.AddMonths(1);

        return datum.Date >= vandaag && datum.Date <= eindDatum.Date;
    }

    public void MaakTijdslotenVoorDatumAlsNietBestaan(DateTime datum)
    {
        string datumString = datum.ToString("yyyy-MM-dd");
        List<Tijdslot> bestaandeTijdsloten = tijdslotAccess.GetTijdslotenByDatum(datumString);

        if (bestaandeTijdsloten.Count > 0)
        {
            return;
        }

        DateTime start = datum.Date.AddHours(17);
        DateTime laatsteStart = datum.Date.AddHours(22);

        while (start <= laatsteStart)
        {
            DateTime eind = start.AddHours(2);

            Tijdslot tijdslot = new Tijdslot(
                0,
                datumString,
                start.ToString("yyyy-MM-dd HH:mm:ss"),
                eind.ToString("yyyy-MM-dd HH:mm:ss")
            );

            tijdslotAccess.AddTijdslot(tijdslot);
            start = start.AddMinutes(15);
        }
    }

    public int GetBenodigdeCapaciteit(int aantalPersonen)
    {
        if (aantalPersonen >= 1 && aantalPersonen <= 2)
        {
            return 2;
        }

        if (aantalPersonen >= 3 && aantalPersonen <= 4)
        {
            return 4;
        }

        if (aantalPersonen >= 5 && aantalPersonen <= 6)
        {
            return 6;
        }

        throw new ArgumentException("Ongeldig aantal personen.");
    }

    public List<Tijdslot> GetBeschikbareTijdsloten(int aantalPersonen, DateTime datum)
    {
        List<Tijdslot> beschikbareTijdsloten = new List<Tijdslot>();

        if (!IsGeldigAantalPersonen(aantalPersonen) || !IsGeldigeDatum(datum))
        {
            return beschikbareTijdsloten;
        }

        MaakTijdslotenVoorDatumAlsNietBestaan(datum);

        int benodigdeCapaciteit = GetBenodigdeCapaciteit(aantalPersonen);
        List<Tafel> mogelijkeTafels = tafelAccess.GetTafelsByCapaciteit(benodigdeCapaciteit);
        List<Tijdslot> tijdsloten = tijdslotAccess.GetTijdslotenByDatum(datum.ToString("yyyy-MM-dd"));

        foreach (Tijdslot tijdslot in tijdsloten)
        {
            bool tijdslotBeschikbaar = false;

            foreach (Tafel tafel in mogelijkeTafels)
            {
                List<Reservering> overlappendeReserveringen = reserveringAccess.GetOverlappendeReserveringen(
                    tafel.ID,
                    tijdslot.StartTijd,
                    tijdslot.EindTijd
                );

                if (overlappendeReserveringen.Count == 0)
                {
                    tijdslotBeschikbaar = true;
                    break;
                }
            }

            if (tijdslotBeschikbaar)
            {
                beschikbareTijdsloten.Add(tijdslot);
            }
        }

        return beschikbareTijdsloten;
    }

    public List<TafelWeergave> GetTafelWeergaveVoorTijdslot(int aantalPersonen, Tijdslot tijdslot)
    {
        List<TafelWeergave> overzicht = new List<TafelWeergave>();
        int benodigdeCapaciteit = GetBenodigdeCapaciteit(aantalPersonen);
        List<Tafel> alleTafels = tafelAccess.GetAllTafels();

        foreach (Tafel tafel in alleTafels.OrderBy(t => t.TafelNummer))
        {
            List<Reservering> overlappendeReserveringen = reserveringAccess.GetOverlappendeReserveringen(
                tafel.ID,
                tijdslot.StartTijd,
                tijdslot.EindTijd
            );

            bool isBeschikbaar = overlappendeReserveringen.Count == 0;
            bool isToegestaan = tafel.Capaciteit == benodigdeCapaciteit;

            overzicht.Add(new TafelWeergave(
                tafel.ID,
                tafel.TafelNummer,
                tafel.Capaciteit,
                isBeschikbaar,
                isToegestaan
            ));
        }

        return overzicht;
    }

    public bool IsTafelBeschikbaarVoorKeuze(int tafelNummer, int aantalPersonen, Tijdslot tijdslot)
    {
        Tafel? tafel = tafelAccess.GetTafelByNummer(tafelNummer);

        if (tafel == null)
        {
            return false;
        }

        int benodigdeCapaciteit = GetBenodigdeCapaciteit(aantalPersonen);

        if (tafel.Capaciteit != benodigdeCapaciteit)
        {
            return false;
        }

        List<Reservering> overlappendeReserveringen = reserveringAccess.GetOverlappendeReserveringen(
            tafel.ID,
            tijdslot.StartTijd,
            tijdslot.EindTijd
        );

        return overlappendeReserveringen.Count == 0;
    }

    public List<int> GetBeschikbareTafelNummers(int aantalPersonen, Tijdslot tijdslot)
    {
        return GetTafelWeergaveVoorTijdslot(aantalPersonen, tijdslot)
            .Where(t => t.IsBeschikbaar && t.IsToegestaan)
            .Select(t => t.TafelNummer)
            .OrderBy(n => n)
            .ToList();
    }

    public bool AddReservering(int gebruikerID, int aantalPersonen, Tijdslot tijdslot, int tafelNummer, string opmerking)
    {
        if (!IsGeldigAantalPersonen(aantalPersonen))
        {
            return false;
        }

        DateTime datum = DateTime.Parse(tijdslot.Datum);

        if (!IsGeldigeDatum(datum))
        {
            return false;
        }

        Tafel? gekozenTafel = tafelAccess.GetTafelByNummer(tafelNummer);

        if (gekozenTafel == null)
        {
            return false;
        }

        int benodigdeCapaciteit = GetBenodigdeCapaciteit(aantalPersonen);

        if (gekozenTafel.Capaciteit != benodigdeCapaciteit)
        {
            return false;
        }

        List<Reservering> overlappendeReserveringen = reserveringAccess.GetOverlappendeReserveringen(
            gekozenTafel.ID,
            tijdslot.StartTijd,
            tijdslot.EindTijd
        );

        if (overlappendeReserveringen.Count > 0)
        {
            return false;
        }

        Reservering reservering = new Reservering(
            0,
            gebruikerID,
            gekozenTafel.ID,
            tijdslot.StartTijd,
            tijdslot.EindTijd,
            aantalPersonen,
            opmerking,
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        );

        reserveringAccess.AddReservering(reservering);
        return true;
    }

    public int VoegGastToe(string naam, string telefoonnummer)
    {
        Gebruiker NieuweGast = new Gebruiker(0, naam, telefoonnummer);
        int gastID = userAccess.AddUser(NieuweGast);
        return gastID;
    }
}