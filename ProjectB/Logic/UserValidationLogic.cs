public class UserValidationLogic
{
    private readonly UserAccess userAccess;

    public UserValidationLogic(UserAccess userAccess)
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

    public static bool IsGeldigTelefoonnummer(string telefoon)
    {
        return !string.IsNullOrEmpty(telefoon) && telefoon.All(char.IsDigit) && telefoon.Length == 10;
    }

    public static bool IsGeldigWachtwoord(string wachtwoord)
    {
        return wachtwoord.Length >= 8 && wachtwoord.Any(char.IsUpper) && wachtwoord.Any(char.IsLower);
    }

    public static bool IsGeldigEmail(string email)
    {
        return email.Contains("@") && email.Contains(".");
    }
}
