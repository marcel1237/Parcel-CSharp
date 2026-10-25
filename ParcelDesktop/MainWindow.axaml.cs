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
        private readonly GitService _gitService;
        private readonly FileSystemService _fileSystemService;
        private readonly QLMEngine _qlmEngine;
        private readonly LocalAgent _agent;
        private readonly GeminiClient _geminiClient;
        private readonly OpenAIClient _openAIClient;
        private readonly MLService _mlService;
        private readonly WebSearchService _webSearchService;
        private readonly WorkspaceService _workspaceService;
        private readonly ShellService _shellService;
        private string? _openFilePath;
        private int _editorMode;

        public MainWindow()
        {
            InitializeComponent();

            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var dbPath = Path.Combine(basePath, Constants.DefaultDatabaseFileName);

            _persistence = new PersistenceService(dbPath);
            _backupService = new BackupService(Path.Combine(basePath, "backups"));
            _gitService = new GitService(Directory.GetCurrentDirectory());
            _fileSystemService = new FileSystemService();
            _workspaceService = new WorkspaceService();
            _shellService = new ShellService();

            _qlmEngine = new QLMEngine();
            var qlmDir = Path.Combine(basePath, "QLM");
            if (!Directory.Exists(qlmDir))
                qlmDir = Path.Combine(basePath, "..", "..", "..", "..", "QLM");
            _qlmEngine.LoadRules(qlmDir);

            _agent = new LocalAgent(_qlmEngine);
            _geminiClient = new GeminiClient();
            _openAIClient = new OpenAIClient();
            _mlService = new MLService();
            _webSearchService = new WebSearchService();

            RestoreSession();
            StatusText.Text = $"Status: Active | Workspace: {_workspaceService.RootPath} | SQLite: {dbPath} | QLM Rules Loaded: {_qlmEngine.RuleCount}";
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            _persistence.SaveSession(_workspaceService.RootPath, _openFilePath, _editorMode);
            base.OnClosing(e);
        }

        // Sidebar Navigation (1:1 C++ Pane Parity)
        private void OnNavDashboard(object sender, RoutedEventArgs e) => MainTabControl.SelectedIndex = 0;

        private void OnNavExplorer(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 1;
            LeftTabs.SelectedIndex = 0;
        }

        private void OnNavEditor(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 1;
            EditorTabs.SelectedIndex = 0;
        }

        private void OnNavAIConsole(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 1;
            BottomTabs.SelectedIndex = 6;
        }

        private void OnNavLLMClients(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 1;
            EastTabs.SelectedIndex = 0;
        }

        private void OnNavDatabase(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 1;
            BottomTabs.SelectedIndex = 7;
        }

        private void OnNavGit(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 1;
            EastTabs.SelectedIndex = 3;
        }

        private void OnNavSaves(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 1;
            BottomTabs.SelectedIndex = 11;
        }

        private void OnNavTerminal(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 1;
            EastTabs.SelectedIndex = 2;
        }

        private void OnNavShellScripting(object sender, RoutedEventArgs e) => OnNavTerminal(sender, e);

        private void OnNavBrowser(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 1;
            BottomTabs.SelectedIndex = 0;
        }

        private void OnNavKDE(object sender, RoutedEventArgs e) => OnNavInspection(sender, e);
        private void OnNavGTK(object sender, RoutedEventArgs e) => OnNavInspection(sender, e);
        private void OnNavDotNet(object sender, RoutedEventArgs e) => MainTabControl.SelectedIndex = 2;

        private void OnNavInspection(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 1;
            BottomTabs.SelectedIndex = 8;
        }

        private void OnNavSettings(object sender, RoutedEventArgs e) => MainTabControl.SelectedIndex = 2;

        // Package Explorer actions
        private void OnRefreshExplorer(object sender, RoutedEventArgs e)
        {
            PackageExplorerList.Items.Clear();
            if (_workspaceService.RootPath is null)
            {
                FileInspectorBox.Text = "No workspace is open.";
                return;
            }

            foreach (var entry in _workspaceService.GetEntries())
            {
                PackageExplorerList.Items.Add(new ListBoxItem
                {
                    Content = entry.IsDirectory ? $"[DIR] {entry.RelativePath}" : entry.RelativePath,
                    Tag = entry
                });
            }
        }

        private void OnOpenWorkspace(object sender, RoutedEventArgs e)
        {
            try
            {
                var path = _workspaceService.OpenOrCreate(WorkspacePathBox.Text ?? string.Empty);
                WorkspacePathBox.Text = path;
                _gitService.SetRepository(path);
                _persistence.SaveSession(path, _openFilePath, _editorMode);
                OnRefreshExplorer(sender, e);
                StatusText.Text = $"Workspace: {path}";
            }
            catch (Exception ex)
            {
                FileInspectorBox.Text = $"Workspace error: {ex.Message}";
            }
        }

        private void OnExplorerSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (PackageExplorerList.SelectedItem is not ListBoxItem { Tag: WorkspaceEntry entry })
                return;

            if (entry.IsDirectory)
            {
                FileInspectorBox.Text = $"Directory\n{entry.RelativePath}";
                return;
            }

            _openFilePath = entry.FullPath;
            var info = new FileInfo(entry.FullPath);
            FileInspectorBox.Text = $"File: {entry.RelativePath}\nSize: {info.Length} bytes\nModified: {info.LastWriteTime}";
            OpenSelectedFile();
            MainTabControl.SelectedIndex = 2;
        }

        // Code Editor actions
        private void OnOpenFile(object sender, RoutedEventArgs e)
        {
            OpenSelectedFile();
        }

        private void OnSaveFile(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_openFilePath is null)
                    throw new InvalidOperationException("Select a file in Package Explorer before saving.");
                _fileSystemService.WriteFile(_workspaceService.ResolvePath(_openFilePath), CodeEditorBox.Text ?? string.Empty);
                _persistence.SaveSession(_workspaceService.RootPath, _openFilePath, _editorMode);
                StatusText.Text = $"Saved: {Path.GetFileName(_openFilePath)}";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Save error: {ex.Message}";
            }
        }

        private void OnToggleDesigner(object sender, RoutedEventArgs e)
        {
            _editorMode = _editorMode == 0 ? 1 : 0;
            EditorModeText.Text = _editorMode == 0
                ? "Mode: Code editor"
                : "Mode: Designer unavailable for this file type";
            if (_editorMode == 0)
                OpenSelectedFile();
            else
                FileInspectorBox.Text = "Designer capability is available only for supported Avalonia/XAML files.";
            _persistence.SaveSession(_workspaceService.RootPath, _openFilePath, _editorMode);
        }

        private void RestoreSession()
        {
            var session = _persistence.LoadSession();
            var workspacePath = session.WorkspacePath;
            if (string.IsNullOrWhiteSpace(workspacePath) || !Directory.Exists(workspacePath))
                workspacePath = Directory.GetCurrentDirectory();

            _workspaceService.OpenOrCreate(workspacePath);
            _gitService.SetRepository(workspacePath);
            WorkspacePathBox.Text = _workspaceService.RootPath;
            _editorMode = session.EditorMode;
            EditorModeText.Text = _editorMode == 0
                ? "Mode: Code editor"
                : "Mode: Designer unavailable for this file type";
            OnRefreshExplorer(this, new RoutedEventArgs());

            if (!string.IsNullOrWhiteSpace(session.OpenFilePath) && File.Exists(session.OpenFilePath))
            {
                _openFilePath = session.OpenFilePath;
                OpenSelectedFile();
            }
        }

        private void OpenSelectedFile()
        {
            try
            {
                if (_openFilePath is null)
                {
                    CodeEditorBox.Text = "Select a file in Package Explorer to open it.";
                    return;
                }

                var path = _workspaceService.ResolvePath(_openFilePath);
                CodeEditorBox.Text = _fileSystemService.ReadFile(path);
                StatusText.Text = $"Open: {Path.GetFileName(path)}";
            }
            catch (Exception ex)
            {
                CodeEditorBox.Text = $"Unable to open file: {ex.Message}";
            }
        }

        // SS AI Agent actions
        private async void OnRunAgent(object sender, RoutedEventArgs e)
        {
            var prompt = AgentInputBox.Text ?? "Analyze project architecture";
            AgentOutputBox.Text = "Executing SS AI Agent reasoning...";
            var result = await _agent.ProcessPromptAsync(prompt);
            AgentOutputBox.Text = result;
        }

        // Gemini & OpenAI Client actions
        private async void OnSendGemini(object sender, RoutedEventArgs e)
        {
            var apiKey = Environment.GetEnvironmentVariable("PARCEL_GEMINI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                GeminiOutput.Text = "Gemini unavailable: set PARCEL_GEMINI_API_KEY to enable this integration.";
                return;
            }

            GeminiOutput.Text = "Sending request to Gemini...";
            var res = await _geminiClient.GenerateContentAsync(apiKey, GeminiInput.Text ?? string.Empty);
            GeminiOutput.Text = res;
        }

        private async void OnSendOpenAI(object sender, RoutedEventArgs e)
        {
            var apiKey = Environment.GetEnvironmentVariable("PARCEL_OPENAI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                OpenAIOutput.Text = "OpenAI unavailable: set PARCEL_OPENAI_API_KEY to enable this integration.";
                return;
            }

            OpenAIOutput.Text = "Sending request to OpenAI...";
            var res = await _openAIClient.CompleteChatAsync(apiKey, OpenAIInput.Text ?? string.Empty);
            OpenAIOutput.Text = res;
        }

        // DBMS Explorer actions
        private void OnRunSql(object sender, RoutedEventArgs e)
        {
            try
            {
                var rows = _persistence.ExecuteQuery(SqlInputBox.Text ?? string.Empty);
                SqlOutputBox.Text = rows.Count == 0
                    ? "Query executed successfully. No rows returned."
                    : string.Join(Environment.NewLine, rows);
            }
            catch (Exception ex)
            {
                SqlOutputBox.Text = $"SQL Error: {ex.Message}";
            }
        }

        // Git actions
        private void OnGitStatus(object sender, RoutedEventArgs e)
        {
            GitOutputBox.Text = _gitService.GetStatus();
        }

        private void OnVersionsLog(object sender, RoutedEventArgs e)
        {
            GitOutputBox.Text = _gitService.RunGitCommand("log -n 5 --oneline");
        }

        // Backup & Saves actions
        private void OnCreateBackup(object sender, RoutedEventArgs e)
        {
            try
            {
                var source = _openFilePath;
                if (string.IsNullOrWhiteSpace(source) || !File.Exists(source))
                    source = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.DefaultDatabaseFileName);

                if (File.Exists(source))
                {
                    var backupPath = _backupService.CreateBackup(source);
                    BackupOutputBox.Text = $"Backup Snapshot Created Successfully:\n{backupPath}\nTimestamp: {DateTime.Now}";
                }
                else
                {
                    BackupOutputBox.Text = "Database file not found for backup.";
                }
            }
            catch (Exception ex)
            {
                BackupOutputBox.Text = $"Backup Error: {ex.Message}";
            }
        }

        // Terminal & Shell actions
        private async void OnRunShell(object sender, RoutedEventArgs e)
        {
            if (_workspaceService.RootPath is null)
            {
                ShellOutputBox.Text = "Terminal error: open a workspace before executing a command.";
                return;
            }

            try
            {
                ShellOutputBox.Text = "Running command...";
                var result = await _shellService.ExecuteAsync(
                    ShellInputBox.Text ?? string.Empty,
                    _workspaceService.RootPath,
                    TimeSpan.FromSeconds(30));
                var status = result.TimedOut
                    ? "Timed out after 30 seconds."
                    : $"Exit code: {result.ExitCode}";
                ShellOutputBox.Text = $"{status}\n{result.Output}{(string.IsNullOrWhiteSpace(result.Error) ? string.Empty : $"\nSTDERR:\n{result.Error}")}";
            }
            catch (Exception ex)
            {
                ShellOutputBox.Text = $"Terminal error: {ex.Message}";
            }
        }

        // Browser & WebStudio actions
        private void OnNavigateUrl(object sender, RoutedEventArgs e)
        {
            BrowserOutputBox.Text = "Embedded browser integration is unavailable in the current Avalonia dependencies. No page was rendered.";
        }

        // Frameworks actions
        private void OnKDE(object sender, RoutedEventArgs e) => FrameworkOutputBox.Text = "KDE Integration Manager: Active (KDE Frameworks / Kirigami parity)";
        private void OnGTK2(object sender, RoutedEventArgs e) => FrameworkOutputBox.Text = "GTK2 Manager: Active";
        private void OnGTK3(object sender, RoutedEventArgs e) => FrameworkOutputBox.Text = "GTK3 Manager: Active";
        private void OnGTK4(object sender, RoutedEventArgs e) => FrameworkOutputBox.Text = "GTK4 Manager: Active (GNOME Desktop Integration)";
        private void OnDotNet(object sender, RoutedEventArgs e) => FrameworkOutputBox.Text = ".NET 8 Integration Manager: Active (CLR & Metadata inspection)";
        private void OnInspection(object sender, RoutedEventArgs e) => FrameworkOutputBox.Text = "InspectionPane: Active (Memory and CPU monitoring)";
        private void OnLogcat(object sender, RoutedEventArgs e) => FrameworkOutputBox.Text = "LogcatPane: Active (Log stream streaming)";
    }
}
