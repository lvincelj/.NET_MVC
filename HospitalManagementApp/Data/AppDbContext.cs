using HospitalManagementApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementApp.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PacijentDatoteka> PacijentDatoteke => Set<PacijentDatoteka>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Doctor>()
            .HasMany(d => d.Departments)
            .WithMany(d => d.Doctors)
            .UsingEntity(j => j.ToTable("DoctorDepartments"));

        modelBuilder.Entity<PacijentDatoteka>()
            .ToTable("PacijentDatoteke");

        modelBuilder.Entity<PacijentDatoteka>()
            .HasOne(f => f.Pacijent)
            .WithMany(p => p.PacijentDatoteke)
            .HasForeignKey(f => f.PacijentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PacijentDatoteka>()
            .HasIndex(f => f.PacijentId);
    }
}