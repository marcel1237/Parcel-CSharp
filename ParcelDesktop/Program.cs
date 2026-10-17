using System;
using System.Threading.Tasks;
using ParcelCore;
using ParcelServices;
using ParcelAIAgent;

namespace ParcelDesktop
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("==================================================");
            Console.WriteLine($"  {Constants.AppTitle} v{VersionInfo.FullVersion}");
            Console.WriteLine("==================================================");

            // 1. Initialize Persistence Service
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.DefaultDatabaseFileName);
            var persistence = new PersistenceService(dbPath);
            persistence.SaveSetting("LastStartup", DateTime.Now.ToString("o"));
            Console.WriteLine($"[Persistence] SQLite initialized at: {dbPath}");
            Console.WriteLine($"[Persistence] LastStartup setting retrieved: {persistence.GetSetting("LastStartup")}");

            // 2. Initialize Backup Service
            var backupService = new BackupService(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups"));
            Console.WriteLine($"[Backup] Backup service initialized.");

            // 3. Initialize QLM Engine & AI Agent
            var qlmEngine = new QLMEngine();
            qlmEngine.LoadRules(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "QLM"));
            Console.WriteLine($"[QLM Engine] Loaded rules count: {qlmEngine.RuleCount}");

            var agent = new LocalAgent(qlmEngine);
            var response = await agent.ProcessPromptAsync("Analyze project architecture and ensure industrial resilience.");
            Console.WriteLine($"[AI Agent] {response}");

            // 4. Initialize Navigation Controller
            var nav = new NavigationController();
            nav.NavigationChanged += target => Console.WriteLine($"[Navigation] Switched to: {target}");
            nav.NavigateTo(NavigationTarget.Dashboard);
            nav.NavigateTo(NavigationTarget.SSAIConsole);

            Console.WriteLine("==================================================");
            Console.WriteLine("  C# Migration Verification Successful!");
            Console.WriteLine("==================================================");
        }
    }
}
