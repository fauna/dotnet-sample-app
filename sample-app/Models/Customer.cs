using Fauna.Mapping;

namespace DotNetSampleApp.Models;

/// <summary>
/// Customer Address
/// </summary>
public class Address
{
    /// <summary>
    /// Street
    /// </summary>
    [Field]
    public required string Street { get; set; }

    /// <summary>
    /// City
    /// </summary>
    [Field]
    public required string City { get; set; }

    /// <summary>
    /// State
    /// </summary>
    [Field]
    public required string State { get; set; }

    /// <summary>
    /// Postal Code
    /// </summary>
    [Field]
    public required string PostalCode { get; set; }

    /// <summary>
    /// Country
    /// </summary>
    [Field]
    public required string Country { get; set; }
}

/// <summary>
/// Customer
/// </summary>
public class Customer
{
    /// <summary>
    /// Document ID 
    /// </summary>
    [Id]
    public string? Id { get; init; }

    /// <summary>
    /// Name
    /// </summary>
    [Field]
    public required string Name { get; init; }

    /// <summary>
    /// Email
    /// </summary>
    [Field]
    public required string Email { get; init; }

    /// <summary>
    /// Address
    /// </summary>
    [Field]
    public required Address Address { get; init; }
}

/// <summary>
/// Request for creating a new Customer
/// </summary>
public class CustomerRequest
{
    /// <summary>
    /// Name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Email
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Address
    /// </summary>
    public required Address Address { get; set; }
}