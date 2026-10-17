using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.IO;
using ParcelCore;
using ParcelServices;
using ParcelAIAgent;

namespace ParcelDesktop
{
    public partial class MainWindow : Window
    {
        private readonly PersistenceService _persistence;
        private readonly BackupService _backupService;
        private readonly QLMEngine _qlmEngine;
        private readonly LocalAgent _agent;
        private readonly NavigationController _nav;

        public MainWindow()
        {
            InitializeComponent();

            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.DefaultDatabaseFileName);
            _persistence = new PersistenceService(dbPath);
            _backupService = new BackupService(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups"));

            _qlmEngine = new QLMEngine();
            var qlmDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "QLM");
            if (!Directory.Exists(qlmDir))
            {
                qlmDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "QLM");
            }
            _qlmEngine.LoadRules(qlmDir);

            _agent = new LocalAgent(_qlmEngine);
            _nav = new NavigationController();

            _nav.NavigationChanged += target =>
            {
                ViewTitleText.Text = $"{target} View";
                ViewContentText.Text = $"Active navigation target: {target}. SQLite DB: {dbPath} | QLM Rules: {_qlmEngine.RuleCount}";
            };
        }

        private void OnDashboardClick(object sender, RoutedEventArgs e) => _nav.NavigateTo(NavigationTarget.Dashboard);
        private void OnAIAgentClick(object sender, RoutedEventArgs e) => _nav.NavigateTo(NavigationTarget.SSAIConsole);
        private void OnDatabaseClick(object sender, RoutedEventArgs e) => _nav.NavigateTo(NavigationTarget.Database);
        private void OnExplorerClick(object sender, RoutedEventArgs e) => _nav.NavigateTo(NavigationTarget.Explorer);
        private void OnSettingsClick(object sender, RoutedEventArgs e) => _nav.NavigateTo(NavigationTarget.Settings);

        private async void OnRunAgentClick(object sender, RoutedEventArgs e)
        {
            var prompt = AgentInputTextBox.Text ?? "Analyze project architecture";
            AgentOutputTextBox.Text = "Running SS AI Agent analysis...";
            var result = await _agent.ProcessPromptAsync(prompt);
            AgentOutputTextBox.Text = result;
        }
    }
}
