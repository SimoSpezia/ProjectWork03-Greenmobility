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
        }
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<VehicleType> VehicleTypes { get; set; } = null!;
        public DbSet<VehicleStatus> VehicleStatuses { get; set; } = null!;
        public DbSet<Hub> Hubs { get; set; } = null!;
        public DbSet<Rental> Rentals { get; set; } = null!;
    }
}
