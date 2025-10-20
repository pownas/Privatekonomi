using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Services;
using Privatekonomi.Web.Components;
using Privatekonomi.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add MudBlazor services
builder.Services.AddMudServices();

// Configure DbContext with InMemory database
builder.Services.AddDbContext<PrivatekonomyContext>(options =>
    options.UseInMemoryDatabase("PrivatekonomyDb"));

// Register services
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IInvestmentService, InvestmentService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IBankSourceService, BankSourceService>();
builder.Services.AddScoped<IHouseholdService, HouseholdService>();
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<IBankConnectionService, BankConnectionService>();
builder.Services.AddScoped<ThemeService>();

// Add HttpClient for API calls (if needed later)
builder.Services.AddHttpClient();

// Register stock price service with HttpClient
builder.Services.AddHttpClient<IStockPriceService, YahooFinanceStockPriceService>();

// Register bank API services
// Note: In production, configure with actual credentials from secure storage
builder.Services.AddScoped<IBankApiService>(sp =>
{
    var context = sp.GetRequiredService<PrivatekonomyContext>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    
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

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PrivatekonomyContext>();
    context.Database.EnsureCreated();
    
    // Seed test data
    TestDataSeeder.SeedTestData(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

// Map Aspire default endpoints (health checks, etc.)
app.MapDefaultEndpoints();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
