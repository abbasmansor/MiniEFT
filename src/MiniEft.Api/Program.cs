using MiniEft.Api.Errors;
using MiniEft.Api.Swagger;
using MiniEft.Application;
using MiniEft.Infrastructure;
using MiniEft.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new() { Title = "Mini EFT Transfer API", Version = "v1", Description = SwaggerDemo.Description() });
    o.SchemaFilter<SwaggerDemo.CreateTransferRequestExample>();
    var xml = Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
    if (File.Exists(xml)) o.IncludeXmlComments(xml);
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

// Apply migrations and seed the demo accounts on startup.
await DbInitializer.MigrateAndSeedAsync(app.Services);

app.Run();
