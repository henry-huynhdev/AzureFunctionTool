using TerminateGRInstance.Application;
using TerminateGRInstance.Enum;

class Program
{
    static async Task Main(string[] args)
    {
        #region APP CONFIGURATION

        // Load configuration from appsettings.json
        var envConfig = ConfigLoader.LoadEnvConfig("appsettings.json");
        var cleanupService = new AzureFunctionCleanupService(envConfig);

        #endregion

        #region MAIN LOGIC

        Console.WriteLine($"Service: {envConfig.AppName}");
        Console.WriteLine("========================================\n");

        // Prompt user to select action
        Console.WriteLine("Select action:");
        Console.WriteLine("1. Collect Information");
        Console.WriteLine("2. Cleanup Instances");
        Console.Write("Enter your choice (1-2): ");
        var actionInput = Console.ReadLine();
        bool doCollect = actionInput == "1";
        bool doCleanup = actionInput == "2";

        Console.WriteLine();

        List<string> listStatusToCleanup = new List<string>();

        // If cleanup is selected, ask user to select which statuses to clean up
        if (doCleanup)
        {
            Console.WriteLine("Select instance statuses to cleanup:");
            var allStatuses = System.Enum.GetValues(typeof(InstanceStatus)).Cast<InstanceStatus>().ToList();
            
            for (int i = 0; i < allStatuses.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {allStatuses[i]}");
            }
            
            Console.Write($"\nEnter status numbers to cleanup (comma-separated, e.g., 1,2): ");
            var statusInput = Console.ReadLine();
            
            if (!string.IsNullOrWhiteSpace(statusInput))
            {
                var selectedNumbers = statusInput.Split(',')
                    .Select(s => s.Trim())
                    .Where(s => int.TryParse(s, out _))
                    .Select(int.Parse)
                    .Where(n => n > 0 && n <= allStatuses.Count)
                    .Distinct()
                    .ToList();

                foreach (var num in selectedNumbers)
                {
                    listStatusToCleanup.Add(allStatuses[num - 1].ToString());
                }
            }

            if (listStatusToCleanup.Count == 0)
            {
                Console.WriteLine("\nNo valid statuses selected. Exiting...");
                return;
            }

            // Show selected statuses and ask for confirmation
            Console.WriteLine("\n========================================");
            Console.WriteLine("Selected statuses to cleanup:");
            foreach (var status in listStatusToCleanup)
            {
                Console.WriteLine($"  - {status}");
            }
            Console.WriteLine("========================================");
            Console.WriteLine("\n⚠️  WARNING: This action CANNOT be undone!");
            Console.WriteLine("⚠️  All instances with the selected statuses will be permanently deleted.");
            Console.Write("\nAre you sure you want to continue? (yes/no): ");
            var confirmation = Console.ReadLine()?.Trim().ToLower();

            if (confirmation != "yes" && confirmation != "y")
            {
                Console.WriteLine("\nCleanup cancelled.");
                return;
            }

            Console.WriteLine();
        }

        if (doCollect)
            await cleanupService.CollectInformationAsync();
        if (doCleanup)
            await cleanupService.CleanupInstancesAsync(listStatusToCleanup);

        Console.WriteLine("\n========================================");
        Console.WriteLine("Processing completed!");
        Console.WriteLine("========================================");

        #endregion
    }
}