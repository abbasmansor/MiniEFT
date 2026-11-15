using Microsoft.Extensions.DependencyInjection;
using MiniEft.Application.Accounts;
using MiniEft.Application.Transfers;

namespace MiniEft.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITransferService, TransferService>();
        services.AddScoped<IStatementService, StatementService>();
        return services;
    }
}
