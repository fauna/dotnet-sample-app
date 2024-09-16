using dotnet_sample_app.Models;
using dotnet_sample_app.Repositories;
using Fauna;
using Fauna.Types;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_sample_app.Controllers;

/// <summary>
/// Customers controller 
/// </summary>
/// <param name="client">Fauna Client</param>
[ApiController,
 Route("/[controller]")]
public class Customers(Client client) : ControllerBase
{
    private readonly CustomerDb _customerDb = client.DataContext<CustomerDb>();

    /// <summary>
    /// Creates a new Customer
    /// </summary>
    /// <returns>Customer Details</returns>
    [ProducesResponseType(typeof(Customer), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> CreateCustomer(CustomerRequest customer)
    {
        return StatusCode(StatusCodes.Status201Created, await _customerDb.Create(customer));
    }


    /// <summary>
    /// Get Customer by ID
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Customer</returns>
    [ProducesResponseType(typeof(Customer), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{customerId}")]
    public async Task<IActionResult> GetCustomer([FromRoute] string customerId)
    {
        return Ok(await _customerDb.Get(customerId));
    }

    /// <summary>
    /// Update Customer Details
    /// </summary>
    /// <returns>Customer Details</returns>
    [ProducesResponseType(typeof(Customer), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPatch("{customerId}")]
    public async Task<IActionResult> UpdateCustomer(
        [FromRoute] string customerId,
        CustomerRequest customer)
    {
        return Ok(await _customerDb.Update(customerId, customer));
    }

    /// <summary>
    /// Return all orders for a specific customer.
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>List of Orders.</returns>
    [ProducesResponseType(typeof(Page<Order>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{customerId}/orders")]
    public async Task<IActionResult> GetOrdersByCustomer([FromRoute] string customerId)
    {
        return Ok(await _customerDb.GetOrdersByCustomer(customerId));
    }


    /// <summary>
    /// Creates a new cart for a specific customer.
    /// </summary>
    /// <param name="customerId">Customer ID for new Cart</param>
    /// <returns>Cart details</returns>
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("{customerId}/cart")]
    public async Task<IActionResult> CreateCart([FromRoute] string customerId)
    {
        return Ok(await _customerDb.GetOrCreateCart(customerId));
    }

    /// <summary>
    /// Adds an item to the customer's cart.
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="item">Item details</param>
    /// <returns>Cart Details</returns>
    [ProducesResponseType(typeof(Cart), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("{customerId}/cart/item")]
    public async Task<IActionResult> AddItemToCart([FromRoute] string customerId, AddItemToCartRequest item)
    {
        return Ok(await _customerDb.AddItemToCart(customerId, item));
    }

    /// <summary>
    /// Returns the contents of the customer's cart.
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Cart Details</returns>
    [ProducesResponseType(typeof(Cart), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{customerId}/cart")]
    public async Task<IActionResult> GetCart([FromRoute] string customerId)
    {
        return Ok(await _customerDb.GetOrCreateCart(customerId));
    }

    /// <summary>
    /// Delete Customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>No Content</returns>
    [ProducesResponseType(typeof(Cart), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpDelete("{customerId}")]
    public async Task<IActionResult> DeleteCustomer([FromRoute] string customerId)
    {
        await _customerDb.Delete(customerId);
        return NoContent();
    }
}