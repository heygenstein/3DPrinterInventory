using Microsoft.EntityFrameworkCore;

namespace PrintInventory.Data;

public class AppDbContext : DbContext
{
    // Each DbSet is one table in the database.
    public DbSet<Printer> Printers => Set<Printer>();
    public DbSet<Filament> Filaments => Set<Filament>();
    public DbSet<Spool> Spools => Set<Spool>();
    public DbSet<PrintModel> PrintModels => Set<PrintModel>();
    public DbSet<PrintJob> PrintJobs => Set<PrintJob>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={AppPaths.DatabaseFile}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Store enums as text ("PLA", "Success") so the database is easy to read.
        modelBuilder.Entity<Filament>()
            .Property(f => f.Material)
            .HasConversion<string>();

        modelBuilder.Entity<PrintJob>()
            .Property(j => j.Result)
            .HasConversion<string>();

        // Restrict: the database refuses to delete a row that is still used by another row.
        // Example: a spool with print jobs cannot be deleted (it must be archived instead).
        modelBuilder.Entity<Spool>()
            .HasOne(s => s.Filament)
            .WithMany(f => f.Spools)
            .HasForeignKey(s => s.FilamentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PrintJob>()
            .HasOne(j => j.Spool)
            .WithMany(s => s.PrintJobs)
            .HasForeignKey(j => j.SpoolId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PrintJob>()
            .HasOne(j => j.PrintModel)
            .WithMany(m => m.PrintJobs)
            .HasForeignKey(j => j.PrintModelId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PrintJob>()
            .HasOne(j => j.Printer)
            .WithMany(p => p.PrintJobs)
            .HasForeignKey(j => j.PrinterId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed data: inserted by the migration when the database is created.
        modelBuilder.Entity<Printer>().HasData(new Printer
        {
            Id = 1,
            Name = "Ender 3 S1",
            Brand = "Creality",
            Model = "Ender 3 S1",
            NozzleDiameterMm = 0.4,
            BuildVolumeXMm = 220,
            BuildVolumeYMm = 220,
            BuildVolumeZMm = 270
        });
    }
}
