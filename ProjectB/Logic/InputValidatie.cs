using System;
using System.Linq;

public static class InputValidatie
{
    public static string ValideerInput(string label, Func<string, bool> validator, string foutmelding)
    {
        while (true)
        {
            if (label is not "Wachtwoord" || label is not "Wachtwoord (min. 8 tekens, 1 hoofdletter, 1 kleine letter)")
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
            else
            {
                Console.Write($"{label}: ");
                string? input = RandomLogicTools.ReadPassword();

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
}
