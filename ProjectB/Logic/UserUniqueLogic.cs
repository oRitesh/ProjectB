public class UserUniqueLogic
{
    private readonly UserAccess userAccess;

    public UserUniqueLogic(UserAccess userAccess)
    {
        this.userAccess = userAccess;
    }

    public bool IsEmailUnique(string email)
    {
        var existingUser = userAccess.GetUserByEmail(email);
        return existingUser == null;
    }

    public bool IsPhoneNumberAvailable(string telefoonnummer)
    {
        var existingUser = userAccess.GetUserByPhoneNumber(telefoonnummer);
        return existingUser == null;
    }

    public bool IsPhoneNumberForGuest(string telefoonnummer)
    {
        var existingUser = userAccess.GetUserByPhoneNumber(telefoonnummer);
        return existingUser != null && existingUser.Rol == 0;
    }

}
