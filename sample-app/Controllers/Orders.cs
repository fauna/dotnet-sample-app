using dotnet_sample_app.Models;
using dotnet_sample_app.Repositories;
using Fauna;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_sample_app.Controllers;

/// <summary>
/// Orders Controller
/// </summary>
/// <param name="client">Fauna Client</param>
[ApiController,
 Route("/[controller]")]
public class Orders(Client client) : ControllerBase
{


    private readonly OrderDb _orderDb = client.DataContext<OrderDb>();

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
        return Ok(await _orderDb.Get(id));
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
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateOrder(
        [FromRoute] string id,
        OrderRequest order
    )
    {
        return Ok(await _orderDb.Update(id, order));
    }

    /// <summary>
    /// Get Completed Orders
    /// </summary>
    /// <returns></returns>
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("list")]
    public async Task<IActionResult> ListOrders()
    {
        return Ok(await _orderDb.Orders.Select(o => new
        {
            id = o.Id,
            status = o.Status,
            total = o.Total
        }).ToListAsync());
    }
}