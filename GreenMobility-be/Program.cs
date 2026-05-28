using Azure.Identity;
using GreenMobility_be.Data;
using GreenMobility_be.Mapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Aggiunta di Azure Key Vault come provider di configurazione
var keyVaultEndpoint = new Uri(builder.Configuration["KeyVault:Azure:Endpoint"]);

// Utilizza Azure Managed Identity
builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSqlServer<GreenMobilityDbContext>(
    builder.Configuration.GetConnectionString("Default"));

builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<GreenMobilityDbContext>()
                .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"])),
    };
});

builder.Services.AddScoped<UserMapper>();
builder.Services.AddScoped<VehicleMapper>();
builder.Services.AddScoped<HubMapper>();
builder.Services.AddScoped<RentalMapper>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalDevCors", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseCors("LocalDevCors");
}


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


// Seeding iniziale di Ruoli

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    string[] roleNames = { Roles.CUSTOMER_ROLE, Roles.OPERATOR_ROLE, Roles.ADMIN_ROLE };

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
app.Run();