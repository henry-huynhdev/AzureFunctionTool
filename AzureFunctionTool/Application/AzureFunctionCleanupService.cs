using Azure.Data.Tables;

namespace TerminateGRInstance.Application;

public class AzureFunctionCleanupService
{
    private readonly EnvConfig _config;

    public AzureFunctionCleanupService(EnvConfig config)
    {
        _config = config;
    }

    public async Task CleanupInstancesAsync(List<string> instanceStatuses)
    {
        var listInstances = new List<string>();
        Console.WriteLine("Starting cleanup of Durable Function instances...");

        // Connect to the Table
        var tableClient = new TableClient(_config.StorageConnectionString, _config.TableName);

        // Query all entities in the table
        await foreach (var entity in tableClient.QueryAsync<TableEntity>())
        {
            var status = entity.GetString("RuntimeStatus");
            var timeStamp = entity.GetDateTimeOffset("Timestamp");
            var isOlderThan30Days = timeStamp < DateTime.UtcNow.AddDays(-30);
            Console.WriteLine($"Processing InstanceId: {entity.PartitionKey}, Status: {status}, Timestamp: {timeStamp}");
            
            // Check if RunTime Status is null
            if (isOlderThan30Days
                && !string.IsNullOrEmpty(entity.GetString("RuntimeStatus"))
                && instanceStatuses.Contains(entity.GetString("RuntimeStatus")))
            {
                listInstances.Add(entity.PartitionKey);
            }
        }

        if (listInstances.Count == 0)
        {
            Console.WriteLine("No instances found with the specified statuses.");
            return;
        }

        // Get access token
        var accessToken = await AzureFunctionAppInstanceService.GetAccessToken(
            _config.TokenUrl, _config.ClientId, _config.ClientSecret, _config.Scope);

        foreach (var instance in listInstances)
        {
            Console.WriteLine($"InstanceId: {instance}");
            // Get instance status
            var status = await AzureFunctionAppInstanceService.GetInstanceStatus(
                _config.GrUrl,
                _config.AppName,
                _config.AppCode,
                accessToken, 
                instance);
            Console.WriteLine($"Status: {status}");


            if (instanceStatuses.Contains(status))
            {
                await AzureFunctionAppInstanceService.PurgeHistoryDelete(
                    _config.GrUrl,
                    _config.AppName,
                    _config.AppCode,
                    accessToken,
                    instance);
            }
        }
    }

    public async Task CollectInformationAsync()
    {
        await AzureFunctionAppInstanceService.CollectInformationAsync(_config.StorageConnectionString, _config.TableName);
    }
}

