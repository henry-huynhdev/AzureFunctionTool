using Azure.Data.Tables;
using TerminateGRInstance.Model;

namespace TerminateGRInstance.Application;

public static class AzureFunctionAppInstanceService
{
    public static async Task<string> GetAccessToken(string tokenUrl, string clientId, string clientSecret, string scope)
    {
        using var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);

        // Set request body
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_Id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("scope", scope),
        });
        request.Content = content;

        // Send request
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        // Parse response
        var responseBody = await response.Content.ReadAsStringAsync();
        var json = System.Text.Json.JsonDocument.Parse(responseBody);
        return json.RootElement.GetProperty("access_token").GetString() ?? throw new InvalidOperationException();
    }
    
    public static async Task<string> GetInstanceStatus(
        string grUrl,
        string appName,
        string appCode,
        string accessToken,
        string instanceId)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var request = new HttpRequestMessage(HttpMethod.Get,
            grUrl + $"/runtime/webhooks/durabletask/instances/{instanceId}"
                  + $"?taskHub={appName}&connection=Storage&code={appCode}");

        // Send request
        var response = await client.SendAsync(request);

        // Parse response
        var responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseBody);
        var instanceStatus = Newtonsoft.Json.JsonConvert.DeserializeObject<InstanceStatusOutDto>(responseBody);

        return instanceStatus?.RuntimeStatus ?? throw new InvalidOperationException();
    }
    
    public static async Task TerminateInstance(
        string grUrl,
        string appName,
        string appCode,
        string accessToken,
        string instanceId,
        string reason)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var request = new HttpRequestMessage(HttpMethod.Post,
            grUrl + $"/runtime/webhooks/durabletask/instances/{instanceId}/terminate"
                  + $"?reason={reason}&taskHub={appName}&connection=Storage&code={appCode}");

        // Send request
        var response = await client.SendAsync(request);

        // Parse response
        var responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseBody);
    }
    
    public static async Task RestartInstance(
        string grUrl,
        string appName,
        string appCode,
        string accessToken,
        string instanceId,
        string reason)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var request = new HttpRequestMessage(HttpMethod.Post,
            grUrl + $"/runtime/webhooks/durabletask/instances/{instanceId}/restart"
                  + $"?taskHub={appName}&connection=Storage&code={appCode}");

        // Send request
        var response = await client.SendAsync(request);

        // Parse response
        var responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseBody);
    }
    
    public static async Task SuspendInstance(
        string grUrl,
        string appName,
        string appCode,
        string accessToken,
        string instanceId,
        string reason)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var request = new HttpRequestMessage(HttpMethod.Post,
            grUrl + $"/runtime/webhooks/durabletask/instances/{instanceId}/suspend"
                  + $"?reason={reason}&taskHub={appName}&connection=Storage&code={appCode}");

        // Send request
        var response = await client.SendAsync(request);

        // Parse response
        var responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseBody);
    }
    
    public static async Task PurgeHistoryDelete(
        string grUrl,
        string appName,
        string appCode,
        string accessToken,
        string instanceId)
    {
        Console.WriteLine("PurgeHistoryDelete: " + instanceId);
        using var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(3600)
        };
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var request = new HttpRequestMessage(HttpMethod.Delete,
            grUrl + $"/runtime/webhooks/durabletask/instances/{instanceId}"
                  + $"?taskHub={appName}&connection=Storage&code={appCode}");

        // Send request
        var response = await client.SendAsync(request);

        // Parse response
        var responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine(instanceId + ": " + responseBody);
    }
    
    public static async Task CollectInformationAsync(string _storageConnectionString, string _tableName)
    {
        var listInstances = new List<Tuple<string, string>>();
        Console.WriteLine("Starting cleanup of failed Durable Function instances...");

        // Connect to the Table
        var tableClient = new TableClient(_storageConnectionString, _tableName);

        // Query all entities in the table
        await foreach (var entity in tableClient.QueryAsync<TableEntity>())
        {
            Console.WriteLine($"Processing InstanceId: {entity.PartitionKey}");
            listInstances.Add(new Tuple<string, string>(entity.PartitionKey,
                entity.GetString("RuntimeStatus") ?? "Unknown"));
        }

        // Print all instances and their statuses
        Console.WriteLine("Instance Statuses:");
        foreach (var instance in listInstances)
        {
            Console.WriteLine($"InstanceId: {instance.Item1}, Status: {instance.Item2}");
        }

        Console.WriteLine("----------------------------------------------------");

        // Group by instance status and count occurrences
        var groupedInstances = listInstances
            .GroupBy(x => x.Item2)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToList();
        Console.WriteLine("Instance Status Summary:");
        foreach (var group in groupedInstances)
        {
            Console.WriteLine($"Status: {group.Status}, Count: {group.Count}");
        }
    }
}