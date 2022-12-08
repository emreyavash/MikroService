using ESourcing.Sourcing.Data;
using ESourcing.Sourcing.Data.Interface;
using ESourcing.Sourcing.Repositories;
using ESourcing.Sourcing.Repositories.Interface;
using ESourcing.Sourcing.Settings;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.Configure<SourcingDatabaseSettings>(builder.Configuration.GetSection(nameof(SourcingDatabaseSettings)));

builder.Services.AddSingleton<ISourcingDatabaseSettings>( sp=> sp.GetRequiredService<IOptions<SourcingDatabaseSettings>>().Value);

builder.Services.AddTransient<ISourcingContext, SourcingContext>();
builder.Services.AddTransient<IAuctionRepository, AuctionRepository>();
builder.Services.AddTransient<IBidRepository, BidRepository>();

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
