using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MiniEft.Domain.Entities;
using MiniEft.Domain.ValueObjects;

namespace MiniEft.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    private static readonly ValueConverter<Money, long> MoneyConverter = new(m => m.Amount, v => Money.Of(v));

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transfer> Transfers => Set<Transfer>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(b =>
        {
            b.ToTable("accounts", t => t.HasCheckConstraint("ck_accounts_balance_non_negative", "balance >= 0"));
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.AccountNumber).HasMaxLength(34).IsRequired();
            b.HasIndex(x => x.AccountNumber).IsUnique();
            b.Property(x => x.HolderName).HasMaxLength(200).IsRequired();
            b.Property(x => x.Currency).HasMaxLength(3).IsFixedLength().IsRequired();
            b.Property(x => x.Balance).HasConversion(MoneyConverter);
        });

        modelBuilder.Entity<Transfer>(b =>
        {
            b.ToTable("transfers", t =>
            {
                t.HasCheckConstraint("ck_transfers_amount_positive", "amount > 0");
                t.HasCheckConstraint("ck_transfers_different_accounts", "from_account_id <> to_account_id");
            });
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.IdempotencyKey).HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.IdempotencyKey).IsUnique();
            b.Property(x => x.Amount).HasConversion(MoneyConverter);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            b.Property(x => x.FailureReason).HasMaxLength(100);

            // Aggregates reference each other by id only (no navigation properties).
            b.HasOne<Account>().WithMany().HasForeignKey(x => x.FromAccountId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Account>().WithMany().HasForeignKey(x => x.ToAccountId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LedgerEntry>(b =>
        {
            b.ToTable("ledger_entries", t => t.HasCheckConstraint("ck_ledger_entries_amount_positive", "amount > 0"));
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedOnAdd();
            b.Property(x => x.Type).HasConversion<string>().HasMaxLength(10).IsRequired();
            b.Property(x => x.Kind).HasConversion<string>().HasMaxLength(10).IsRequired();
            b.Property(x => x.Amount).HasConversion(MoneyConverter);
            b.Property(x => x.BalanceAfter).HasConversion(MoneyConverter);

            b.HasIndex(x => new { x.AccountId, x.CreatedAt });
            b.HasIndex(x => x.TransferId);
            b.HasOne<Account>().WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Transfer>().WithMany().HasForeignKey(x => x.TransferId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
