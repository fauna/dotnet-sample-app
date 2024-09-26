using dotnet_sample_app.Models;
using Fauna;
using Fauna.Linq;
using Fauna.Types;

namespace dotnet_sample_app.Repositories;

// ReSharper disable once ClassNeverInstantiated.Global
internal class CustomerDb : DataContext
{
    /// <summary>
    /// Get customer by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Customer</returns>
    public async Task<Customer> Get(string id) =>
        await Fn<Customer>("getCustomer").CallAsync(id); // see schema/functions.fsl


    /// <summary>
    /// Create Customer
    /// </summary>
    /// <param name="customer"></param>
    /// <returns>Customer</returns>
    public async Task<Customer> Create(CustomerRequest customer) =>
        await Fn<Customer>("createCustomer").CallAsync(customer); // see schema/functions.fsl

    /// <summary>
    /// Update Customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="customer">Updated Customer details</param>
    /// <returns></returns>
    public async Task<Customer> Update(string id, CustomerRequest customer) =>
        await Fn<Customer>("updateCustomer").CallAsync(id, customer); // see schema/functions.fsl

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
    public async Task<Page<Order>> GetOrdersByCustomer(string customerId) =>
        await Fn<Page<Order>>("getOrdersByCustomer").CallAsync(customerId); // see schema/functions.fsl

    /// <summary>
    /// Get or create cart for Customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Order</returns>
    public async Task<Order> GetOrCreateCart(string customerId) =>
        await Fn<Order>("getOrCreateCartForCustomer").CallAsync(customerId); // see schema/functions.fsl

    /// <summary>
    /// Add item to cart
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="item">Item Details</param>
    /// <returns>Order</returns>
    public async Task<Order> AddItemToCart(string customerId, AddItemToCartRequest item) =>
        await Fn<Order>("updateCartItem").CallAsync(customerId, item.ProductName, item.Quantity); // see schema/functions.fsl
}