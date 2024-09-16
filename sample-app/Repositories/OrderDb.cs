using dotnet_sample_app.Models;
using Fauna;
using Fauna.Linq;

namespace dotnet_sample_app.Repositories;

// ReSharper disable once ClassNeverInstantiated.Global
internal class OrderDb : DataContext
{
    internal static readonly Query Response = Query.FQL($$"""
                                                          // Use projection to return only the necessary fields.
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

    /// <summary>
    /// Get Order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order</returns>
    public async Task<Order> Get(string id)
    {
        var query = Query.FQL($"""
                               let order :Any = Order.byId({id})
                               if (order == null) abort("Order does not exist.")

                               {Response}
                               """);
        var result = await QueryAsync<Order>(query);
        return result.Data;
    }

    /// <summary>
    /// Update Order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="order">Updated Order details</param>
    /// <returns>Order</returns>
    public async Task<Order> Update(string id, OrderRequest order)
    {
        var query = order.Status == "processing"
            ? Query.FQL($$"""
                          let order: Any = checkout({{id}}, {{order.Status}}, { type: "credit_card" })

                          {{Response}}
                          """)
            : Query.FQL($$"""
                          let order :Any = Order.byId({{id}})
                          if (order == null) abort("Order does not exist.")

                          // Validate the order status transition if a status is provided.
                          if ({{order.Status}} != null) validateOrderStatusTransition(order.status, {{order.Status}})

                          // If the order status is not "cart" and a payment is provided, throw an error.
                          if (order.status != "cart" && {{order.Payment}} != null) abort("Can not update payment information after an order has been placed.")

                          // Update the order with the new status and payment information.
                          order.update({ status: {{order.Status}}, payment: {{order.Payment}} })

                          {{Response}}
                          """);

        var result = await QueryAsync<Order>(query);
        return result.Data;
    }
}