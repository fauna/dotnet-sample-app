using System.Diagnostics.CodeAnalysis;
using DotNetSampleApp.Controllers;
using DotNetSampleApp.Models;
using Fauna;
using Fauna.Types;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace DotNetSampleApp.Tests;

[TestFixture]
[TestOf(typeof(Products))]
public class ProductsTest
{
    [AllowNull]
    private Products _products;

    [OneTimeSetUp]
    public void Setup()
    {
        _products = new Products(TestSetup.Client);
    }
    
    [Test]
    public async Task PaginateProductsTest()
    {
        string? after = null;
        do
        {
            var res = await _products.PaginateProducts(null, after, 5);
            switch (res)
            {
                case OkObjectResult r:
                    var page = (r.Value! as Page<Product>)!;
                    Assert.That(page.Data.Count, Is.LessThanOrEqualTo(5));
                    after = page.After;
                    break;
                default:
                    Assert.Fail($"Unexpected result: {res}");
                    break;
            }
        } while (after != null);
    }
    
    [Test]
    public async Task CreateProductTest()
    {
        var productRequest = new ProductRequest
        {
            Name = "Shiny",
            Description = "A shiny new thing",
            Category = "electronics",
            Price = 1999,
            Stock = 100,
        };
        var res = await _products.CreateProduct(productRequest);
        switch (res)
        {
            case OkObjectResult r:
                var product = (r.Value! as Product)!;
                Assert.That(product.Name, Is.EqualTo("Shiny"));
                break;
            default:
                Assert.Fail($"Unexpected result: {res}");
                break;
        }
    }
    
    [Test]
    public async Task UpdateProductByIdTest()
    {
        var id = (await TestSetup.Client.QueryAsync<string>(
            Query.FQL($"let p = Product.all().first(); p.id")
            )).Data;
        var productRequest = new ProductRequest
        {
            Name = "Updated name",
            Description = "Updated description.",
            Price = 100,
            Stock = 100,
            Category = "electronics",
        };
        
        var res = await _products.UpdateProductById(id, productRequest);

        switch (res)
        {
            case OkObjectResult r:
                var product = (r.Value! as Product)!;
                Assert.That(product.Name, Is.EqualTo(productRequest.Name));
                break;
            default:
                Assert.Fail($"Unexpected result: {res}");
                break;
        }
    }
    
    [Test]
    public async Task SearchProductsTest()
    {
        string? after = null;
        do
        {
            var res = await _products.SearchProducts(after, 5, 0, 10000);
            switch (res)
            {
                case OkObjectResult r:
                    var products = (r.Value! as Page<Product>)!;
                    Assert.That(products.Data.Count, Is.LessThanOrEqualTo(5));
                    after = products.After;
                    break;
                default:
                    Assert.Fail($"Unexpected result: {res}");
                    break;
            }
        } while (after != null);
    }
}