using DotNetSampleApp.Models;
using Fauna;
using Microsoft.AspNetCore.Mvc;

namespace DotNetSampleApp.Controllers;

/// <summary>
/// Orders Controller
/// </summary>
/// <param name="client">Fauna Client</param>
[ApiController,
 Route("/[controller]")]
public class Orders(Client client) : ControllerBase
{

    /// <summary>
    /// Retrieves an order by its ID.
    /// </summary>
    /// <param name="id">Order ID to retrieve.</param>
    /// <returns></returns>
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder([FromRoute] string id)
    {
        var query = Query.FQL($"""
                                let order: Any = Order.byId({id})!
                                {QuerySnippets.OrderResponse()}
                                """);
        
        // Connect to fauna using the client. The query method accepts an FQL query
        // as a parameter and a generic type parameter representing the return type.
        var res = await client.QueryAsync<Order>(query);
        return StatusCode(StatusCodes.Status201Created, res.Data);
    }


    /// <summary>
    /// Updates an order.
    /// </summary>
    /// <param name="id">The ID of the order to update.</param>
    /// <param name="order">The new order details.</param>
    /// <returns>Order details</returns>
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("{id}")]
    public async Task<IActionResult> UpdateOrder(
        [FromRoute] string id,
        OrderRequest order
    )
    {
        // If the new order status is "processing" call the checkout UDF to process the checkout. The checkout
        // function definition can be found in 'server/schema/functions.fsl'. It is responsible
        // for validating that the order in a valid state to be processed and decrements the stock
        // of each product in the order. This ensures that the product stock is updated in the same transaction
        // as the order status.
        var query = order.Status == "processing" 
            ? Query.FQL($"""
                      let req = {order}
                      let order: Any = checkout({id}, req.status, req.payment)
                      {QuerySnippets.OrderResponse()}
                      """)
            
            // Define an FQL query to update the order. The query first retrieves the order by id
            // using the Order.byId function. If the order does not exist, Fauna will throw a document_not_found
            // error. We then use the validateOrderStatusTransition UDF to ensure that the order status transition
            // is valid. If the transition is not valid, the UDF will throw an abort error.
            : Query.FQL($$"""
                        let order: Any = Order.byId({{id}})!
                        let req = {{order}}

                        // Validate the order status transition if a status is provided.
                        if (req.status != null) {
                          validateOrderStatusTransition(order!.status, req.status)
                        }

                        // If the order status is not "cart" and a payment is provided, throw an error.
                        if (order!.status != "cart" && req.payment != null) {
                          abort("Can not update payment information after an order has been placed.")
                        }

                        // Update the order with the new status and payment information.
                        order.update(req)

                        // Return the order.
                        {{QuerySnippets.OrderResponse()}}
                        """);

        var res = await client.QueryAsync<Order>(query);
        return Ok(res.Data);
    }
}
