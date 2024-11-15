using System.Diagnostics.CodeAnalysis;
using Fauna.Mapping;
using Fauna.Types;

namespace DotNetSampleApp.Models;

/// <summary>
/// Create Product request details
/// </summary>
public class ProductRequest
{
    /// <summary>
    /// Name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Description
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Price
    /// </summary>
    public int Price { get; init; } = 0;

    /// <summary>
    /// The number of items in stock
    /// </summary>
    public int Stock { get; init; } = 0;

    /// <summary>
    /// Category
    /// </summary>
    public required string Category { get; init; }
}

/// <summary>
/// Category
/// </summary>
public class Category
{
    /// <summary>
    /// Document ID
    /// </summary>
    [Id]
    [AllowNull]
    public string Id { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Field]
    public required string Name { get; init; }

    /// <summary>
    /// 
    /// </summary>
    [Field]
    public required string Description { get; init; }
}

/// <summary>
/// Product
/// </summary>
public class Product
{
    /// <summary>
    /// Document ID
    /// </summary>
    [Id]
    [AllowNull]
    public string Id { get; init; }

    /// <summary>
    /// Name
    /// </summary>
    [Field]
    public required string Name { get; init; }

    /// <summary>
    /// Description
    /// </summary>
    [Field]
    public required string Description { get; init; }

    /// <summary>
    /// Price
    /// </summary>
    [Field]
    public int Price { get; init; } = 0;

    /// <summary>
    /// Total units in stock
    /// </summary>
    [Field]
    public int Stock { get; init; } = 0;

    /// <summary>
    /// Category
    /// </summary>
    [Field]
    public required Category Category { get; init; }
}