using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Yoser_API.Data.Models;

namespace Yoser_API.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<PatientProfile> PatientProfiles { get; set; }
        public DbSet<MedicalProvider> MedicalProviders { get; set; }
        public DbSet<MedicationReminder> MedicationReminders { get; set; }
        public DbSet<PharmacyOrder> PharmacyOrders { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<PatientProfile>()
                .HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<PatientProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MedicalProvider>()
                .Property(s => s.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany()
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Provider)
                .WithMany()
                .HasForeignKey(a => a.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
