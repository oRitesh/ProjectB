public static class ArrowMenu
{
    public static T? ShowMenu<T>(
        string title,
        List<T> opties,
        Func<T, string> formatter,
        Action? extraInfo = null,
        bool showHeader = true)
    {
        int geselecteerd = 0;

        while (true)
        {
            Console.Clear();

            string lijn = "==================================";

            if (showHeader)
            {
                Console.WriteLine(lijn);
                int padding = (lijn.Length - title.Length) / 2;
                Console.WriteLine(
                    new string(' ', Math.Max(0, padding)) + title
                );

                Console.WriteLine(lijn);
                Console.WriteLine();
            }
            else if (!string.IsNullOrEmpty(title))
            {
                Console.WriteLine(title);
            }



            extraInfo?.Invoke();

            Console.WriteLine("Gebruik ↑ en ↓ om te kiezen.");
            Console.WriteLine("Druk op Enter om te bevestigen.");
            Console.WriteLine("Druk op Escape om terug te gaan.");
            Console.WriteLine();

            for (int i = 0; i < opties.Count; i++)
            {
                string prefix = i == geselecteerd ? "> " : "  ";

                Console.WriteLine(prefix + formatter(opties[i]));
            }

            ConsoleKeyInfo key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.UpArrow:

                    if (geselecteerd > 0)
                    {
                        geselecteerd--;
                    }

                    break;

                case ConsoleKey.DownArrow:

                    if (geselecteerd < opties.Count - 1)
                    {
                        geselecteerd++;
                    }

                    break;

                case ConsoleKey.Enter:

                    return opties[geselecteerd];

                case ConsoleKey.Escape:

                    return default(T);
            }
        }
    }
}