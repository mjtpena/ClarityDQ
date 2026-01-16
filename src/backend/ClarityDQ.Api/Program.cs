using ClarityDQ.Core.Interfaces;
using ClarityDQ.Infrastructure.Data;
using ClarityDQ.Profiling.Services;
using ClarityDQ.Api.BackgroundJobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Hangfire;
using Hangfire.SqlServer;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddScoped<ScheduledJobProcessor>();

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

app.Run();
