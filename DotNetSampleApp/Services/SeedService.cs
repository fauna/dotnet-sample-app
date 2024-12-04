using Fauna;

namespace DotNetSampleApp.Services;

/// <summary>
/// Creates seed data
/// </summary>
public static class SeedService
{
    /// <summary>
    /// Init seed data
    /// </summary>
    /// <param name="client"></param>
    public static void Init(Client client)
    {
        // Clear Customer History
        client.QueryAsync(Query.FQL($"Customer.all().forEach(c => c.delete())")).Wait();
        client.QueryAsync(Query.FQL($"Product.all().forEach(c => c.delete())")).Wait();
        client.QueryAsync(Query.FQL($"Order.all().forEach(c => c.delete())")).Wait();

        // Ensure categories exist
        client.QueryAsync(Query.FQL($$"""
                                      [
                                        {name: 'electronics'},
                                        {name: 'books'},
                                        {name: 'movies'}
                                      ].map(c => {
                                        Category.byName(c.name).first() ?? Category.create({ name: c.name, description: "Bargain #{c.name}!" })
                                      })

                                      // Force empty return
                                      {}
                                      """)).Wait();

        // Ensure products exist
        client.QueryAsync(Query.FQL($$"""
                                      [
                                        {name: 'iPhone', price: 10000, description: 'Apple flagship phone', stock: 100, category: 'electronics'},
                                        {name: 'Drone', price: 9000, description: 'Fly and let people wonder if you are filming them!', stock: 1, category: 'electronics'},
                                        {name: 'Signature Box III', price: 300000, description: 'Latest box by Hooli!', stock: 1000, category: 'electronics'},
                                        {name: 'Raspberry Pi', price: 3000, description: 'A tiny computer', stock: 5, category: 'electronics'},
                                        {name: 'For Whom the Bell Tolls', price: 899, description: 'A book by Ernest Hemingway', stock: 10, category: 'books'},
                                        {name: 'Getting Started with Fauna', price: 1999, description: 'A book by Fauna, Inc.', stock: 0, category: 'books'},
                                        {name: 'The Godfather', price: 1299, description: 'A movie by Francis Ford Coppola', stock: 10, category: 'movies'},
                                        {name: 'The Godfather II', price: 1299, description: 'A movie by Francis Ford Coppola', stock: 10, category: 'movies'},
                                        {name: 'The Godfather III', price: 1299, description: 'A movie by Francis Ford Coppola', stock: 10, category: 'movies'}
                                      ].map(p => {
                                        let existing = Product.byName(p.name).first()
                                        if (existing != null) {
                                          existing!.update({ stock: p.stock })
                                        } else {
                                          Product.create({
                                            name: p.name,
                                            price: p.price,
                                            description: p.description,
                                            stock: p.stock,
                                            category: Category.byName(p.category).first()!
                                          })
                                        }
                                      })

                                      // Force empty return
                                      {}
                                      """)).Wait();
        // Ensure customers exist
        client.QueryAsync(Query.FQL($$"""
                                      [{
                                        id: '999',
                                        name: 'Valued Customer',
                                        email: 'fake@fauna.com',
                                        address: {
                                          street: '123 Main St',
                                          city: 'San Francisco',
                                          state: 'CA',
                                          postalCode: '12345',
                                          country: 'United States'
                                        }
                                      }].map(c => Customer.byEmail(c.email).first() ?? Customer.create(c))

                                      // Force empty return
                                      {}
                                      """)).Wait();

        // Ensure orders exist
        client.QueryAsync(Query.FQL($$"""
                                      let customer = Customer.byEmail('fake@fauna.com').first()!
                                      let orders = ['cart', 'processing', 'shipped', 'delivered'].map(status => {
                                        let order = Order.byCustomer(customer).firstWhere(o => o.status == status)
                                        if (order == null) {
                                          let newOrder = Order.create({
                                            customer: customer,
                                            status: status,
                                            createdAt: Time.now(),
                                            payment: {}
                                          })
                                          let product = Product.byName('Drone').first()!
                                          let orderItem = OrderItem.create({ order: newOrder, product: product, quantity: 1 })
                                          orderItem
                                          newOrder
                                        } else {
                                          order
                                        }
                                      })
                                      orders

                                      // Force empty return
                                      {}
                                      """)).Wait();

    }
}
