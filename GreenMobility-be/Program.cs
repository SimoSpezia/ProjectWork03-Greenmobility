using GreenMobility_be.Data;
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddSqlServer<GreenMobilityDbContext>(builder.Configuration.GetConnectionString("Default"));

builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<GreenMobilityDbContext>()
                .AddDefaultTokenProviders();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
