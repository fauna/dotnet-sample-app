using System.Diagnostics.CodeAnalysis;
using DotNetSampleApp.Controllers;
using DotNetSampleApp.Models;
using DotNetSampleApp.Services;
using Fauna;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace DotNetSampleApp.Tests.Controllers;

[TestFixture]
[TestOf(typeof(Customers))]
public class CustomersTest
{
    [AllowNull]
    private Client _fauna;

    [AllowNull]
    private Customers _cust;

    [OneTimeSetUp]
    public void Setup()
    {
        _fauna = new Client(new Configuration("secret")
        {
            Endpoint = new Uri("http://localhost:8443"),
        });
        _cust = new Customers(_fauna);
        
        SeedService.Init(_fauna);
    }
    
    [Test]
    public async Task GetExistingCustomerTest()
    {
        var res = await _cust.GetCustomer("999");
        switch (res)
        {
            case OkObjectResult r:
                Assert.That(r.StatusCode, Is.EqualTo(200));
                var customer = r.Value! as Customer;
                Assert.That(customer?.Id, Is.EqualTo("999"));
                break;
            default:
                Assert.Fail($"Unexpected result when getting existing customer: {res}");
                break;
        }
    }
}