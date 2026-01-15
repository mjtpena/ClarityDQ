using ClarityDQ.Core.Entities;
using ClarityDQ.Core.Interfaces;
using ClarityDQ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ClarityDQ.Profiling.Services;

public class ProfilingService : IProfilingService
{
    private readonly ClarityDbContext _context;

    public ProfilingService(ClarityDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> ProfileTableAsync(string workspaceId, string datasetName, string tableName, CancellationToken cancellationToken = default)
    {
        var profile = new DataProfile
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            DatasetName = datasetName,
            TableName = tableName,
            ProfiledAt = DateTime.UtcNow,
            Status = ProfileStatus.Pending
        };

        _context.DataProfiles.Add(profile);
        await _context.SaveChangesAsync(cancellationToken);

        // Trigger async profiling job (placeholder)
        _ = Task.Run(async () => await ExecuteProfilingAsync(profile.Id), cancellationToken);

        return profile.Id;
    }

    public async Task<DataProfile?> GetProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        return await _context.DataProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == profileId, cancellationToken);
    }

    public async Task<List<DataProfile>> GetProfilesAsync(string workspaceId, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
    {
        return await _context.DataProfiles
            .AsNoTracking()
            .Where(p => p.WorkspaceId == workspaceId)
            .OrderByDescending(p => p.ProfiledAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    private async Task ExecuteProfilingAsync(Guid profileId)
    {
        var profile = await _context.DataProfiles.FindAsync(profileId);
        if (profile == null) return;

        try
        {
            profile.Status = ProfileStatus.InProgress;
            await _context.SaveChangesAsync();

            // Simulate profiling logic
            await Task.Delay(2000);

            var mockData = new
            {
                Columns = new[]
                {
                    new { ColumnName = "Id", DataType = "int", NonNullCount = 1000, NullCount = 0, DistinctCount = 1000, CompletenessPercent = 100.0 },
                    new { ColumnName = "Name", DataType = "string", NonNullCount = 980, NullCount = 20, DistinctCount = 950, CompletenessPercent = 98.0 }
                }
            };

            profile.RowCount = 1000;
            profile.ColumnCount = 2;
            profile.SizeInBytes = 50000;
            profile.ProfileData = JsonSerializer.Serialize(mockData);
            profile.Status = ProfileStatus.Completed;
            profile.ProfiledAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            profile.Status = ProfileStatus.Failed;
            profile.ErrorMessage = ex.Message;
            await _context.SaveChangesAsync();
        }
    }
}
