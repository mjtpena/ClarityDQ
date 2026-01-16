using Azure.Storage.Files.DataLake;
using ClarityDQ.Core.Entities;

namespace ClarityDQ.OneLake;

public interface IOneLakeService
{
    Task WriteProfilingResultAsync(string workspaceId, string datasetName, DataProfile result, CancellationToken cancellationToken = default);
    Task WriteRuleExecutionAsync(string workspaceId, Guid ruleId, RuleExecution execution, CancellationToken cancellationToken = default);
    Task<List<DataProfile>> ReadProfilingResultsAsync(string workspaceId, string datasetName, DateTime? since = null, CancellationToken cancellationToken = default);
}

public class OneLakeService : IOneLakeService
{
    private readonly DataLakeServiceClient _serviceClient;
    private readonly string _filesystemName;

    public OneLakeService(string connectionString, string filesystemName = "claritydq-results")
    {
        _serviceClient = new DataLakeServiceClient(connectionString);
        _filesystemName = filesystemName;
    }

    public async Task WriteProfilingResultAsync(string workspaceId, string datasetName, DataProfile result, CancellationToken cancellationToken = default)
    {
        var fileSystemClient = _serviceClient.GetFileSystemClient(_filesystemName);
        await fileSystemClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var directoryPath = $"profiling/{workspaceId}/{datasetName}";
        var directoryClient = fileSystemClient.GetDirectoryClient(directoryPath);
        await directoryClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var fileName = $"{result.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}.json";
        var fileClient = directoryClient.GetFileClient(fileName);

        var json = System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        var data = System.Text.Encoding.UTF8.GetBytes(json);
        using var stream = new MemoryStream(data);
        
        await fileClient.UploadAsync(stream, overwrite: true, cancellationToken: cancellationToken);
    }

    public async Task WriteRuleExecutionAsync(string workspaceId, Guid ruleId, RuleExecution execution, CancellationToken cancellationToken = default)
    {
        var fileSystemClient = _serviceClient.GetFileSystemClient(_filesystemName);
        await fileSystemClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var directoryPath = $"rule-executions/{workspaceId}/{ruleId}";
        var directoryClient = fileSystemClient.GetDirectoryClient(directoryPath);
        await directoryClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var fileName = $"{execution.Id}_{execution.ExecutedAt:yyyyMMddHHmmss}.json";
        var fileClient = directoryClient.GetFileClient(fileName);

        var json = System.Text.Json.JsonSerializer.Serialize(execution, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        var data = System.Text.Encoding.UTF8.GetBytes(json);
        using var stream = new MemoryStream(data);
        
        await fileClient.UploadAsync(stream, overwrite: true, cancellationToken: cancellationToken);
    }

    public async Task<List<DataProfile>> ReadProfilingResultsAsync(string workspaceId, string datasetName, DateTime? since = null, CancellationToken cancellationToken = default)
    {
        var results = new List<DataProfile>();
        
        var fileSystemClient = _serviceClient.GetFileSystemClient(_filesystemName);
        if (!await fileSystemClient.ExistsAsync(cancellationToken))
        {
            return results;
        }

        var directoryPath = $"profiling/{workspaceId}/{datasetName}";
        var directoryClient = fileSystemClient.GetDirectoryClient(directoryPath);
        
        if (!await directoryClient.ExistsAsync(cancellationToken))
        {
            return results;
        }

        await foreach (var pathItem in directoryClient.GetPathsAsync(cancellationToken: cancellationToken))
        {
            if (pathItem.IsDirectory == true) continue;

            var fileClient = directoryClient.GetFileClient(pathItem.Name);
            var response = await fileClient.ReadAsync(cancellationToken: cancellationToken);
            
            using var reader = new StreamReader(response.Value.Content);
            var json = await reader.ReadToEndAsync(cancellationToken);
            
            var result = System.Text.Json.JsonSerializer.Deserialize<DataProfile>(json);
            if (result != null && (!since.HasValue || result.ProfiledAt >= since.Value))
            {
                results.Add(result);
            }
        }

        return results.OrderByDescending(r => r.ProfiledAt).ToList();
    }
}
