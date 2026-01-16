namespace ClarityDQ.FabricClient;

public class FabricClientOptions
{
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string FabricApiBaseUrl { get; set; } = "https://api.fabric.microsoft.com/v1";
}
