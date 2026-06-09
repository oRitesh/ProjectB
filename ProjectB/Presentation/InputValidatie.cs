using System;
using System.Linq;

public static class InputValidatie
{
    public static string? ValideerInput(string label, Func<string, bool> validator, string foutmelding)
    {
        while (true)
        {
            string? input;

            if (!label.Contains("Wachtwoord"))
                input = LeesInvoer.LeesInvoerMetEscape($"{label}: ");
            else
                input = RandomLogicTools.ReadPassword($"{label}: ");

            if (input == null) return null;

            if (!string.IsNullOrWhiteSpace(input) && validator(input))
                return input;

            Console.WriteLine();
            Console.WriteLine($"❌ {foutmelding}");
            Console.WriteLine("Druk op een toets om opnieuw te proberen...");
            Console.ReadKey(true);
            Console.Clear();
        }
    }
}
