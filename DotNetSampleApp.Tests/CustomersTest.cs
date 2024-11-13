using System.Diagnostics.CodeAnalysis;
using DotNetSampleApp.Controllers;
using DotNetSampleApp.Models;
using Fauna;
using Fauna.Types;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace DotNetSampleApp.Tests;

[TestFixture]
[TestOf(typeof(Customers))]
public class CustomersTest
{
    [AllowNull]
    private Customers _cust;

    [OneTimeSetUp]
    public void Setup()
    {
        _cust = new Customers(TestSetup.Client);
    }
    
    [Test]
    public async Task GetCustomerTest()
    {
        var res = await _cust.GetCustomer("999");
        switch (res)
        {
            case OkObjectResult r:
                var customer = r.Value! as Customer;
                Assert.That(customer?.Id, Is.EqualTo("999"));
                break;
            default:
                Assert.Fail($"Unexpected result: {res}");
                break;
        }
    }
    
    [Test]
    public async Task CreateCustomerTest()
    {
        var req = new CustomerRequest
        {
            Name = "Sample Customer",
            Email = $"CreateCustomerTest{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}@fauna.com",
            Address = new Address
            {
                City = "Sample City",
                Street = "33 Sample Street",
                State = "Sample State",
                PostalCode = "12345",
                Country = "Sample Country",
            },
        };

        var res = await _cust.CreateCustomer(req);
        switch (res)
        {
            case CreatedAtActionResult r:
                var customer = r.Value! as Customer;
                Assert.That(customer?.Name, Is.EqualTo("Sample Customer"));
                break;
            default:
                Assert.Fail($"Unexpected result: {res}");
                break;
        }
    }
    
    [Test]
    public async Task UpdateCustomerTest()
    {
        var req = new CustomerRequest
        {
            Name = "Sample Customer",
            Email = $"CreateCustomerTest{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}@fauna.com",
            Address = new Address
            {
                City = "Sample City",
                Street = "33 Sample Street",
                State = "Sample State",
                PostalCode = "12345",
                Country = "Sample Country",
            },
        };

        var res = await _cust.CreateCustomer(req);
        var customerId = (res as CreatedAtActionResult)!.RouteValues!["customerId"]!.ToString()!;
        req.Name = "Sample Customer Updated";
        res = await _cust.UpdateCustomer(customerId, req);
        switch (res)
        {
            case OkObjectResult r:
                var customer = r.Value! as Customer;
                Assert.That(customer?.Name, Is.EqualTo("Sample Customer Updated"));
                break;
            default:
                Assert.Fail($"Unexpected result: {res}");
                break;
        }
    }
    
    [Test]
    public async Task GetCustomerOrdersTest()
    {
        var res = await _cust.GetOrdersByCustomer("999", null);
        switch (res)
        {
            case OkObjectResult r:
                var page = (r.Value! as Page<Order>)!;
                
                Assert.That(page.Data.Count, Is.GreaterThan(0));
                break;
            default:
                Assert.Fail($"Unexpected result: {res}");
                break;
        }
    }
    
    [Test]
    public async Task CreateCustomerCartTest()
    {
        var res = await _cust.CreateCart("999");
        switch (res)
        {
            case CreatedAtActionResult r:
                var order = (r.Value! as Order)!;
                
                Assert.That(order.Items.Count, Is.GreaterThan(0));
                break;
            default:
                Assert.Fail($"Unexpected result: {res}");
                break;
        }
    }
    
    [Test]
    public async Task AddItemToCartTest()
    {
        var item = new AddItemToCartRequest
        {
            ProductName = "iPhone",
            Quantity = 2
        };
        
        var res = await _cust.AddItemToCart("999", item);
        switch (res)
        {
            case OkObjectResult r:
                var order = (r.Value! as Order)!;
                Assert.That(order.Items.Find(i => i.Product.Name == "iPhone")!.Quantity, Is.EqualTo(2));
                break;
            default:
                Assert.Fail($"Unexpected result: {res}");
                break;
        }
    }
    
    [Test]
    public async Task GetCartTest()
    {
        var res = await _cust.GetCart("999");
        switch (res)
        {
            case OkObjectResult r:
                var order = (r.Value! as Order)!;
                Assert.That(order.Items.Count, Is.GreaterThan(0));
                break;
            default:
                Assert.Fail($"Unexpected result: {res}");
                break;
        }
    }
}