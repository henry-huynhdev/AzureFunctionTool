using System.Text.Json;

namespace TerminateGRInstance.Application;

public class EnvConfig
{
    public required string TokenUrl { get; set; }
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required string Scope { get; set; }
    public required string GrUrl { get; set; }
    public required string AppName { get; set; }
    public required string AppCode { get; set; }
    public required string StorageConnectionString { get; set; }
    public required string TableName { get; set; }
}

public static class ConfigLoader
{
    public static EnvConfig LoadEnvConfig(string configPath = "appsettings.json")
    {
        var json = File.ReadAllText(configPath);
        var envConfig = JsonSerializer.Deserialize<EnvConfig>(json);
        if (envConfig == null)
            throw new Exception("Invalid configuration file.");
        return envConfig;
    }
}
