using Fauna.Mapping;

namespace dotnet_sample_app.Models;

/// <summary>
/// Order Request
/// </summary>
public class OrderRequest
{
    /// <summary>
    /// Order Status
    /// </summary>
    [Field] public string Status { get; set; } = "pending";

    /// <summary>
    /// Payment details
    /// </summary>
    [Field]
    public Dictionary<string, string> Payment { get; set; } = new();
}

/// <summary>
/// Request to add an item to the cart
/// </summary>
public class AddItemToCartRequest
{
    /// <summary>
    /// Product Name
    /// </summary>
    [Field] public required string ProductName { get; set; }

    /// <summary>
    /// Quantity
    /// </summary>
    [Field] public int Quantity { get; set; }
}

/// <summary>
/// Item Details
/// </summary>
public class Item
{
    /// <summary>
    /// Product
    /// </summary>
    [Field] public required Product Product { get; set; }

    /// <summary>
    /// Quantity
    /// </summary>
    [Field] public int Quantity { get; set; }
}

/// <summary>
/// Order Details
/// </summary>
public class Order
{
    /// <summary>
    /// Document ID
    /// </summary>
    [Id] public string? Id { get; set; }

    /// <summary>
    /// Created At
    /// </summary>
    [Ts] public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Customer Details
    /// </summary>
    [Field] public required Customer Customer { get; set; }

    /// <summary>
    /// Items Ordered
    /// </summary>
    [Field] public required List<Item> Items { get; set; }

    /// <summary>
    /// Order status
    /// </summary>
    [Field] public required string Status { get; set; }

    /// <summary>
    /// Order total 
    /// </summary>
    [Field] public int Total { get; set; }

    /// <summary>
    /// Payment details
    /// </summary>
    [Field] public Dictionary<string, string>? Payment { get; set; }
}

/// <summary>
/// Cart Details
/// </summary>
public class Cart
{
    /// <summary>
    /// Document ID
    /// </summary>
    [Id] public string? Id { get; set; }

    /// <summary>
    /// Items in the Cart
    /// </summary>
    [Field] public List<Item> Items { get; set; } = [];
}

