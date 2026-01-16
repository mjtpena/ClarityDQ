using ClarityDQ.Api.BackgroundJobs;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using System.Security.Principal;

namespace ClarityDQ.Tests.BackgroundJobs;

public class HangfireAuthorizationFilterTests
{
    private readonly HangfireAuthorizationFilter _filter;

    public HangfireAuthorizationFilterTests()
    {
        _filter = new HangfireAuthorizationFilter();
    }

    [Fact]
    public void Authorize_ReturnsTrue_WhenUserIsAuthenticated()
    {
        var identity = new Mock<IIdentity>();
        identity.Setup(i => i.IsAuthenticated).Returns(true);

        var user = new ClaimsPrincipal(identity.Object);
        
        var httpContext = new DefaultHttpContext
        {
            User = user,
            Request = { Host = new HostString("example.com") }
        };

        var dashboardContext = new Mock<DashboardContext>();
        dashboardContext.Setup(c => c.GetHttpContext()).Returns(httpContext);

        var result = _filter.Authorize(dashboardContext.Object);

        Assert.True(result);
    }

    [Fact]
    public void Authorize_ReturnsTrue_WhenRequestIsFromLocalhost()
    {
        var identity = new Mock<IIdentity>();
        identity.Setup(i => i.IsAuthenticated).Returns(false);

        var user = new ClaimsPrincipal(identity.Object);
        
        var httpContext = new DefaultHttpContext
        {
            User = user,
            Request = { Host = new HostString("localhost") }
        };

        var dashboardContext = new Mock<DashboardContext>();
        dashboardContext.Setup(c => c.GetHttpContext()).Returns(httpContext);

        var result = _filter.Authorize(dashboardContext.Object);

        Assert.True(result);
    }

    [Fact]
    public void Authorize_ReturnsFalse_WhenUserNotAuthenticatedAndNotLocalhost()
    {
        var identity = new Mock<IIdentity>();
        identity.Setup(i => i.IsAuthenticated).Returns(false);

        var user = new ClaimsPrincipal(identity.Object);
        
        var httpContext = new DefaultHttpContext
        {
            User = user,
            Request = { Host = new HostString("example.com") }
        };

        var dashboardContext = new Mock<DashboardContext>();
        dashboardContext.Setup(c => c.GetHttpContext()).Returns(httpContext);

        var result = _filter.Authorize(dashboardContext.Object);

        Assert.False(result);
    }
}
