using ESourcing.Order.Extensions;
using Ordering.Application;
using Ordering.Insfrastructure;
using Ordering.Insfrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
#region Insfrastructure
builder.Services.AddInfrastructure(builder.Configuration);

#endregion

#region AddAplication
builder.Services.AddApplication();

#endregion
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.MigrateDatabase().Run();
