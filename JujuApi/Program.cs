using Application.Common.Mappings;
using Application.Services;
using Application.Services.Interface;
using Application.UseCases;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repository.Context;
using Repository.Interface;
using Repository.Logger;
using Repository.Repositories;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Acceso a la configuración
var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ILogInterface, LogService>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

builder.Services.AddScoped<CreateBulkPostsUseCase>();
builder.Services.AddScoped<IBulkPostService, BulkPostService>();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 5 * 1024 * 1024;
});

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("bulkPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name
                          ?? context.Connection.RemoteIpAddress?.ToString()
                          ?? "anon",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,                
                Window = TimeSpan.FromMinutes(3)                
            }));
});

// AutoMapper con configuración explícita
builder.Services.AddAutoMapper((serviceProvider, cfg) =>
{
    cfg.ConstructServicesUsing(serviceProvider.GetService);
    cfg.AddProfile<PostProfile>();
    cfg.AddProfile<CustomerProfile>();
}, typeof(Program).Assembly);

builder.Services.AddDbContext<JujuTestContext>(options =>
{
    options.UseSqlServer(configuration.GetConnectionString("SqlServerConnectionString"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
