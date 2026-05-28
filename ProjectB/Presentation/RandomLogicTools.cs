public static class RandomLogicTools
{
    public static string ReadPassword()
    {
        string password = "";
        ConsoleKeyInfo key;

        while (true)
        {
            key = Console.ReadKey(true); // true = don't display the key

            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password.Substring(0, password.Length - 1); // remove last char
                Console.Write("\b \b");   // remove last *
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password += key.KeyChar;
                Console.Write("*");
            }
        }

        return password;
    }
}