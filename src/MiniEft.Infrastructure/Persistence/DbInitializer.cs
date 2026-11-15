using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniEft.Domain.Entities;
using MiniEft.Domain.ValueObjects;

namespace MiniEft.Infrastructure.Persistence;

public static class DbInitializer
{
    /// <summary>Well-known demo accounts (fixed ids so they can be used straight from Swagger / curl).</summary>
    public static readonly (Guid Id, string Number, string Holder)[] DemoAccounts =
    [
        (Guid.Parse("11111111-1111-1111-1111-111111111111"), "IQ-0001", "Ali Hassan"),
        (Guid.Parse("22222222-2222-2222-2222-222222222222"), "IQ-0002", "Zainab Kareem"),
        (Guid.Parse("33333333-3333-3333-3333-333333333333"), "IQ-0003", "Omar Saleh"),
        (Guid.Parse("44444444-4444-4444-4444-444444444444"), "IQ-0004", "Noor Jabbar")
    ];

    public const long OpeningBalance = 1_000_000;

    public static async Task MigrateAndSeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync(ct);

        foreach (var (id, number, holder) in DemoAccounts)
        {
            if (await db.Accounts.AnyAsync(a => a.Id == id, ct)) continue;
            db.Accounts.Add(Account.Open(id, number, holder, Money.Of(OpeningBalance), DateTimeOffset.UtcNow));
        }
        await db.SaveChangesAsync(ct);
    }
}
