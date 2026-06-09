public class UserLogic
{
    private readonly UserAccess userAccess;
    private readonly UserValidationLogic validationLogic;

    public UserLogic()
    {
        userAccess = new UserAccess();
        validationLogic = new UserValidationLogic(userAccess);
    }

    public Gebruiker? Login(string email, string wachtwoord) => userAccess.GetUserByEmail(email, wachtwoord);

    public int AddUser(Gebruiker gebruiker) => userAccess.AddUser(gebruiker);

    public Gebruiker? ChangeRole(string telefoon, string naam, int rol, string email, string wachtwoord)
        => userAccess.ChangeRole(telefoon, naam, rol, email, wachtwoord);

    public bool IsEmailUnique(string email) => validationLogic.IsEmailUnique(email);

    public bool IsPhoneNumberAvailable(string telefoon) => validationLogic.IsPhoneNumberAvailable(telefoon);

    public bool IsPhoneNumberForGuest(string telefoon) => validationLogic.IsPhoneNumberForGuest(telefoon);

    public bool IsGeldigTelefoonnummer(string telefoon) => UserValidationLogic.IsGeldigTelefoonnummer(telefoon);

    public bool IsGeldigWachtwoord(string wachtwoord) => UserValidationLogic.IsGeldigWachtwoord(wachtwoord);

    public bool IsGeldigEmail(string email) => UserValidationLogic.IsGeldigEmail(email);
}
