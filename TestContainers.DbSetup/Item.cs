namespace TestContainers.DbSetup;

internal class Item
{
    public int ItemId { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Stock { get; set; }
}
