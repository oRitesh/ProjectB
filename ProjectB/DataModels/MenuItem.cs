public class MenuItem
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Allergens { get; set; }

    public MenuItem(string name, string description, decimal price, string allergens)
    {
        Name = name;
        Description = description;
        Price = price;
        Allergens = allergens;
    }
}
