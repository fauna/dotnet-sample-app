using System.Reflection;
using DotNetSampleApp.Middlewares;
using DotNetSampleApp.Services;
using Fauna;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new ProducesAttribute("application/json"));
    options.Filters.Add(new ConsumesAttribute("application/json"));
});

builder.Services.AddSingleton<Client>(_ =>
{
    var secret = Environment.GetEnvironmentVariable("FAUNA_SECRET");
    if (string.IsNullOrEmpty(secret))
    {
        throw new InvalidOperationException("Required environment variable not set: FAUNA_SECRET");
    }

    var client = new Client(secret);
    SeedService.Init(client);

    return client;
});
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Fauna dotnet Sample App",
        Description = "A mock shopping API",
        Contact = new OpenApiContact
        {
            Name = "Fauna Support",
            Url = new Uri("https://fauna.com/contact-us")
        },
        License = new OpenApiLicense
        {
            Name = "MPL 2.0"
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();


app.Run();
