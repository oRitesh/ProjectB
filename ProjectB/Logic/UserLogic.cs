public class UserLogic
{
    private readonly UserAccess userAccess;
    private readonly UserUniqueLogic uniqueLogic;

    public UserLogic()
    {
        userAccess = new UserAccess();
        uniqueLogic = new UserUniqueLogic(userAccess);
    }

    public Gebruiker? Login(string email, string wachtwoord) => userAccess.GetUserByEmail(email, wachtwoord);

    public Gebruiker? GetGebruikerByID(int iD) => userAccess.GetUserById(iD);

    public int AddUser(Gebruiker gebruiker) => userAccess.AddUser(gebruiker);

    public Gebruiker? ChangeRole(string telefoon, string naam, int rol, string email, string wachtwoord)
        => userAccess.ChangeRole(telefoon, naam, rol, email, wachtwoord);

    public bool IsEmailUnique(string email) => uniqueLogic.IsEmailUnique(email);

    public bool IsPhoneNumberAvailable(string telefoon) => uniqueLogic.IsPhoneNumberAvailable(telefoon);

    public bool IsPhoneNumberForGuest(string telefoon) => uniqueLogic.IsPhoneNumberForGuest(telefoon);

    public bool IsGeldigTelefoonnummer(string telefoon) =>
    !string.IsNullOrEmpty(telefoon) && telefoon.All(char.IsDigit) && telefoon.Length == 10;

    public bool IsGeldigWachtwoord(string wachtwoord) =>
    wachtwoord.Length >= 8 && wachtwoord.Any(char.IsUpper) && wachtwoord.Any(char.IsLower);

    public bool IsGeldigEmail(string email) =>
    email.Contains("@") && email.Contains(".");
}
