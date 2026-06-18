public class AdminLogic
{
    private readonly UserAccess userAccess;
    private readonly AdminAccess adminAccess;

    public AdminLogic()
    {
        this.userAccess = new UserAccess();
        this.adminAccess = new AdminAccess();
    }

    public Gebruiker? GetUserIfAuthorized(int userId)
    {
        var user = userAccess.GetRoleByUser(userId);
        if (user == null)
        {
            return null;
        }

        if (user.Rol == 2 || user.Rol == 3)
        {
            return user;
        }

        return null;
    }

    public bool IsUserAdmin(Gebruiker user)
    {
        return user.Rol == 2 || user.Rol == 3;
    }

}