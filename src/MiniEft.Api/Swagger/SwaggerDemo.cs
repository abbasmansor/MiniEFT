using System.Text;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using MiniEft.Api.Dtos;
using MiniEft.Infrastructure.Persistence;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MiniEft.Api.Swagger;

/// <summary>Makes the API testable from Swagger alone: lists the seeded demo accounts and pre-fills the transfer body.</summary>
public static class SwaggerDemo
{
    /// <summary>Text shown at the top of the Swagger page (built from the same list the seeder uses).</summary>
    public static string Description()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"**Demo accounts** ({DbInitializer.OpeningBalance:N0} IQD each):");
        sb.AppendLine();
        foreach (var (id, number, holder) in DbInitializer.DemoAccounts)
            sb.AppendLine($"- `{id}` {number} {holder}");
        return sb.ToString();
    }

    /// <summary>Pre-fills the request body example with two real demo accounts.</summary>
    public sealed class CreateTransferRequestExample : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type != typeof(CreateTransferRequest)) return;
            schema.Example = new OpenApiObject
            {
                ["fromAccountId"] = new OpenApiString(DbInitializer.DemoAccounts[0].Id.ToString()),
                ["toAccountId"] = new OpenApiString(DbInitializer.DemoAccounts[1].Id.ToString()),
                ["amount"] = new OpenApiLong(250_000)
            };
        }
    }
}
