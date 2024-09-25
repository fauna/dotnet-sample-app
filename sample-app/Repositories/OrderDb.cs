using dotnet_sample_app.Models;
using Fauna.Linq;

namespace dotnet_sample_app.Repositories;

// ReSharper disable once ClassNeverInstantiated.Global
internal class OrderDb : DataContext
{
    internal class OrderCol : Collection<Order> {}

    public OrderCol Orders => GetCollection<OrderCol>();

    /// <summary>
    /// Get Order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order</returns>
    public async Task<Order> Get(string id) => await Fn<Order>("getOrder").CallAsync(id); // see schema/functions.fsl

    /// <summary>
    /// Update Order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="order">Updated Order details</param>
    /// <returns>Order</returns>
    public async Task<Order> Update(string id, OrderRequest order) =>
        await Fn<Order>("updateOrder").CallAsync(id, order.Status, order.Payment); // see schema/functions.fsl
}