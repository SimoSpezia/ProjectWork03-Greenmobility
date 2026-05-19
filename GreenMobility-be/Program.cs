using GreenMobility_be.Data;
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSqlServer<GreenMobilityDbContext>(
    builder.Configuration.GetConnectionString("Default"));

// ── IDENTITY ─────────────────────────────────────────────────
// Necessario per [Authorize] e la gestione dei ruoli
builder.Services.AddIdentityApiEndpoints<User>()   // ← espone /register, /login, /logout
    .AddRoles<IdentityRole>()                      // ← abilita la gestione dei ruoli
    .AddEntityFrameworkStores<GreenMobilityDbContext>(); // ← usa il tuo DbContext

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();    // ← mancava: deve stare PRIMA di UseAuthorization
app.UseAuthorization();

app.MapIdentityApi<User>(); // ← mancava: registra le rotte /login /register /logout
app.MapControllers();

app.Run();