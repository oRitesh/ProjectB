public class AdminMenuUI
{
    private readonly AdminMenuLogic adminMenuLogic;
    private readonly MenuItemAccess menuItemAccess;

    public AdminMenuUI(AdminMenuLogic adminMenuLogic)
    {
        this.adminMenuLogic = adminMenuLogic;
        this.menuItemAccess = new MenuItemAccess(new DatabaseContext());
    }

    public void ShowAdminMenu()
    {
        Console.WriteLine("Admin Menu:");
        Console.WriteLine("1. Wijzig menukaart");
        Console.WriteLine("2. Bekijk Reserveringen");
        Console.WriteLine("3. Uitloggen");

        Console.Write("Maak een keuze: ");
        string input = Console.ReadLine();

        switch (input)
        {
            case "1":
                EditMenu();
                break;
            case "2":
                ViewReservations();
                break;
            case "3":
                break;
            default:
                Console.WriteLine("Ongeldige keuze. Druk op een toets om verder te gaan...");
                Console.ReadKey(true);
                break;
        }
    }

    public void EditMenu()
    {
        Console.WriteLine("Wijzig menukaart:");
        Console.WriteLine("1. Voeg menu item toe");
        Console.WriteLine("2. Werk menu item bij");
        Console.WriteLine("3. Verwijder menu item");
        Console.WriteLine("4. Terug naar Admin Menu");

        Console.Write("Maak een keuze: ");
        string input = Console.ReadLine();

        switch (input)
        {
            case "1":
                Console.WriteLine("Naam:");
                string naam = Console.ReadLine();
                Console.Clear();
                Console.WriteLine("Prijs:");
                decimal prijs = decimal.Parse(Console.ReadLine());
                Console.Clear();
                Console.WriteLine("MenuCategorieID:");
                int menuCategorieID = int.Parse(Console.ReadLine());
                Console.Clear();
                menuItemAccess.AddMenuItem(new MenuItem { Naam = naam, Prijs = prijs, MenuCatogorieID = menuCategorieID });
                Console.WriteLine("Item toegevoegd.");
                Console.WriteLine($"Naam: {naam}\nPrijs: {prijs}\nMenuCategorieID: {menuCategorieID}");
                Console.WriteLine("Druk op een toets om verder te gaan...");
                Console.ReadKey(true);
                break;
            case "2":
                Console.WriteLine("ID:");
                int id = int.Parse(Console.ReadLine());
                Console.Clear();
                Console.WriteLine("Naam:");
                string updateNaam = Console.ReadLine();
                Console.Clear();
                Console.WriteLine("Prijs:");
                decimal updatePrijs = decimal.Parse(Console.ReadLine());
                Console.Clear();
                Console.WriteLine("MenuCategorieID:");
                int updateMenuCategorieID = int.Parse(Console.ReadLine());
                Console.Clear();
                menuItemAccess.UpdateMenuItem(new MenuItem { ID = id, Naam = updateNaam, Prijs = updatePrijs, MenuCatogorieID = updateMenuCategorieID });
                Console.WriteLine("Item bijgewerkt.");
                Console.WriteLine($"ID: {id}\nNaam: {updateNaam}\nPrijs: {updatePrijs}\nMenuCategorieID: {updateMenuCategorieID}");
                Console.WriteLine("Druk op een toets om verder te gaan...");
                Console.ReadKey(true);
                break;
            case "3":
                Console.WriteLine("ID:");
                int deleteId = int.Parse(Console.ReadLine());
                Console.Clear();
                menuItemAccess.DeleteMenuItem(deleteId);
                Console.WriteLine("Item verwijderd.");
                Console.WriteLine($"ID: {deleteId}");
                Console.WriteLine("Druk op een toets om verder te gaan...");
                Console.ReadKey(true);
                break;
            case "4":
                ShowAdminMenu();
                break;
            default:
                Console.WriteLine("Ongeldige keuze. Druk op een toets om verder te gaan...");
                Console.ReadKey(true);
                break;
        }
    }

    public void ViewReservations()
    {
        Console.WriteLine("Bekijken van reserveringen...");
        // Call method to view reservations
    }
}