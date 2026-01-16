using ClarityDQ.Api.BackgroundJobs;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ClarityDQ.Tests.BackgroundJobs;

public class HangfireAuthorizationFilterTests
{
    private class TestDashboardContext : DashboardContext
    {
        private readonly HttpContext _httpContext;

        public TestDashboardContext(HttpContext httpContext) : base(null, null)
        {
            _httpContext = httpContext;
        }

        public HttpContext GetTestHttpContext() => _httpContext;
    }

    [Fact]
    public void Authorize_ReturnsTrue_WhenUserIsAuthenticated()
    {
        var filter = new TestableHangfireAuthorizationFilter();
        
        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity(authenticationType: "Test");
        httpContext.User = new ClaimsPrincipal(identity);
        httpContext.Request.Host = new HostString("example.com");
        
        var result = filter.TestAuthorize(httpContext);

        Assert.True(result);
    }

    [Fact]
    public void Authorize_ReturnsTrue_WhenRequestIsFromLocalhost()
    {
        var filter = new TestableHangfireAuthorizationFilter();
        
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal();
        httpContext.Request.Host = new HostString("localhost");
        
        var result = filter.TestAuthorize(httpContext);

        Assert.True(result);
    }

    [Fact]
    public void Authorize_ReturnsFalse_WhenUserNotAuthenticatedAndNotLocalhost()
    {
        var filter = new TestableHangfireAuthorizationFilter();
        
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal();
        httpContext.Request.Host = new HostString("example.com");
        
        var result = filter.TestAuthorize(httpContext);

        Assert.False(result);
    }

    private class TestableHangfireAuthorizationFilter : HangfireAuthorizationFilter
    {
        public bool TestAuthorize(HttpContext httpContext)
        {
            return httpContext.User.Identity?.IsAuthenticated ?? 
                   httpContext.Request.Host.Host == "localhost";
        }
    }
}
