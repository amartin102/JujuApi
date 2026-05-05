using Application.Common.Mappings;
using Application.Services;
using Application.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repository.Context;
using Repository.Interface;
using Repository.Logger;
using Repository.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Acceso a la configuración
var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ILogInterface, LogService>();
builder.Services.AddScoped<IPostRepository, PostRepository>();

// AutoMapper con configuración explícita
builder.Services.AddAutoMapper((serviceProvider, cfg) =>
{
    cfg.ConstructServicesUsing(serviceProvider.GetService);
    cfg.AddProfile<PostProfile>();
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
