using System;
using System.Linq;

public static class InputValidatie
{
    public static string ValideerInput(string label, Func<string, bool> validator, string foutmelding)
    {
        while (true)
        {
            Console.Write($"{label}: ");
            string? input = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(input) && validator(input))
            {
                return input;
            }

            Console.WriteLine();
            Console.WriteLine($"❌ {foutmelding}");
            Console.WriteLine("Druk op een toets om opnieuw te proberen...");
            Console.ReadKey(true);
            Console.Clear();
        }
    }
}
