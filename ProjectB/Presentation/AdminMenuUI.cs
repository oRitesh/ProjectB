public class AdminMenuUI
{
    private readonly AdminMenuLogic adminMenuLogic;

    public AdminMenuUI(AdminMenuLogic adminMenuLogic)
    {
        this.adminMenuLogic = adminMenuLogic;
    }

    public void ShowAdminMenu()
    {
        Console.WriteLine("Admin Menu:");
        Console.WriteLine("1. Edit menu");
        Console.WriteLine("2. View Reservations");
        Console.WriteLine("3. Logout");

        Console.Write("Please select an option: ");
        string input = Console.ReadLine();

        switch (input)
        {
            case "1":
                adminMenuLogic.EditMenu();
                break;
            case "2":
                adminMenuLogic.ViewReservations();
                break;
            case "3":
                Console.WriteLine("Logging out...");
                break;
            default:
                Console.WriteLine("Ongeldige keuze. Druk op een toets om verder te gaan...");
                Console.ReadKey(true);
                break;
        }
    }
}