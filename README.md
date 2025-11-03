# Azure Function App Tool

A .NET console application for managing and cleaning up Azure Durable Function instances. This tool allows you to collect information about function instances and clean up instances based on their runtime status.

## Features

- **Collect Information**: Query and display information about Azure Durable Function instances stored in Azure Table Storage
- **Cleanup Instances**: Purge instances older than 30 days based on their runtime status (Failed, Completed, Running)
- **Interactive CLI**: User-friendly command-line interface with confirmation prompts for destructive operations
- **Flexible Status Selection**: Choose specific statuses to target for cleanup operations

## Prerequisites

- .NET 8.0 SDK or later
- Azure Storage Account with Table Storage
- Azure Function App with Durable Functions
- Azure AD application credentials (Client ID, Client Secret) with appropriate permissions

## Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd AzureFunctionAppTool
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build the project:
```bash
dotnet build
```

## Configuration

Before running the application, you need to configure the `appsettings.json` file located in the `AzureFunctionTool` directory.

### Configuration File Structure

```json
{
    "TokenUrl": "https://login.microsoftonline.com/{tenant-id}/oauth2/v2.0/token",
    "ClientId": "your-azure-ad-client-id",
    "ClientSecret": "your-azure-ad-client-secret",
    "Scope": "https://management.azure.com/.default",
    "GrUrl": "https://your-function-app.azurewebsites.net",
    "AppName": "YourFunctionAppName",
    "AppCode": "your-function-app-key",
    "StorageConnectionString": "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net",
    "TableName": "DurableFunctionsHubInstances"
}
```

### Configuration Parameters

| Parameter | Description |
|-----------|-------------|
| `TokenUrl` | Azure AD OAuth 2.0 token endpoint URL |
| `ClientId` | Azure AD application (client) ID |
| `ClientSecret` | Azure AD application client secret |
| `Scope` | OAuth scope for accessing Azure resources |
| `GrUrl` | Base URL of your Azure Function App |
| `AppName` | Name of your Function App |
| `AppCode` | Function App master key or host key |
| `StorageConnectionString` | Connection string to Azure Storage Account |
| `TableName` | Name of the table storing Durable Function instances (typically `DurableFunctionsHubInstances`) |

## Usage

### Running the Application

Navigate to the project directory and run:

```bash
dotnet run --project AzureFunctionTool
```

Or run the compiled executable:

```bash
cd AzureFunctionTool/bin/Debug/net8.0
./AzureFunctionTool
```

### Workflow

1. **Start the application**
   - The application will display the service name and present action options

2. **Select an action**
   ```
   Select action:
   1. Collect Information
   2. Cleanup Instances
   Enter your choice (1-2):
   ```

3. **If you choose "1. Collect Information":**
   - The tool will query Azure Table Storage
   - Display information about all Durable Function instances
   - Show statistics about instance statuses

4. **If you choose "2. Cleanup Instances":**
   
   a. **Select statuses to cleanup:**
   ```
   Select instance statuses to cleanup:
   1. Failed
   2. Completed
   3. Running
   
   Enter status numbers to cleanup (comma-separated, e.g., 1,2):
   ```
   
   b. **Review selection:**
   - The tool displays the selected statuses
   
   c. **Confirm the operation:**
   ```
   ⚠️  WARNING: This action CANNOT be undone!
   ⚠️  All instances with the selected statuses will be permanently deleted.
   
   Are you sure you want to continue? (yes/no):
   ```
   
   d. **Cleanup process:**
   - Only instances **older than 30 days** with the selected statuses will be deleted
   - The tool will display progress for each instance being processed

5. **Completion**
   - A summary message will be displayed when processing is complete

## Instance Statuses

The tool recognizes the following Durable Function instance statuses:

- **Failed**: Instances that have failed during execution
- **Completed**: Instances that have successfully completed
- **Running**: Instances that are currently running

## Safety Features

- **Age Filter**: Only instances older than 30 days are eligible for cleanup
- **Confirmation Prompt**: Requires explicit confirmation before performing destructive operations
- **Status Selection**: Allows granular control over which instances to clean up
- **Preview Mode**: Use "Collect Information" to review instances before cleanup

## Project Structure

```
AzureFunctionTool/
├── Program.cs                          # Main entry point with CLI logic
├── appsettings.json                    # Configuration file
├── Application/
│   ├── AzureFunctionCleanupService.cs  # Core cleanup logic
│   ├── AzureFunctionAppInstanceService.cs  # Azure Function API interactions
│   └── ConfigLoader.cs                 # Configuration loader
├── Enum/
│   └── InstanceStatus.cs               # Instance status enumeration
└── Model/
    └── InstanceStatusOutDto.cs         # Data transfer objects
```

## Dependencies

- **Azure.Data.Tables** (v12.10.0): For Azure Table Storage operations
- **Azure.Storage.Blobs** (v12.24.0): For Azure Blob Storage operations
- **Microsoft.Azure.WebJobs.Extensions.DurableTask** (v3.0.4): For Durable Functions support

## Security Considerations

⚠️ **Important Security Notes:**

- Never commit `appsettings.json` with real credentials to version control
- Use environment variables or Azure Key Vault for production deployments
- Ensure the Azure AD application has minimum required permissions
- Restrict access to the Function App keys
- Regularly rotate client secrets and access keys

## Troubleshooting

### Common Issues

1. **Authentication Errors**
   - Verify TokenUrl, ClientId, and ClientSecret are correct
   - Ensure the Azure AD application has proper permissions

2. **Connection Errors**
   - Check StorageConnectionString is valid
   - Verify network connectivity to Azure resources

3. **No Instances Found**
   - Verify TableName matches your Durable Functions hub table
   - Ensure instances exist and are older than 30 days

4. **Permission Denied**
   - Verify the AppCode (function key) has sufficient permissions
   - Check Azure RBAC roles on the Function App

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

[Specify your license here]

## Support

For issues or questions, please open an issue in the GitHub repository.

