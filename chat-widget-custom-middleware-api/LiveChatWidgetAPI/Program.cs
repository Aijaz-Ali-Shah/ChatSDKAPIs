using Azure.Identity;
using LiveChatWidgetAPI.Middleware;
using LiveChatWidgetAPI.Services;

var builder = WebApplication.CreateBuilder(args);

const string localEnvironment = "Local";
var isLocal = builder.Environment.IsEnvironment(localEnvironment);

if (!isLocal)
{
    var vaultUri = builder.Configuration["KeyVault:VaultUri"];
    if (string.IsNullOrWhiteSpace(vaultUri))
    {
        throw new InvalidOperationException(
            "KeyVault:VaultUri is required when ASPNETCORE_ENVIRONMENT is not Local. " +
            "On the App Service, set KeyVault__VaultUri to https://<vault-name>.vault.azure.net/.");
    }

    builder.Configuration.AddAzureKeyVault(
        new Uri(vaultUri.Trim()),
        new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            ExcludeInteractiveBrowserCredential = true
        }));
}

string[] requiredKeys =
[
    "AzureAd:TenantId",
    "AzureAd:ClientId",
    "AzureAd:ClientSecret",
    "AzureAd:Scope",
    "D365:BaseUrl"
];

var missingKeys = requiredKeys
    .Where(key => string.IsNullOrWhiteSpace(builder.Configuration[key]))
    .ToArray();

if (missingKeys.Length > 0)
{
    var source = isLocal
        ? "appsettings.json"
        : "Key Vault. Secret names use -- in place of :, for example AzureAd--ClientSecret";

    throw new InvalidOperationException(
        $"Missing configuration values: {string.Join(", ", missingKeys)}. Provide them in {source}.");
}

var appInsightsConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
if (string.IsNullOrWhiteSpace(appInsightsConnectionString))
{
    if (!isLocal)
    {
        throw new InvalidOperationException(
            "Missing ApplicationInsights:ConnectionString. " +
            "Add the Key Vault secret ApplicationInsights--ConnectionString.");
    }
}
else
{
    builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);
}


builder.Services.AddCors(options =>
{
    options.AddPolicy("RestrictedCors", policy =>
    {
        policy.WithOrigins(

            "http://localhost:5173",

 "http://localhost:5175",
                "https://purple-wave-0ccea6f03.5.azurestaticapps.net",
                "https://purple-wave-0ccea6f03.5.azurestaticapps.net/",
                "https://appfe-obotp1-dev-live-chat-widget-mw-aadfdnf9baaxc4ag.swedencentral-01.azurewebsites.net"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTokenAccessor, CurrentTokenAccessor>();
builder.Services.AddHttpClient<AzureAdTokenService>();

builder.Services.AddHttpClient<D365Service>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["correlationId"] = context.HttpContext.TraceIdentifier;
    };
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.Logger.LogInformation(
    "Environment {EnvironmentName}. Secrets are loaded from {SecretsSource}.",
    app.Environment.EnvironmentName,
    isLocal ? "appsettings.json" : "Azure Key Vault");

// Swagger stays available for Local, Development, and Uat.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "LiveChatWidgetAPI v1");
    options.RoutePrefix = "swagger";
});

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseCors("RestrictedCors");
app.UseAuthorization();
app.UseMiddleware<TokenAuthMiddleware>();
app.MapControllers();

app.Run();
