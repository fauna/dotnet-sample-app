using dotnet_sample_app.Models;
using dotnet_sample_app.Repositories;
using Fauna;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_sample_app.Controllers;

/// <summary>
/// Products Controller
/// </summary>
/// <param name="client">Fauna Client</param>
[ApiController,
 Route("/[controller]")]
public class Products(Client client) : ControllerBase
{
    private readonly ProductDb _productDb = client.DataContext<ProductDb>();
    
    /// <summary>
    /// Lists products with optional filtering by category.
    /// </summary>
    /// <param name="category">Optional category to filter products by.</param>
    /// <param name="afterToken">Optional pagination token to get the next set of products.</param>
    /// <param name="pageSize">Number of products to return per page (default is 10).</param>
    /// <returns>List of products.</returns>
    [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> ListProducts(
        [FromQuery] string? category,
        [FromQuery] string? afterToken,
        [FromQuery] int pageSize = 10
    )
    {
        return Ok(await _productDb.List(category, afterToken, pageSize));
    }

    /// <summary>
    /// List Product Categories
    /// </summary>
    /// <returns>List of Product Categories Names</returns>
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("categories")]
    public async Task<IActionResult> GetProductCategories()
    {
        return Ok(await _productDb.Categories.Select(c => c.Name).ToListAsync());
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="product">Product details.</param>
    /// <returns>Created product.</returns>
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> CreateProduct(ProductRequest product)
    {
        return Ok(await _productDb.Create(product));
    }

    /// <summary>
    /// Updates a product by its ID.
    /// </summary>
    /// <param name="id">The ID of the product to update.</param>
    /// <param name="product">The product details.</param>
    /// <returns>Updated product</returns>
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateProductById(
        [FromRoute] string id,
        ProductRequest product)
    {
        return Ok(await _productDb.Update(id, product));
    }

    /// <summary>
    /// Searches products by price range and sorts by price.
    /// </summary>
    /// <param name="afterToken">Optional pagination token.</param>
    /// <param name="pageSize">Number of products to return per page (default is 10).</param>
    /// <param name="minPrice">Minimum price for filtering products (default is 0).</param>
    /// <param name="maxPrice">Maximum price for filtering products (default is 1000).</param>
    /// <returns>List of filtered and sorted products.</returns>
    [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] string? afterToken,
        [FromQuery] int pageSize = 10,
        [FromQuery] int minPrice = 0,
        [FromQuery] int maxPrice = 1000
    )
    {
        return Ok(await _productDb.Search(pageSize, minPrice, maxPrice, afterToken));
    }


    /// <summary>
    /// Deletes a product by its ID.
    /// </summary>
    /// <param name="id">The ID of the product to delete.</param>
    /// <returns>Confirmation of deletion.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        await _productDb.Delete(id);
        return NoContent();
    }
}