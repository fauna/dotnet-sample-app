using DotNetSampleApp.Models;
using Fauna;
using Fauna.Types;
using Microsoft.AspNetCore.Mvc;

namespace DotNetSampleApp.Controllers;

/// <summary>
/// Products Controller
/// </summary>
/// <param name="client">Fauna Client</param>
[ApiController,
 Route("/[controller]")]
public class Products(Client client) : ControllerBase
{

    /// <summary>
    /// Paginates products with optional filtering by category.
    /// </summary>
    /// <param name="category">Optional category to filter products by.</param>
    /// <param name="afterToken">Optional pagination token to get the next set of products.</param>
    /// <param name="pageSize">Number of products to return per page (default is 10).</param>
    /// <returns>Page of products.</returns>
    [ProducesResponseType(typeof(Page<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> PaginateProducts(
        [FromQuery] string? category,
        [FromQuery] string? afterToken,
        [FromQuery] int pageSize = 10
    )
    {
        // Define an FQL query fragment that will return a page of products. This query
        // fragment will either return all products sorted by category or all products in a specific
        // category depending on whether the category query parameter is provided. This will later
        // be embedded in a larger query.
         var queryPrefix = string.IsNullOrEmpty(category) 
             ? Query.FQL($"Product.sortedByCategory().pageSize({pageSize})") 
             : Query.FQL($"""
                          let category = Category.byName({category}).first()
                          if (category == null) abort("Category does not exist.")
                          
                          Product.byCategory(category).pageSize({pageSize})
                          """);
         
         // The `afterToken` parameter contains a Fauna `after` cursor.
         // `after` cursors may contain special characters, such as `.` or `+`).
         // Make sure to URL encode the `afterToken` value to preserve these
         // characters in URLs.
         var query = !string.IsNullOrEmpty(afterToken) 
             
             // Paginate with the after token if it's present.
             ? Query.FQL($"Set.paginate({afterToken})")
             
             // Define the main query. This query will return a page of products using the query fragment
             // defined above.
             : Query.FQL($$"""
                           // Return only the fields we want to display to the user
                           // by mapping over the data returned by the index and returning a
                           // new object with the desired fields.
                           {{queryPrefix}}.map(product => {
                                {{QuerySnippets.ProductResponse()}}
                           })
                         """);

         // Connect to fauna using the client. The query method accepts an FQL query
         // as a parameter and a generic type parameter representing the return type.
         var result = await client.QueryAsync<Page<Product>>(query);
         return Ok(result.Data);
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
        // Using the abort function we can throw an error if a condition is not met. In this case,
        // we check if the category exists before creating the product. If the category does not exist,
        // fauna will throw an AbortError which we can handle in our catch block.
        var query = Query.FQL($$"""
                                let category = Category.byName({{product.Category}}).first()
                                if (category == null) abort("Category does not exist.")

                                // Create the product with the given values.
                                let args = { 
                                     name: {{product.Name}},
                                     price: {{product.Price}},
                                     stock: {{product.Stock}},
                                     description: {{product.Description}},
                                     category: category 
                                }
                                let product: Any = Product.create(args)
                                {{QuerySnippets.ProductResponse()}}
                                """);
        
        // Connect to fauna using the client. The query method accepts an FQL query
        // as a parameter and a generic type parameter representing the return type.
        var result = await client.QueryAsync<Product>(query);
        return Ok(result.Data);
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
    [HttpPost("{id}")]
    public async Task<IActionResult> UpdateProductById(
        [FromRoute] string id,
        ProductRequest product)
    {
       var query = Query.FQL($$"""
                                // Get the product by id, using the ! operator to assert that the product exists.
                                // If it does not exist Fauna will throw a document_not_found error.
                                let product: Any = Product.byId({{id}})!
                                if (product == null) abort("Product does not exist.")

                                // Get the category by name. We can use .first() here because we know that the category
                                // name is unique.
                                let category:Any = Category.byName({{product.Category}})?.first()
                                if (category == null) abort("Category does not exist.")

                                // Update category if a new one was provided
                                let newCategory: Any = Category.byName({{product.Category}})?.first()

                                let fields = {
                                    name: {{product.Name}},
                                    price: {{product.Price}},
                                    stock: {{product.Stock}},
                                    description: {{product.Description}}
                                }

                                if (newCategory != null && newCategory.id != category.id) {
                                  // If a category was provided, update the product with the new category document as well as
                                  // any other fields that were provided.
                                  product!.update(Object.assign(fields, { category: category }))
                                } else {
                                  // If no category was provided, update the product with the fields that were provided.
                                  product!.update(fields)
                                }

                                {{QuerySnippets.ProductResponse()}}
                                """);
       
       // Connect to fauna using the client. The query method accepts an FQL query
       // as a parameter and a generic type parameter representing the return type.
        var result = await client.QueryAsync<Product>(query);
        return Ok(result.Data);
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
        [FromQuery] int maxPrice = 10000
    )
    {
        // The `afterToken` parameter contains a Fauna `after` cursor.
        // `after` cursors may contain special characters, such as `.` or `+`).
        // Make sure to URL encode the `afterToken` value to preserve these
        // characters in URLs.
        var query = !string.IsNullOrEmpty(afterToken)
        
            // Paginate with the after token if it's present.
            ? Query.FQL($"Set.paginate({afterToken})")
            
            // This is an example of a covered query.  A covered query is a query where all fields
            // returned are indexed fields. In this case, we are querying the Product collection
            // for products with a price between minPrice and maxPrice. We are also limiting the
            // number of results returned to the limit parameter. The query is covered because
            // all fields returned are indexed fields. In this case, the fields returned are
            // `name`, `description`, `price`, and `stock` are all indexed fields.
            // Covered queries are typically faster and less expensive than uncovered queries,
            // which require document reads.
            // Learn more about covered queries here: https://docs.fauna.com/fauna/current/learn/data_model/indexes#covered-queries
            : Query.FQL($$"""
                          Product.sortedByPriceLowToHigh({ from: {{minPrice}}, to: {{maxPrice}}})
                          .pageSize({{pageSize}}).map(product => {
                              {{QuerySnippets.ProductResponse()}}
                            })
                          """);
        
        // Connect to fauna using the client. The query method accepts an FQL query
        // as a parameter and a generic type parameter representing the return type.
        var result = await client.QueryAsync<Page<Product>>(query);
        return Ok(result.Data);
    }
}
