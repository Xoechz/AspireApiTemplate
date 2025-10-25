using AspireApiTemplate.Data;
using AspireApiTemplate.Seeder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var connectionString = builder.Configuration.GetConnectionString("DBTEMPLATENAME")
    ?? throw new InvalidOperationException("Connection String DB is not configured.");

builder.Services.AddDbContext<TemplateContext>(options => options.UseSqlServer(connectionString));

var host = builder.Build();
await host.RunAsync();