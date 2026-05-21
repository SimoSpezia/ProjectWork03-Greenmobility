using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GreenMobility_be.Data
{
    public class GreenMobilityDbContext : IdentityDbContext<User>
    {
        public GreenMobilityDbContext() : base() { }
        public GreenMobilityDbContext(DbContextOptions<GreenMobilityDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<VehicleStatus>().HasData(
                new VehicleStatus { VehicleStatusId = 1, Status = "Disponibile" },
                new VehicleStatus { VehicleStatusId = 2, Status = "In Uso" },
                new VehicleStatus { VehicleStatusId = 3, Status = "In Manutenzione" }
            );
            modelBuilder.Entity<VehicleType>().HasData(
                new VehicleType { VehicleTypeId = 1, Type = "E-Bike" },
                new VehicleType { VehicleTypeId = 2, Type = "Monopattino" }
            );

            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaim");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogin");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRole");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");


            // Configurazione Indici Filtrati ed Ottimizzati per Rentals
            modelBuilder.Entity<Rental>()
                .HasIndex(r => r.UserId)
                .HasFilter("[EndDate] IS NULL")
                .HasDatabaseName("IX_Active_Rentals");

            modelBuilder.Entity<Rental>()
                .HasIndex(r => r.RentalCode)
                .IsUnique()
                .HasFilter("[RentalCode] IS NOT NULL AND [EndDate] IS NULL")
                .HasDatabaseName("UX_RentalCode");

            modelBuilder.Entity<Rental>()
                .HasIndex(r => r.EndDate)
                .IsDescending()
                .HasFilter("[EndDate] IS NOT NULL")
                .HasDatabaseName("IX_Rentals_History_Desc");

            // Configurazione Indici per Vehicles
            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => new { v.HubId, v.IsDeleted, v.VehicleStatusId })
                .HasDatabaseName("IX_Vehicles_Hub_Filter");

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.ApiKey)
                .IsUnique()
                .HasDatabaseName("UX_Vehicles_ApiKey");


            // Configurazione Indici per Hubs
            modelBuilder.Entity<Hub>()
                .HasIndex(h => h.Name)
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("IX_Hubs_Active");

            // Configurazione Indici per Users
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("UX_Users_Email");

        }
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<VehicleType> VehicleTypes { get; set; } = null!;
        public DbSet<VehicleStatus> VehicleStatuses { get; set; } = null!;
        public DbSet<Hub> Hubs { get; set; } = null!;
        public DbSet<Rental> Rentals { get; set; } = null!;
    }
}
