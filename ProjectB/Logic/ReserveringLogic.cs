using System;
using System.Collections.Generic;

public class ReservationLogic
{
    private readonly ReserveringAccess reserveringAccess;
    private readonly TafelAccess tafelAccess;
    private readonly TijdslotAccess tijdslotAccess;

    public ReservationLogic(ReserveringAccess reserveringAccess, TafelAccess tafelAccess, TijdslotAccess tijdslotAccess)
    {
        this.reserveringAccess = reserveringAccess;
        this.tafelAccess = tafelAccess;
        this.tijdslotAccess = tijdslotAccess;
    }

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

    public List<Tijdslot> GetBeschikbareTijdsloten(int aantalPersonen, DateTime datum)
    {
        List<Tijdslot> beschikbareTijdsloten = new List<Tijdslot>();

        if (!IsGeldigAantalPersonen(aantalPersonen) || !IsGeldigeDatum(datum))
        {
            return beschikbareTijdsloten;
        }

        MaakTijdslotenVoorDatumAlsNietBestaan(datum);

        List<Tafel> mogelijkeTafels = tafelAccess.GetTafelsByMinimaleCapaciteit(aantalPersonen);
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

    public bool AddReservering(int gebruikerID, int aantalPersonen, Tijdslot tijdslot, string opmerking)
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

        List<Tafel> mogelijkeTafels = tafelAccess.GetTafelsByMinimaleCapaciteit(aantalPersonen);

        Tafel? beschikbareTafel = null;

        foreach (Tafel tafel in mogelijkeTafels)
        {
            List<Reservering> overlappendeReserveringen = reserveringAccess.GetOverlappendeReserveringen(
                tafel.ID,
                tijdslot.StartTijd,
                tijdslot.EindTijd
            );

            if (overlappendeReserveringen.Count == 0)
            {
                beschikbareTafel = tafel;
                break;
            }
        }

        if (beschikbareTafel == null)
        {
            return false;
        }

        Reservering reservering = new Reservering(
            0,
            gebruikerID,
            beschikbareTafel.ID,
            tijdslot.StartTijd,
            tijdslot.EindTijd,
            aantalPersonen,
            opmerking,
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        );

        reserveringAccess.AddReservering(reservering);
        return true;
    }
}