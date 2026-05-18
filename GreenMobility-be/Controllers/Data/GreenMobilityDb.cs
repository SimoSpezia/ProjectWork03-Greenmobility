using Microsoft.EntityFrameworkCore;

namespace GreenMobility_be.Controllers.Data
{
    public class GreenMobilityDb : DbContext
    {
        public GreenMobilityDb(DbContextOptions<GreenMobilityDb> options) : base(options)
        {
        }

       

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure one-to-one between Rental and RentalCode (optional)
            modelBuilder.Entity<Rental>()
                .HasOne(r => r.Code)
                .WithOne(c => c.Rental)
                .HasForeignKey<RentalCode>(c => c.RentalId);

            // scalar properties and relationships
            modelBuilder.Entity<User>(eb =>
            {
                eb.HasKey(u => u.UserId);
                eb.Property(u => u.Name).IsRequired().HasMaxLength(100);
                eb.Property(u => u.Surname).IsRequired().HasMaxLength(100);
                eb.Property(u => u.Email).IsRequired().HasMaxLength(200);
                eb.Property(u => u.Role).IsRequired().HasMaxLength(50);
                eb.HasMany(u => u.Rentals).WithOne(r => r.User).HasForeignKey(r => r.UserId);
            });

            modelBuilder.Entity<VehicleType>(eb =>
            {
                eb.HasKey(t => t.VehicleTypeId);
                eb.Property(t => t.Type).IsRequired().HasMaxLength(100);
                eb.HasMany(t => t.Vehicles).WithOne(v => v.VehicleType).HasForeignKey(v => v.VehicleTypeId);
            });

            modelBuilder.Entity<VehicleStatus>(eb =>
            {
                eb.HasKey(s => s.VehicleStatusId);
                eb.Property(s => s.Status).IsRequired().HasMaxLength(100);
                eb.HasMany(s => s.Vehicles).WithOne(v => v.Status).HasForeignKey(v => v.VehicleStatusId);
            });

            modelBuilder.Entity<Hub>(eb =>
            {
                eb.HasKey(h => h.HubId);
                eb.Property(h => h.Name).IsRequired().HasMaxLength(150);
                eb.Property(h => h.City).IsRequired().HasMaxLength(150);
                eb.Property(h => h.MaximumCapacity).IsRequired();
                eb.HasMany(h => h.Vehicles).WithOne(v => v.Hub).HasForeignKey(v => v.HubId);
            });

            modelBuilder.Entity<Vehicle>(eb =>
            {
                eb.HasKey(v => v.VehicleId);
                eb.Property(v => v.BatteryLevel).IsRequired();
            });

            modelBuilder.Entity<Rental>(eb =>
            {
                eb.HasKey(r => r.RentalId);
                eb.Property(r => r.StartDate).IsRequired();
                eb.Property(r => r.EndDate).IsRequired();
                eb.Property(r => r.TotalCost).HasColumnType("decimal(10,2)").IsRequired();
                eb.HasOne(r => r.Vehicle).WithMany(v => v.Rentals).HasForeignKey(r => r.VehicleId);
            });

            modelBuilder.Entity<RentalCode>(eb =>
            {
                eb.HasKey(c => c.RentalCodeId);
                eb.Property(c => c.Code).IsRequired().HasMaxLength(100);
                eb.HasIndex(c => c.RentalId).IsUnique();
            });

            modelBuilder.Entity<VehicleStatus>().HasData(
                new VehicleStatus { VehicleStatusId = 1, Status = "Disponibile" },
                new VehicleStatus { VehicleStatusId = 2, Status = "In Uso" },
                new VehicleStatus { VehicleStatusId = 3, Status = "In Manutenzione" }
            );
            modelBuilder.Entity<VehicleType>().HasData(
                new VehicleType { VehicleTypeId = 1, Type = "E-Bike" },
                new VehicleType { VehicleTypeId = 2, Type = "Monopattino" }
            );
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<VehicleType> VehicleTypes { get; set; } = null!;
        public DbSet<VehicleStatus> VehicleStatuses { get; set; } = null!;
        public DbSet<Hub> Hubs { get; set; } = null!;
        public DbSet<Rental> Rentals { get; set; } = null!;
        public DbSet<RentalCode> RentalCodes { get; set; } = null!;
    }
}
