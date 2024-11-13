using DotNetSampleApp.Services;
using Fauna;
using NUnit.Framework;

namespace DotNetSampleApp.Tests;

[SetUpFixture]
public class Setup
{
    [OneTimeSetUp]
    public void SetupOnce()
    {
        var c = new Client(new Configuration("secret")
        {
            Endpoint = new Uri("http://localhost:8443"),
        });
        SeedService.Init(c);
    }
}