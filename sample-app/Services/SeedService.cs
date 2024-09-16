using System.Diagnostics;
using Fauna;

namespace dotnet_sample_app.Services;

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

        // Ensure categories exist
        client.QueryAsync(Query.FQL($$"""
                                      [
                                        {name: 'electronics'},
                                        {name: 'books'},
                                        {name: 'movies'}
                                      ].map(c => {
                                        Category.byName(c.name).first() ?? Category.create({ name: c.name, description: 'Bargain #{c.name}!' })
                                      })

                                      // Force empty return
                                      {}
                                      """)).Wait();

        // Ensure products exist
        client.QueryAsync(Query.FQL($$"""
                                      [
                                        {name: 'iPhone', price: 10000, description: 'Apple flagship phone', stock: 100, category: 'electronics'},
                                        {name: 'Drone', price: 9000, description: 'Fly and let people wonder if you are filming them!', stock: 0, category: 'electronics'},
                                        {name: 'Signature Box III', price: 300000, description: 'Latest box by Hooli!', stock: 1000, category: 'electronics'},
                                        {name: 'Raspberry Pi', price: 3000, description: 'A tiny computer', stock: 5, category: 'electronics'},
                                        {name: 'For Whom the Bell Tolls', price: 899, description: 'A book by Ernest Hemingway', stock: 10, category: 'books'},
                                        {name: 'Getting Started with Fauna', price: 1999, description: 'A book by Fauna, Inc.', stock: 0, category: 'books'},
                                        {name: 'The Godfather', price: 1299, description: 'A movie by Francis Ford Coppola', stock: 10, category: 'movies'},
                                        {name: 'The Godfather II', price: 1299, description: 'A movie by Francis Ford Coppola', stock: 10, category: 'movies'},
                                        {name: 'The Godfather III', price: 1299, description: 'A movie by Francis Ford Coppola', stock: 10, category: 'movies'}
                                      ].map(p => {
                                        let existing: Any = Product.byName(p.name).first()
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
    }
}