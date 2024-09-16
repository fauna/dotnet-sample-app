using dotnet_sample_app.Models;
using Fauna;
using Fauna.Linq;
using Fauna.Types;

namespace dotnet_sample_app.Repositories;

// ReSharper disable once ClassNeverInstantiated.Global
internal class CustomerDb : DataContext
{
    private static readonly Query _response = Query.FQL($$"""
                                                          // Use projection to return only the necessary fields.
                                                          customer {
                                                              id,
                                                              name,
                                                              email,
                                                              address
                                                          }
                                                          """);

    /// <summary>
    /// Get customer by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Customer</returns>
    public async Task<Customer> Get(string id)
    {
        var query = Query.FQL($"""
                               let customer :Customer? = Customer.byId({id})
                               if (customer == null) abort("Customer does not exist.")

                               {_response}
                               """);
        var result = await QueryAsync<Customer>(query);
        return result.Data;
    }

    /// <summary>
    /// Create Customer
    /// </summary>
    /// <param name="customer"></param>
    /// <returns>Customer</returns>
    public async Task<Customer> Create(CustomerRequest customer)
    {
        var query = Query.FQL($$"""
                                let customer: Customer = Customer.create({
                                  name: {{customer.Name}}, 
                                  email: {{customer.Email}}, 
                                  address: { 
                                      street: {{customer.Address.Street}}, 
                                      city: {{customer.Address.City}},
                                      state: {{customer.Address.State}},
                                      country: {{customer.Address.Country}},
                                      postalCode: {{customer.Address.PostalCode}}
                                  }
                                })

                                {{_response}}
                                """);
        var result = await QueryAsync<Customer>(query);
        return result.Data;
    }

    /// <summary>
    /// Update Customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="customer">Updated Customer details</param>
    /// <returns></returns>
    public async Task<Customer> Update(string id, CustomerRequest customer)
    {
        var query = Query.FQL($$"""
                                let customer :Customer? = Customer.byId({{id}})
                                if (customer == null) abort("Customer does not exist.")

                                customer!.update({
                                  name: {{customer.Name}},
                                  email: {{customer.Email}},
                                  address: { 
                                      street: {{customer.Address.Street}}, 
                                      city: {{customer.Address.City}}, 
                                      state: {{customer.Address.State}}, 
                                      country: {{customer.Address.Country}}, 
                                      postalCode: {{customer.Address.PostalCode}}
                                  }
                                })

                                {{_response}}
                                """);
        var result = await QueryAsync<Customer>(query);
        return result.Data;
    }

    /// <summary>
    /// Delete Customer by ID
    /// </summary>
    /// <param name="id">Customer ID</param>
    public async Task Delete(string id)
    {
        await QueryAsync(Query.FQL($"""
                                    let customer :Customer? = Customer.byId({id})
                                    if (customer == null) abort("Customer does not exist.")

                                    customer!.delete()
                                    """));
    }

    /// <summary>
    /// Get orders by customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Page of Orders</returns>
    public async Task<Page<Order>> GetOrdersByCustomer(string customerId)
    {
        var query = Query.FQL($$"""
                                let customer :Customer? = Customer.byId({{customerId}})
                                if (customer == null) abort("Customer does not exist.")

                                Order.byCustomer(customer).map((order) => {
                                  let order: Any = order
                                  {{OrderDb.Response}}
                                })
                                """);
        var result = await QueryAsync<Page<Order>>(query);
        return result.Data;
    }

    /// <summary>
    /// Get or create cart for Customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Order</returns>
    public async Task<Order> GetOrCreateCart(string customerId)
    {
        var query = Query.FQL($"""
                               let customer :Customer? = Customer.byId({customerId})
                               if (customer == null) abort("Customer does not exist.")

                               let order: Any = getOrCreateCart(customer!.id)

                               {OrderDb.Response}
                               """);
        var result = await QueryAsync<Order>(query);
        return result.Data;
    }

    /// <summary>
    /// Add item to cart
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="item">Item Details</param>
    /// <returns>Order</returns>
    public async Task<Order> AddItemToCart(string customerId, AddItemToCartRequest item)
    {
        var query = Query.FQL($"""
                               let customer :Customer? = Customer.byId({customerId})
                               if (customer == null) abort("Customer does not exist.")

                               let order: Any = createOrUpdateCartItem(customer!.id, {item.ProductName}, {item.Quantity})

                               {OrderDb.Response}
                               """);
        var result = await QueryAsync<Order>(query);
        return result.Data;
    }
}