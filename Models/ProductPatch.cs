namespace AIReadyRestApi.Models;

/// <summary>
/// Represents a partial update for a product (used in PATCH operations).
/// All properties are nullable to allow partial updates.
/// </summary>
public class ProductPatch
{
    /// <summary>
    /// The name of the product (optional).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The description of the product (optional).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The price of the product (optional).
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// The category of the product (optional).
    /// </summary>
    public string? Category { get; set; }
}
