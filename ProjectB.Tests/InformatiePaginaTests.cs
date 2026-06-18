using Dapper;

[TestClass]
public class InformatiePaginaTests
{
    private OpeningsTijdenLogic openingsTijdenLogic;
    private OpeningsDagLogic openingsDagLogic;

    [TestInitialize]
    public void TestInitialize()
    {
        openingsTijdenLogic = new OpeningsTijdenLogic();
        openingsDagLogic = new OpeningsDagLogic();

        ClearTestData();
        SeedTestData();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        ClearTestData();
    }

    [TestMethod]
    public void UpdateOpeningsTijden_GeldigeNieuweTijden_UpdateWordtOpgeslagen()
    {
        // Arrange
        OpeningsTijden tijden = openingsTijdenLogic.GetOpeningsTijden()!;

        tijden.OpeningsTijd = "16:00";
        tijden.SluitingsTijd = "23:30";

        // Act
        openingsTijdenLogic.UpdateOpeningsTijden(tijden);

        OpeningsTijden resultaat = openingsTijdenLogic.GetOpeningsTijden()!;

        // Assert
        Assert.AreEqual("16:00", resultaat.OpeningsTijd);
        Assert.AreEqual("23:30", resultaat.SluitingsTijd);
    }

    [TestMethod]
    public void UpdateOpeningsDag_MaandagGesloten_UpdateWordtOpgeslagen()
    {
        // Arrange
        OpeningsDag maandag = openingsDagLogic.GetAllOpeningsDagen()
            .Find(d => d.DagVanWeek == 1)!;

        maandag.IsOpen = 0;

        // Act
        openingsDagLogic.UpdateOpeningsDag(maandag);

        OpeningsDag resultaat = openingsDagLogic.GetAllOpeningsDagen()
            .Find(d => d.DagVanWeek == 1)!;

        // Assert
        Assert.AreEqual(0, resultaat.IsOpen);
    }

    [TestMethod]
    public void UpdateOpeningsDag_MaandagOpen_UpdateWordtOpgeslagen()
    {
        // Arrange
        OpeningsDag maandag = openingsDagLogic.GetAllOpeningsDagen()
            .Find(d => d.DagVanWeek == 1)!;

        maandag.IsOpen = 1;

        // Act
        openingsDagLogic.UpdateOpeningsDag(maandag);

        OpeningsDag resultaat = openingsDagLogic.GetAllOpeningsDagen()
            .Find(d => d.DagVanWeek == 1)!;

        // Assert
        Assert.AreEqual(1, resultaat.IsOpen);
    }

    [TestMethod]
    public void GetAllOpeningsDagen_BestaandeDagen_ReturnsZevenDagen()
    {
        // Arrange

        // Act
        List<OpeningsDag> resultaat = openingsDagLogic.GetAllOpeningsDagen();

        // Assert
        Assert.AreEqual(7, resultaat.Count);
    }

    [TestMethod]
    public void GetOpeningsTijden_BestaandeOpeningstijden_ReturnsCorrecteWaarden()
    {
        // Arrange

        // Act
        OpeningsTijden resultaat = openingsTijdenLogic.GetOpeningsTijden()!;

        // Assert
        Assert.AreEqual("17:00", resultaat.OpeningsTijd);
        Assert.AreEqual("22:00", resultaat.SluitingsTijd);
    }

    [TestMethod]
    public void ZijnGeldigeTijden_GeldigeTijden_ReturnsTrue()
    {
        // Arrange
        string opening = "17:00";
        string sluiting = "22:00";

        // Act
        bool resultaat = openingsTijdenLogic.ZijnGeldigeTijden(opening, sluiting);

        // Assert
        Assert.IsTrue(resultaat);
    }

    [TestMethod]
    public void ZijnGeldigeTijden_OngeldigeOpeningstijd_ReturnsFalse()
    {
        // Arrange
        string opening = "fout";
        string sluiting = "22:00";

        // Act
        bool resultaat = openingsTijdenLogic.ZijnGeldigeTijden(opening, sluiting);

        // Assert
        Assert.IsFalse(resultaat);
    }

    [TestMethod]
    public void GetOpeningsDagenTekst_AlleDagenOpen_ReturnsElkeDag()
    {
        // Arrange
        List<OpeningsDag> dagen = new List<OpeningsDag>
        {
            new OpeningsDag(1, 0, 1),
            new OpeningsDag(2, 1, 1),
            new OpeningsDag(3, 2, 1),
            new OpeningsDag(4, 3, 1),
            new OpeningsDag(5, 4, 1),
            new OpeningsDag(6, 5, 1),
            new OpeningsDag(7, 6, 1)
        };

        // Act
        string resultaat = openingsDagLogic.GetOpeningsDagenTekst(dagen);

        // Assert
        Assert.AreEqual("Elke dag", resultaat);
    }

    [TestMethod]
    public void GetOpeningsDagenTekst_GeenDagenOpen_ReturnsGesloten()
    {
        // Arrange
        List<OpeningsDag> dagen = new List<OpeningsDag>
        {
            new OpeningsDag(1, 0, 0),
            new OpeningsDag(2, 1, 0),
            new OpeningsDag(3, 2, 0),
            new OpeningsDag(4, 3, 0),
            new OpeningsDag(5, 4, 0),
            new OpeningsDag(6, 5, 0),
            new OpeningsDag(7, 6, 0)
        };

        // Act
        string resultaat = openingsDagLogic.GetOpeningsDagenTekst(dagen);

        // Assert
        Assert.AreEqual("Gesloten", resultaat);
    }

    [TestMethod]
    public void GetOpeningsDagenTekst_EnkeleDagenOpen_ReturnsDagnamen()
    {
        // Arrange
        List<OpeningsDag> dagen = new List<OpeningsDag>
        {
            new OpeningsDag(1, 0, 0),
            new OpeningsDag(2, 1, 1),
            new OpeningsDag(3, 2, 0),
            new OpeningsDag(4, 3, 1),
            new OpeningsDag(5, 4, 0),
            new OpeningsDag(6, 5, 1),
            new OpeningsDag(7, 6, 0)
        };

        // Act
        string resultaat = openingsDagLogic.GetOpeningsDagenTekst(dagen);

        // Assert
        Assert.AreEqual("Maandag, Woensdag, Vrijdag", resultaat);
    }

    [TestMethod]
    public void GetDagNaam_DagVanWeekIsEen_ReturnsMaandag()
    {
        // Arrange
        int dagVanWeek = 1;

        // Act
        string resultaat = openingsDagLogic.GetDagNaam(dagVanWeek);

        // Assert
        Assert.AreEqual("Maandag", resultaat);
    }

    [TestMethod]
    public void GetDagNaam_DagVanWeekIsOnbekend_ReturnsOnbekend()
    {
        // Arrange
        int dagVanWeek = 99;

        // Act
        string resultaat = openingsDagLogic.GetDagNaam(dagVanWeek);

        // Assert
        Assert.AreEqual("Onbekend", resultaat);
    }

    private void SeedTestData()
    {
        var connection = DatabaseContext.Instance.Connection;

        connection.Execute(@"
            INSERT INTO OpeningsTijden
            (OpeningsTijd, SluitingsTijd)
            VALUES
            ('17:00', '22:00');
        ");

        connection.Execute(@"
            INSERT INTO OpeningsDag
            (DagVanWeek, IsOpen)
            VALUES
            (0,1),
            (1,1),
            (2,1),
            (3,1),
            (4,1),
            (5,1),
            (6,1);
        ");
    }

    private void ClearTestData()
    {
        var connection = DatabaseContext.Instance.Connection;

        connection.Execute("DELETE FROM OpeningsDag;");
        connection.Execute("DELETE FROM OpeningsTijden;");
    }
}