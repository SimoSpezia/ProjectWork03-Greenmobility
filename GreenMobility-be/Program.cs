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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


// Seeding iniziale di Ruoli e Utente Admin

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
    // Creazione utente admin se non esiste già, da rimuovere in produzione
    var adminEmail = "admin@mail.it";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var newAdmin = new User
        {
            UserName = adminEmail,
            Name = "admin",
            Surname = "admin",
            Email = adminEmail,
            EmailConfirmed = true
        };

        var createAdmin = await userManager.CreateAsync(newAdmin, "MiaoMiao_321");
        if (createAdmin.Succeeded)
        {
            await userManager.AddToRoleAsync(newAdmin, Roles.ADMIN_ROLE);
        }
    }
}

app.MapControllers();

app.Run();