using System.Text.Json;

namespace TerminateGRInstance.Application;

public class EnvConfig
{
    public string TokenUrl { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string Scope { get; set; }
    public string GrUrl { get; set; }
    public string AppName { get; set; }
    public string AppCode { get; set; }
    public string StorageConnectionString { get; set; }
    public string TableName { get; set; }
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