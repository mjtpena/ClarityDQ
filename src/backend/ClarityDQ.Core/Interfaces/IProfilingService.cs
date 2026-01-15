using ClarityDQ.Core.Entities;

namespace ClarityDQ.Core.Interfaces;

public interface IProfilingService
{
    Task<Guid> ProfileTableAsync(string workspaceId, string datasetName, string tableName, CancellationToken cancellationToken = default);
    Task<DataProfile?> GetProfileAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<List<DataProfile>> GetProfilesAsync(string workspaceId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
}
