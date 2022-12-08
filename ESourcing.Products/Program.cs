using ESourcing.Products.Data;
using ESourcing.Products.Data.Interfaces;
using ESourcing.Products.Repositories;
using ESourcing.Products.Repositories.Interfaces;
using ESourcing.Products.Settings;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

#region Configuration Dependencies
builder.Services.Configure<ProductDatabaseSettings>(builder.Configuration.GetSection(nameof(ProductDatabaseSettings)));
builder.Services.AddSingleton<IProductDatabaseSettings>(sp => sp.GetRequiredService<IOptions<ProductDatabaseSettings>>().Value);
#endregion

#region  Project Dependencies
builder.Services.AddTransient<IProductContext, ProductContext>();
builder.Services.AddTransient<IProductRepository, ProductRepository>(); 
#endregion
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title="ESourcing Products",
        Version="v1",
    });
});

var app = builder.Build();

//Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c=> c.SwaggerEndpoint("/swagger/v1/swagger.json","ESourcing.Products v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
