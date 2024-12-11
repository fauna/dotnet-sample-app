using DotNetSampleApp.Models;
using Fauna;
using Fauna.Types;
using Microsoft.AspNetCore.Mvc;

namespace DotNetSampleApp.Controllers;

/// <summary>
/// Customers controller 
/// </summary>
/// <param name="client">Fauna Client</param>
[ApiController,
 Route("/[controller]")]
public class Customers(Client client) : ControllerBase
{

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
        // Get the Customer document by `id`, using the ! operator to assert that the document exists.
        // If the document does not exist, Fauna will throw a document_not_found error.
        //
        // Use projection to only return the fields you need.
        var query = Query.FQL($"""
                               let customer: Any = Customer.byId({customerId})!
                               {QuerySnippets.CustomerResponse()}
                               """);
        
        // Connect to fauna using the client. The query method accepts an FQL query
        // as a parameter and a generic type parameter representing the return type.
        var res = await client.QueryAsync<Customer>(query);
        return Ok(res.Data);
    }

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
        // Create a new Customer document with the provided fields.
        var query = Query.FQL($"""
                               let customer: Any = Customer.create({customer})
                               {QuerySnippets.CustomerResponse()}
                               """);
        
        // Connect to fauna using the client. The query method accepts an FQL query
        // as a parameter and a generic type parameter representing the return type.
        var res = await client.QueryAsync<Customer>(query);
        return CreatedAtAction(nameof(GetCustomer), new { customerId = res.Data.Id }, res.Data);
    }
    
    /// <summary>
    /// Update Customer Details
    /// </summary>
    /// <returns>Customer Details</returns>
    [ProducesResponseType(typeof(Customer), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("{customerId}")]
    public async Task<IActionResult> UpdateCustomer(
        [FromRoute] string customerId,
        CustomerRequest customer)
    {
        // Get the Customer document by `customerId`, using the ! operator to assert that the document exists.
        // If the document does not exist, Fauna will throw a document_not_found error.
        //
        // All unannotated fields and fields annotated with @FaunaField will be serialized, including
        // those with `null` values. When an update is made to a field with a null value, that value of
        // that field is unset on the document. Partial updates must be made with a dedicated class,
        // anonymous class, or Map.
        //
        // Use projection to only return the fields you need.
        var query = Query.FQL($"""
                               let customer: Any =  Customer.byId({customerId})!.update({customer})
                               {QuerySnippets.CustomerResponse()}
                               """);
        
        // Connect to fauna using the client. The query method accepts an FQL query
        // as a parameter and a generic type parameter representing the return type.
        var res = await client.QueryAsync<Customer>(query);
        return Ok(res.Data);
    }

    /// <summary>
    /// Return all orders for a specific customer.
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="afterToken">The after token for pagination.</param>
    /// <param name="pageSize">A page size. Ignored if after token is provided.</param>
    /// <returns>List of Orders.</returns>
    [ProducesResponseType(typeof(Page<Order>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{customerId}/orders")]
    public async Task<IActionResult> GetOrdersByCustomer(
        [FromRoute] string customerId,
        [FromQuery] string? afterToken,
        [FromQuery] int pageSize = 10)
    {
        // The `afterToken` parameter contains a Fauna `after` cursor.
        // `after` cursors may contain special characters, such as `.` or `+`).
        // Make sure to URL encode the `afterToken` value to preserve these
        // characters in URLs.
        var query = !string.IsNullOrEmpty(afterToken)
        
            // Paginate with the after token if it's present.
            ? Query.FQL($"Set.paginate({afterToken})")
            
            // Define an FQL query to retrieve a page of orders for a given customer.
            // Get the Customer document by id, using the ! operator to assert that the document exists.
            // If the document does not exist, Fauna will throw a document_not_found error. We then
            // use the Order.byCustomer index to retrieve all orders for that customer and map over
            // the results to return only the fields we care about.
            : Query.FQL($$"""
                let customer: Any = Customer.byId({{customerId}})!
                Order.byCustomer(customer).pageSize({{pageSize}}).map((order) => {
                  let order: Any = order

                  // Return the order.
                  {{QuerySnippets.OrderResponse()}}
                })
                """);
        
        // Connect to fauna using the client. The query method accepts an FQL query
        // as a parameter and a generic type parameter representing the return type.
        var result = await client.QueryAsync<Page<Order>>(query);
        return Ok(result.Data);
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
        // Call our getOrCreateCart UDF to get the customer's cart. The function
        // definition can be found 'server/schema/functions.fsl'.
        var query = Query.FQL($"""
                                let order: Any = getOrCreateCart({customerId})
                                
                                // Return the cart.
                                {QuerySnippets.OrderResponse()}
                                """);
        
        // Connect to fauna using the client. The query method accepts an FQL query
        // as a parameter and a generic type parameter representing the return type.
        var res = await client.QueryAsync<Order>(query);
        return CreatedAtAction(nameof(GetCart), new { customerId }, res.Data);
    }

    /// <summary>
    /// Adds an item to the customer's cart.
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="item">Item details</param>
    /// <returns>Cart Details</returns>
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("{customerId}/cart/item")]
    public async Task<IActionResult> AddItemToCart([FromRoute] string customerId, AddItemToCartRequest item)
    {
        // Call our createOrUpdateCartItem UDF to add an item to the customer's cart. The function
        // definition can be found 'server/schema/functions.fsl'.
        var query = Query.FQL($"""
                let req = {item}
                let order: Any = createOrUpdateCartItem({customerId}, req.productName, req.quantity)

                // Return the cart as an OrderResponse object.
                {QuerySnippets.OrderResponse()}
                """);
        
        // Connect to fauna using the client. The query method accepts an FQL query
        // as a parameter and a generic type parameter representing the return type.
        var res = await client.QueryAsync<Order>(query);
        return Ok(res.Data);
    }

    /// <summary>
    /// Returns the contents of the customer's cart.
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Cart Details</returns>
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{customerId}/cart")]
    public async Task<IActionResult> GetCart([FromRoute] string customerId)
    {
        // Get the customer's cart by id, using the ! operator to assert that the document exists.
        // If the document does not exist, Fauna will throw a document_not_found error.
        var query = Query.FQL($"""
                                let order: Any = Customer.byId({customerId})!.cart
                                
                                // Return the cart as an OrderResponse object.
                                {QuerySnippets.OrderResponse()}
                                """);
        
        // Connect to fauna using the client. The query method accepts an FQL query
        // as a parameter and a generic type parameter representing the return type.
        var res = await client.QueryAsync<Order>(query);
        return Ok(res.Data);
    }
}
