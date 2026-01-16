using Hangfire.Dashboard;

namespace ClarityDQ.Api.BackgroundJobs;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return context.GetHttpContext().User.Identity?.IsAuthenticated ?? 
               context.GetHttpContext().Request.Host.Host == "localhost";
    }
}
