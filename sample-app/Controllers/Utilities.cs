using Fauna;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_sample_app.Controllers;

/// <summary>
/// Utilities Controller
/// </summary>
/// <param name="client">Fauna Client</param>
[ApiController]
public class Utilities(Client client) : ControllerBase
{
    /// <summary>
    /// Resets the database
    /// </summary>
    [HttpPost("/reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetDatabase()
    {
        await client.QueryAsync(Query.FQL($"""
                                           Order.all().forEach(order => order.delete())
                                           OrderItem.all().forEach(orderItem => orderItem.delete())
                                           """));
        return NoContent();
    }
}