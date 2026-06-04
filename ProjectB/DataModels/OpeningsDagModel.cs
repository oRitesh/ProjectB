public class OpeningsDag
{
    public int ID { get; set; }
    public int DagVanWeek { get; set; }
    public int IsOpen { get; set; }

    public OpeningsDag()
    {
    }

    public OpeningsDag(int id, int dagVanWeek, int isOpen)
    {
        ID = id;
        DagVanWeek = dagVanWeek;
        IsOpen = isOpen;
    }
}