using System.Diagnostics.CodeAnalysis;
using DotNetSampleApp.Controllers;
using DotNetSampleApp.Models;
using Fauna;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace DotNetSampleApp.Tests;

[TestFixture]
[TestOf(typeof(Orders))]
public class OrdersTest
{
    [AllowNull]
    private Orders _orders;

    [OneTimeSetUp]
    public void Setup()
    {
        _orders = new Orders(TestSetup.Client);
    }
    
    [Test]
    public async Task GetOrderTest()
    {
        var id = (await TestSetup.Client.QueryAsync<string>(
            Query.FQL($"let o = Order.all().first(); o.id")
        )).Data;
        var res = await _orders.GetOrder(id);
        switch (res)
        {
            case OkObjectResult r:
                var order = (r.Value! as Order)!;
                Assert.That(order.Id, Is.EqualTo(id));
                break;
            default:
                Assert.Fail($"Unexpected result: {res}");
                break;
        }
    }

    [Test]
    public async Task UpdateOrderTest()
    {
        var id = (await TestSetup.Client.QueryAsync<string>(
            Query.FQL($"let o = Order.all().first(); o.id")
        )).Data;

        var req = new OrderRequest
        {
            Status = "processing"
        };

        var res = await _orders.UpdateOrder(id, req);
        switch (res)
        {
            case OkObjectResult r:
                var order = (r.Value! as Order)!;
                Assert.That(order.Status, Is.EqualTo("processing"));
                break;
            default:
                Assert.Fail($"Unexpected result: {res}");
                break;
        }
    }
}