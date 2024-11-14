using DotNetSampleApp.Services;
using Fauna;
using NUnit.Framework;

namespace DotNetSampleApp.Tests;

[SetUpFixture]
public class TestSetup
{
    public static string Secret = "";
    public static Client Client = null!;
    
    [OneTimeSetUp]
    public void SetupOnce()
    {
        if (Environment.GetEnvironmentVariable("FAUNA_SECRET") == null)
        {
            var c = new Client(new Configuration("secret")
            {
                Endpoint = new Uri("http://localhost:8443"),
            });
            Secret = c.QueryAsync<string>(Query.FQL($$"""
                                                      let k = Key.create({role: 'admin', database: 'ECommerceDotnet'})
                                                      k.secret
                                                      """)).Result.Data;
        }
        else
        {
            Secret = Environment.GetEnvironmentVariable("FAUNA_SECRET")!;
        }
        
        Client = new Client(new Configuration(Secret)
        {
            Endpoint = new Uri("http://localhost:8443"),
        });
        
        SeedService.Init(Client);
    }

}