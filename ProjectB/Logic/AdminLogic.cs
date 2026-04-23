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
    public void CheckAdminPermission(int userId)
    {
        var roleId = userAccess.GetRoleByUser(userId);
        if (roleId.Rol == 2)
        {
            adminMenuUI.ShowAdminMenu();
        }
    }
}