using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Services;
using Privatekonomi.Api.Middleware;
using Privatekonomi.Core.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Configure ProblemDetails
builder.Services.AddProblemDetails();

// Register global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure storage based on appsettings
builder.Services.AddPrivatekonomyStorage(builder.Configuration);

// Register services
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRuleService, CategoryRuleService>();
builder.Services.AddScoped<ICsvImportService, CsvImportService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IDebtStrategyService, DebtStrategyService>();
builder.Services.AddScoped<IInvestmentService, InvestmentService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IBankSourceService, BankSourceService>();
builder.Services.AddScoped<IHouseholdService, HouseholdService>();
builder.Services.AddScoped<IChildAllowanceService, ChildAllowanceService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IBankConnectionService, BankConnectionService>();
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<IPocketService, PocketService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ISalaryHistoryService, SalaryHistoryService>();

// Register security services for bank API
builder.Services.AddMemoryCache();
builder.Services.AddDataProtection();
builder.Services.AddSingleton<ITokenEncryptionService, Privatekonomi.Core.Services.TokenEncryptionService>();
builder.Services.AddSingleton<IOAuthStateService, Privatekonomi.Core.Services.OAuthStateService>();

// Register bank sync background service
builder.Services.Configure<Privatekonomi.Core.Services.BankSyncSettings>(
    builder.Configuration.GetSection("BankSync"));
builder.Services.AddHostedService<Privatekonomi.Core.Services.BankSyncBackgroundService>();

// Register HttpClient for bank API services
builder.Services.AddHttpClient();

// Register bank API services
// Note: In production, these should be configured with actual client credentials from configuration
builder.Services.AddScoped<IBankApiService>(sp =>
{
    var context = sp.GetRequiredService<PrivatekonomyContext>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    
    // For demo purposes, using placeholder credentials
    // In production, load from secure configuration (Azure Key Vault, etc.)
    var clientId = builder.Configuration["Swedbank:ClientId"] ?? "demo-client-id";
    var clientSecret = builder.Configuration["Swedbank:ClientSecret"] ?? "demo-client-secret";
    
    return new Privatekonomi.Core.Services.BankApi.SwedbankApiService(context, httpClient, clientId, clientSecret);
});

builder.Services.AddScoped<IBankApiService>(sp =>
{
    var context = sp.GetRequiredService<PrivatekonomyContext>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    
    return new Privatekonomi.Core.Services.BankApi.AvanzaApiService(context, httpClient);
});

builder.Services.AddScoped<IBankApiService>(sp =>
{
    var context = sp.GetRequiredService<PrivatekonomyContext>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    
    var clientId = builder.Configuration["IcaBanken:ClientId"] ?? "demo-client-id";
    var clientSecret = builder.Configuration["IcaBanken:ClientSecret"] ?? "demo-client-secret";
    
    return new Privatekonomi.Core.Services.BankApi.IcaBankenApiService(context, httpClient, clientId, clientSecret);
});

// Configure CORS for Blazor Web
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorWeb", policy =>
    {
        policy.WithOrigins("https://localhost:5001", "http://localhost:5000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var storageSettings = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<StorageSettings>>().Value;
    var context = scope.ServiceProvider.GetRequiredService<PrivatekonomyContext>();
    
    // For SQLite, ensure database is created
    if (storageSettings.Provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        logger.LogInformation("Using SQLite storage, ensuring database is created...");
        context.Database.EnsureCreated();
    }
    else
    {
        context.Database.EnsureCreated();
    }
    
    logger.LogInformation("Database initialized with provider: {Provider}", storageSettings.Provider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add exception handler middleware
app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseHttpsRedirection();
app.UseCors("AllowBlazorWeb");
app.UseAuthorization();

// Map Aspire default endpoints (health checks, etc.)
app.MapDefaultEndpoints();

app.MapControllers();

app.Run();

// Make the implicit Program class public for testing
public partial class Program { }

