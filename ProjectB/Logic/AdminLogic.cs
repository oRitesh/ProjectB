public class AdminLogic
{
    private readonly DatabaseContext db;
    private readonly AdminMenuUI adminMenuUI;
    private readonly UserAccess userAccess;

    public AdminLogic(DatabaseContext db)
    {
        this.db = db;
        this.adminMenuUI = new AdminMenuUI();
        this.userAccess = new UserAccess(db);
    }

    public void ShowAdminMenuIfAuthorized(int userId)
    {
        var user = userAccess.GetRoleByUser(userId);
        if (user == null)
        {
            Console.WriteLine("Gebruiker niet gevonden.");
            return;
        }

        if (user.Rol == 2 || user.Rol == 3)
        {
            adminMenuUI.ShowAdminMenu(user.Rol);
        }
        else
        {
            Console.WriteLine("Geen toegang. Admin-rechten vereist.");
            Console.WriteLine("Druk op een toets om door te gaan...");
            Console.ReadKey(true);
        }
    }
}