using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext with InMemory database
builder.Services.AddDbContext<PrivatekonomyContext>(options =>
    options.UseInMemoryDatabase("PrivatekonomyDb"));

// Register services
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICsvImportService, CsvImportService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IDebtStrategyService, DebtStrategyService>();
builder.Services.AddScoped<IInvestmentService, InvestmentService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IBankSourceService, BankSourceService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IBankConnectionService, BankConnectionService>();
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<IExportService, ExportService>();

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

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PrivatekonomyContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazorWeb");
app.UseAuthorization();

// Map Aspire default endpoints (health checks, etc.)
app.MapDefaultEndpoints();

app.MapControllers();

app.Run();
