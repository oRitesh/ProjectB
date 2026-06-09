public static class LeesInvoer
{
    public static string? LeesInvoerMetEscape(string prompt)
    {
        Console.Write(prompt);
        string invoer = "";

        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Escape)
            {
                Console.WriteLine();
                return null; // terug vorige scherm
            }
            else if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return invoer.Trim();
            }
            else if (key.Key == ConsoleKey.Backspace && invoer.Length > 0)
            {
                invoer = invoer.Remove(invoer.Length - 1);
                Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                Console.Write(" ");
                Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
            }
            else if (!char.IsControl(key.KeyChar))
            {
                invoer += key.KeyChar;
                Console.Write(key.KeyChar);
            }
        }
    }
}