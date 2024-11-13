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
        var res = await _products.PaginateProducts(null, null, 5);
        switch (res)
        {
            case OkObjectResult r:
                var page = (r.Value! as Page<Product>)!;
                Assert.That(page.Data.Count, Is.EqualTo(5));
                Assert.That(page.After, Is.Not.Null);
                break;
            default:
                Assert.Fail($"Unexpected result: {res}");
                break;
        }
        
        var res2 = await _products.PaginateProducts(null, null);
        switch (res2)
        {
            case OkObjectResult r:
                var page = (r.Value! as Page<Product>)!;
                Assert.That(page.Data.Count, Is.EqualTo(5));
                Assert.That(page.After, Is.Not.Null);
                break;
            default:
                Assert.Fail($"Unexpected result: {res}");
                break;
        }
    }
    
}