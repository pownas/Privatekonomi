using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using Privatekonomi.Web.Components;
using Privatekonomi.Web.Components.Account;
using Privatekonomi.Web.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Configure Swedish culture as default
var swedishCulture = new CultureInfo("sv-SE");
CultureInfo.DefaultThreadCurrentCulture = swedishCulture;
CultureInfo.DefaultThreadCurrentUICulture = swedishCulture;

// Add Aspire service defaults
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure Blazor Server options for better error handling
builder.Services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.DetailedErrors = true;
    }
});

// Add MudBlazor services with configuration for better accessibility
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 10000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
});

// Configure DbContext with InMemory database
builder.Services.AddDbContext<PrivatekonomyContext>(options =>
    options.UseInMemoryDatabase("PrivatekonomyDb"));

// Add Identity services
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();
builder.Services.AddAuthorizationBuilder();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<PrivatekonomyContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, NoOpEmailSender>();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// Add HttpContextAccessor for CurrentUserService
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Register services
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRuleService, CategoryRuleService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IDebtStrategyService, DebtStrategyService>();
builder.Services.AddScoped<IInvestmentService, InvestmentService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IBankSourceService, BankSourceService>();
builder.Services.AddScoped<IHouseholdService, HouseholdService>();
builder.Services.AddScoped<IChildAllowanceService, ChildAllowanceService>();
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<IPocketService, PocketService>();
builder.Services.AddScoped<ISharedGoalService, SharedGoalService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IBankConnectionService, BankConnectionService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ISalaryHistoryService, SalaryHistoryService>();
builder.Services.AddScoped<ThemeService>();

// Swedish-specific services
builder.Services.AddScoped<ISieExporter, SieExporter>();
builder.Services.AddScoped<IK4Generator, K4Generator>();
builder.Services.AddScoped<ITaxDeductionService, TaxDeductionService>();
builder.Services.AddScoped<IISKTaxCalculator, ISKTaxCalculator>();

// Add HttpClient for API calls (if needed later) using Aspire service discovery
builder.Services.AddHttpClient("api", client =>
{
    // Aspire will configure this automatically through service discovery

    // // This will be configured by Aspire service discovery
    // client.BaseAddress = new Uri(builder.Configuration["services:api:http:0"] ?? "http://localhost:5001");
});

// Configure default HttpClient 
builder.Services.AddScoped<HttpClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    return httpClientFactory.CreateClient();
    //// to use the API base address
    //return httpClientFactory.CreateClient("api");
});

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

// Configure request localization for Swedish
var supportedCultures = new[] { new CultureInfo("sv-SE") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("sv-SE"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// Seed the database
try
{
    using (var scope = app.Services.CreateScope())
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Starting database seeding...");
        
        var context = scope.ServiceProvider.GetRequiredService<PrivatekonomyContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        context.Database.EnsureCreated();
        
        // Seed test data
        await TestDataSeeder.SeedTestDataAsync(context, userManager);
        
        logger.LogInformation("Database seeding completed successfully");
    }
}
catch (Exception ex)
{
    // Log the error but don't crash the application
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while seeding the database. Application will continue without test data.");
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

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add Identity endpoints
app.MapGroup("/Account").MapIdentityApi<ApplicationUser>();

app.Run();
