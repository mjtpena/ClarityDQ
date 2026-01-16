using ClarityDQ.Core.Interfaces;
using ClarityDQ.Infrastructure.Data;
using ClarityDQ.Profiling.Services;
using ClarityDQ.Lineage.Services;
using ClarityDQ.RuleEngine;
using ClarityDQ.Api.BackgroundJobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Hangfire;
using Hangfire.SqlServer;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ClarityDQ")
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting ClarityDQ API");

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ClarityDQ")
    .WriteTo.Console()
    .WriteTo.ApplicationInsights(
        context.Configuration["ApplicationInsights:ConnectionString"],
        TelemetryConverter.Traces));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();

builder.Services.AddDbContext<ClarityDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

builder.Services.AddHangfireServer();

builder.Services.AddScoped<IProfilingService, ProfilingService>();
builder.Services.AddScoped<IRuleService, RuleService>();
builder.Services.AddScoped<ISchedulingService, SchedulingService>();
builder.Services.AddScoped<ILineageService, LineageService>();
builder.Services.AddScoped<IRuleExecutor, RuleExecutor>();
builder.Services.AddScoped<IRuleDataSource, MockRuleDataSource>();
builder.Services.AddScoped<ScheduledJobProcessor>();

builder.Services.AddScoped<ClarityDQ.FabricClient.IFabricClient>(sp =>
{
    var options = new ClarityDQ.FabricClient.FabricClientOptions
    {
        TenantId = builder.Configuration["Fabric:TenantId"] ?? "",
        ClientId = builder.Configuration["Fabric:ClientId"] ?? "",
        ClientSecret = builder.Configuration["Fabric:ClientSecret"] ?? "",
        FabricApiBaseUrl = builder.Configuration["Fabric:ApiBaseUrl"] ?? "https://api.fabric.microsoft.com/v1"
    };
    return new ClarityDQ.FabricClient.FabricClient(new HttpClient(), options);
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? "")
    .AddCheck("api", () => HealthCheckResult.Healthy("API is running"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            })
        });
        await context.Response.WriteAsync(result);
    }
});

RecurringJob.AddOrUpdate<ScheduledJobProcessor>(
    "process-scheduled-jobs",
    processor => processor.ProcessDueSchedules(),
    "*/5 * * * *");

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
    };
});

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
