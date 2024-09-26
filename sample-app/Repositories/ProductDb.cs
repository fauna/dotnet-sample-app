using dotnet_sample_app.Models;
using Fauna;
using Fauna.Linq;
using Fauna.Types;

namespace dotnet_sample_app.Repositories;

// ReSharper disable once ClassNeverInstantiated.Global
internal class ProductDb : DataContext
{
    internal class ProductCol : Collection<Product> { }
    public ProductCol Products => GetCollection<ProductCol>();
    
    internal class CategoryCol : Collection<Category> { }

    public CategoryCol Categories => GetCollection<CategoryCol>();


    private readonly Query _response = Query.FQL($$"""
                                                   // Use projection to return only the necessary fields.
                                                   product {
                                                       id,
                                                       name,
                                                       price,
                                                       description,
                                                       stock,
                                                       category {
                                                           id,
                                                           name,
                                                           description
                                                       }
                                                   }
                                                   """);


    /// <summary>
    /// Create a Product
    /// </summary>
    /// <param name="product">Product details</param>
    /// <returns>Product</returns>
    public async Task<Product> Create(ProductRequest product)
    {
        var result = await QueryAsync<Product>(Query.FQL($$"""
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

                                                           {{_response}}
                                                           """));
        return result.Data;
    }

    /// <summary>
    /// Update Product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="product">Updated Product details</param>
    /// <returns>Product</returns>
    public async Task<Product> Update(string id, ProductRequest product)
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

                                {{_response}}
                                """);
        var result = await QueryAsync<Product>(query);

        return result.Data;
    }

    /// <summary>
    /// Delete Product
    /// </summary>
    /// <param name="id">Product ID</param>
    public async Task Delete(string id)
    {
        await QueryAsync(Query.FQL($"""
                                    let product: Product? = Product.byId({id})
                                    if (product == null) abort("Product does not exist.")

                                    product!.delete()
                                    """));
    }

    /// <summary>
    /// List Products, optionally filtered by category
    /// </summary>
    /// <param name="category">Category Name</param>
    /// <param name="afterToken">Optional pagination token</param>
    /// <param name="pageSize">Number of products to return per page</param>
    /// <returns>Page of Products</returns>
    public async Task<Page<Product>> List(string? category, string? afterToken, int pageSize)
    {
        Query query;
        if (!string.IsNullOrEmpty(afterToken))
        {
            query = Query.FQL($"Set.paginate({afterToken})");
        }
        else
        {
            query = Query.FQL($"Product.sortedByCategory().pageSize({pageSize})");
            if (!string.IsNullOrEmpty(category))
            {
                query = Query.FQL($"""
                                   let category: Category = Category.byName({category})!.first()

                                   Product.byCategory(category).pageSize({pageSize})
                                   """);
            }

            query = new QueryExpr(query, Query.FQL($$"""
                                                         // Return only the fields we want to display to the user
                                                         // by mapping over the data returned by the index and returning a
                                                         // new object with the desired fields.
                                                         .map(product => {
                                                             let product: Any = product
                                                             let category: Any = product.category
                                                             {
                                                                 id: product.id,
                                                                 name: product.name,
                                                                 price: product.price,
                                                                 description: product.description,
                                                                 stock: product.stock,
                                                                 category: {
                                                                     id: category.id,
                                                                     name: category.name,
                                                                     description: category.description 
                                                                 },
                                                             }
                                                         })
                                                     """));
        }

        var result = await QueryAsync<Page<Product>>(query);
        return result.Data;
    }

    /// <summary>
    /// Search products with pagination
    /// </summary>
    /// <param name="pageSize">Number of items to return per page</param>
    /// <param name="minPrice">Minimum price</param>
    /// <param name="maxPrice">Maximum price</param>
    /// <param name="afterToken">Optional pagination token</param>
    /// <returns>Page of Products</returns>
    public async Task<Page<Product>> Search(
        int pageSize,
        int minPrice,
        int maxPrice,
        string? afterToken
    )
    {
        var query = !string.IsNullOrEmpty(afterToken)
            ? Query.FQL($"Set.paginate({afterToken})")
            : Query.FQL($$"""
                          Product.sortedByPriceLowToHigh({ from: {{minPrice}}, to: {{maxPrice}}})
                          .pageSize({{pageSize}}).map(product => {
                              let product: Any = product
                              
                              {{_response}}
                            })
                          """);

        var result = await QueryAsync<Page<Product>>(query);

        return result.Data;
    }
}