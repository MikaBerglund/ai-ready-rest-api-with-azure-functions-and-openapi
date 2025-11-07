namespace AIReadyRestApi.Models;

/// <summary>
/// Represents a product in the catalog.
/// </summary>
public class Product
{
    /// <summary>
    /// The unique identifier for the product.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The description of the product.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The price of the product.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// The category of the product.
    /// </summary>
    public string Category { get; set; } = string.Empty;
}
