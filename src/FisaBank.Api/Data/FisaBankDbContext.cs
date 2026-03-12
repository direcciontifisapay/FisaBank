using Microsoft.EntityFrameworkCore;
using FisaBank.Api.Models;

namespace FisaBank.Api.Data;

// ═══════════════════════════════════════════════════════════════════════════════
// DbContext  —  FisaBank v1
// ═══════════════════════════════════════════════════════════════════════════════

public class FisaBankDbContext : DbContext
{
    public FisaBankDbContext(DbContextOptions<FisaBankDbContext> options) : base(options) { }

    public DbSet<User>         Users         => Set<User>();
    public DbSet<Payment>      Payments      => Set<Payment>();
    public DbSet<AuditLog>     AuditLogs     => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Owner)
            .WithMany(u => u.Payments)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RefreshToken>()
            .HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // SQLite no tiene tipo DECIMAL nativo — usar TEXT
        modelBuilder.Entity<User>()
            .Property(u => u.AccountBalance)
            .HasColumnType("TEXT");

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasColumnType("TEXT");
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// Seed  —  Datos iniciales para el laboratorio
// ═══════════════════════════════════════════════════════════════════════════════

public static class DatabaseSeeder
{
    public static void Seed(FisaBankDbContext db)
    {
        db.Database.EnsureCreated();

        if (db.Users.Any()) return;

        // ── Usuarios ────────────────────────────────────────────────────────────
        var carlos = new User
        {
            Id            = 1,
            Email         = "carlos@tfsco.com",
            PasswordHash  = BCrypt.Net.BCrypt.HashPassword("Abc123!"),
            FullName      = "Carlos Méndez",
            SSN           = "123-45-6789",
            AccountBalance= 15_000_000m,
            PhoneNumber   = "+57 310 555 0101",
            Address       = "Calle 93 # 15-32, Bogotá",
            IsAdmin       = false,
            LastLoginAt   = DateTime.UtcNow.AddDays(-2),
        };

        var laura = new User
        {
            Id            = 2,
            Email         = "laura@tfsco.com",
            PasswordHash  = BCrypt.Net.BCrypt.HashPassword("Xyz789!"),
            FullName      = "Laura Jiménez",
            SSN           = "987-65-4321",
            AccountBalance= 8_500_000m,
            PhoneNumber   = "+57 315 555 0202",
            Address       = "Carrera 11 # 82-45, Bogotá",
            IsAdmin       = false,
            LastLoginAt   = DateTime.UtcNow.AddDays(-1),
        };

        var admin = new User
        {
            Id            = 3,
            Email         = "admin@fisabank.com",
            PasswordHash  = BCrypt.Net.BCrypt.HashPassword("Admin@2026!"),
            FullName      = "Administrador FisaBank",
            SSN           = "000-00-0000",
            AccountBalance= 0m,
            PhoneNumber   = "+57 300 000 0000",
            Address       = "Sede Principal FisaBank",
            IsAdmin       = true,
            LastLoginAt   = DateTime.UtcNow.AddHours(-3),
        };

        db.Users.AddRange(carlos, laura, admin);

        // ── Pagos ───────────────────────────────────────────────────────────────
        db.Payments.AddRange(
            new Payment { Id=1, OwnerId=1, Description="Pago cuota seguro vehicular",
                Amount=450_000m, Currency="COP", Status="completed",
                RecipientName="Seguros Colombia S.A.", RecipientAccount="COL-00123456",
                CreatedAt=DateTime.UtcNow.AddDays(-15), ProcessedAt=DateTime.UtcNow.AddDays(-15) },

            new Payment { Id=2, OwnerId=1, Description="Abono cuota Toyota Plan Mayor",
                Amount=1_200_000m, Currency="COP", Status="completed",
                RecipientName="Toyota Financial Services", RecipientAccount="TFS-00789012",
                CreatedAt=DateTime.UtcNow.AddDays(-7), ProcessedAt=DateTime.UtcNow.AddDays(-7) },

            new Payment { Id=3, OwnerId=1, Description="Comisión vendedor independiente",
                Amount=320_000m, Currency="COP", Status="pending",
                RecipientName="Andrés Reyes", RecipientAccount="NEQUI-3105550303",
                CreatedAt=DateTime.UtcNow.AddDays(-1) },

            new Payment { Id=4, OwnerId=2, Description="Pago SOAT anual",
                Amount=280_000m, Currency="COP", Status="completed",
                RecipientName="Seguros del Estado", RecipientAccount="COL-00456789",
                CreatedAt=DateTime.UtcNow.AddDays(-30), ProcessedAt=DateTime.UtcNow.AddDays(-30) },

            new Payment { Id=5, OwnerId=2, Description="Bono incentivo trimestral",
                Amount=950_000m, Currency="COP", Status="completed",
                RecipientName="Laura Jiménez", RecipientAccount="BAN-00654321",
                CreatedAt=DateTime.UtcNow.AddDays(-3), ProcessedAt=DateTime.UtcNow.AddDays(-3) },

            new Payment { Id=6, OwnerId=2, Description="Cuota Plan Menor Toyota",
                Amount=780_000m, Currency="COP", Status="pending",
                RecipientName="Toyota Financial Services", RecipientAccount="TFS-00789013",
                CreatedAt=DateTime.UtcNow.AddHours(-4) }
        );

        db.SaveChanges();
    }
}
