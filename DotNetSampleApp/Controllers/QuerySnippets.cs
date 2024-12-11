using Fauna;

namespace DotNetSampleApp.Controllers;

/// <summary>
/// A class with query snippets.
/// </summary>
public static class QuerySnippets
{
  /// <summary>
  /// A Query snippet for customer response projection. 
  /// </summary>
  /// <returns></returns>
  public static Query CustomerResponse()
  {
    return Query.FQL($$"""
                       // Use projection to return only the necessary fields.
                       customer {
                         id,
                         name,
                         email,
                         address
                       }
                       """);
  }

  /// <summary>
  /// A Query snippet for order response projection. 
  /// </summary>
  /// <returns></returns>
  public static Query OrderResponse()
  { 
    return Query.FQL($$"""
                       {
                         id: order.id,
                         payment: order.payment,
                         createdAt: order.createdAt,
                         status: order.status,
                         total: order.total,
                         items: order.items.toArray().map((item) => {
                           product: {
                             id: item.product.id,
                             name: item.product.name,
                             price: item.product.price,
                             description: item.product.description,
                             stock: item.product.stock,
                             category: {
                               id: item.product.category.id,
                               name: item.product.category.name,
                               description: item.product.category.description
                             }
                           },
                           quantity: item.quantity
                         }),
                         customer: {
                           id: order.customer.id,
                           name: order.customer.name,
                           email: order.customer.email,
                           address: order.customer.address
                         }
                       }
                     """);
  }

  /// <summary>
  /// A Query snippet for product response projection. 
  /// </summary>
  /// <returns></returns>
  public static Query ProductResponse()
  {
    return Query.FQL($$"""
                       // Use projection to return only the necessary fields.
                       let product: Any = product
                       let category: Any = product.category
                       product {
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
                       """);
  }
}