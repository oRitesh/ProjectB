public class Timeslots
{
    private List<(int tableId, int capacity)> tables = new List<(int, int)>()
    {
        (1, 2),
        (2, 4),
        (3, 4),
        (4, 6)
    };

    private List<(int tableId, DateTime start, DateTime end)> reservations
        = new List<(int, DateTime, DateTime)>();

    public void Run()
    {
        Console.WriteLine("Voer datum in (yyyy-MM-dd):");
        DateTime date = DateTime.Parse(Console.ReadLine());

        Console.WriteLine("Aantal personen:");
        int people = int.Parse(Console.ReadLine());

        var slots = GenerateTimeslots(date);

        Console.WriteLine("\nBeschikbare tijdsloten:");

        Dictionary<int, (DateTime start, DateTime end)> validSlots = new();
        int index = 1;

        foreach (var slot in slots)
        {
            var availableTables = GetAvailableTables(slot.start, slot.end, people);

            if (availableTables.Count > 0)
            {
                Console.WriteLine($"{index}. {slot.start:HH:mm} - {slot.end:HH:mm}");
                validSlots[index] = slot;
                index++;
            }
        }

        if (validSlots.Count == 0)
        {
            Console.WriteLine("Geen beschikbare tijdsloten.");
            return;
        }

        Console.WriteLine("\nKies een tijdslot (nummer):");
        int choice = int.Parse(Console.ReadLine());

        if (!validSlots.ContainsKey(choice))
        {
            Console.WriteLine("Ongeldige keuze.");
            return;
        }

        var chosenSlot = validSlots[choice];

        var tablesAvailable = GetAvailableTables(chosenSlot.start, chosenSlot.end, people);

        Console.WriteLine("\nBeschikbare tafels:");
        foreach (var t in tablesAvailable)
        {
            Console.WriteLine($"Tafel {t.tableId} (capaciteit {t.capacity})");
        }

        Console.WriteLine("Kies tafel ID:");
        int tableChoice = int.Parse(Console.ReadLine());

        var selectedTable = tablesAvailable.FirstOrDefault(t => t.tableId == tableChoice);

        if (selectedTable == default)
        {
            Console.WriteLine("Ongeldige tafel.");
            return;
        }

        MakeReservation(selectedTable.tableId, chosenSlot.start, chosenSlot.end);

        Console.WriteLine("\nReservering succesvol!");
        Console.WriteLine($"Tafel {selectedTable.tableId} van {chosenSlot.start:HH:mm} tot {chosenSlot.end:HH:mm}");
    }

    private List<(DateTime start, DateTime end)> GenerateTimeslots(DateTime date)
    {
        List<(DateTime, DateTime)> slots = new();

        DateTime start = date.Date.AddHours(10);
        DateTime end = date.Date.AddHours(22);

        while (start.AddHours(2) <= end)
        {
            slots.Add((start, start.AddHours(2)));
            start = start.AddMinutes(15);
        }

        return slots;
    }

    private bool IsOverlapping(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
    {
        return start1 < end2 && start2 < end1;
    }

    private List<(int tableId, int capacity)> GetAvailableTables(DateTime start, DateTime end, int people)
    {
        return tables
            .Where(t => t.capacity >= people)
            .Where(t => !reservations.Any(r =>
                r.tableId == t.tableId &&
                IsOverlapping(start, end, r.start, r.end)))
            .ToList();
    }

    private void MakeReservation(int tableId, DateTime start, DateTime end)
    {
        reservations.Add((tableId, start, end));
    }
}