using ESourcing.Sourcing.Data;
using ESourcing.Sourcing.Data.Interface;
using ESourcing.Sourcing.Repositories;
using ESourcing.Sourcing.Repositories.Interface;
using ESourcing.Sourcing.Settings;
using EventBusRabbitMQ;
using EventBusRabbitMQ.Producer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.Configure<SourcingDatabaseSettings>(builder.Configuration.GetSection(nameof(SourcingDatabaseSettings)));

builder.Services.AddSingleton<ISourcingDatabaseSettings>( sp=> sp.GetRequiredService<IOptions<SourcingDatabaseSettings>>().Value);

builder.Services.AddTransient<ISourcingContext, SourcingContext>();
builder.Services.AddTransient<IAuctionRepository, AuctionRepository>();
builder.Services.AddTransient<IBidRepository, BidRepository>();
builder.Services.AddAutoMapper(typeof(Program));

#region EventBus
builder.Services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

    var factory = new ConnectionFactory()
    {
        HostName = builder.Configuration["EventBus:HostName"]

    };
    if (!string.IsNullOrWhiteSpace(builder.Configuration["EventBus:Username"]))
    {
        factory.UserName = builder.Configuration["EventBus:Username"];
    } 
    if (!string.IsNullOrWhiteSpace(builder.Configuration["EventBus:Password"]))
    {
        factory.Password = builder.Configuration["EventBus:Password"];
    }
    var retryCount = 5;

    if (!string.IsNullOrWhiteSpace(builder.Configuration["EventBus:RetryCount"]))
    {
        retryCount = int.Parse(builder.Configuration["EventBus:RetryCount"]);
    }
    return new DefaultRabbitMQPersistentConnection(factory, retryCount, logger);
});
builder.Services.AddSingleton<EventBusRabbitMQProducer>();
#endregion
builder.Services.AddSwaggerGen(s=>
{
    s.SwaggerDoc("v1",new OpenApiInfo { Title="ESourcing.Sourcing",Version="v1"});
});
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sourcing Api V1");
});
app.Run();
