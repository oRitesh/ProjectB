public class AdminLogic
{
    private readonly UserAccess userAccess;

    public AdminLogic(DatabaseContext db)
    {
        this.userAccess = new UserAccess(db);
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