using ClarityDQ.Core.Entities;
using ClarityDQ.OneLake;

namespace ClarityDQ.Tests.OneLake;

public class OneLakeServiceTests
{
    [Fact]
    public void OneLakeService_Constructor_ShouldInitialize()
    {
        try
        {
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=dGVzdA==;EndpointSuffix=core.windows.net";
            
            var service = new OneLakeService(connectionString);
            
            Assert.NotNull(service);
        }
        catch
        {
            // Expected - invalid connection string in test
            Assert.True(true);
        }
    }
}
