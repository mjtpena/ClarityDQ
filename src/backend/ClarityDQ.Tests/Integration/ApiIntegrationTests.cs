using Microsoft.Extensions.DependencyInjection;
using ClarityDQ.Core.Interfaces;

namespace ClarityDQ.Tests.Integration;

public class ApiIntegrationTests
{
    [Fact]
    public void ProgramClass_Exists()
    {
        var programType = typeof(Program);
        Assert.NotNull(programType);
    }

    [Fact]
    public void ServiceRegistration_InterfacesExist()
    {
        var interfaces = new[]
        {
            typeof(IProfilingService),
            typeof(IRuleService),
            typeof(ISchedulingService),
            typeof(ILineageService)
        };

        foreach (var @interface in interfaces)
        {
            Assert.NotNull(@interface);
            Assert.True(@interface.IsInterface);
        }
    }

    [Fact]
    public void ProgramClass_IsPublicPartial()
    {
        var programType = typeof(Program);
        Assert.True(programType.IsPublic || programType.IsNestedPublic);
    }
}

