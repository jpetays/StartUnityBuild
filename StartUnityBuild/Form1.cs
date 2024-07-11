using System.Diagnostics.CodeAnalysis;
using System.Media;
using System.Reflection;
using System.Text;
using NLog;
using PrgBuild;
using StartUnityBuild.Commands;

namespace StartUnityBuild;

[SuppressMessage("ReSharper", "LocalizableElement")]
public partial class Form1 : Form
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly string _baseTitle;

    private static readonly char[] Separators = ['\r', '\n'];

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private static Form1 _instance;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private BuildSettings _settings;

    private bool _isCommandExecuting;
    private string _commandLabel = "";
    private DateTime _commandStartTime;

    public Form1(bool isTesting = false)
    {
        RunCommand.IsVerbose = isTesting;
        var appVersion = Application.ProductVersion.Split('+')[0];
        _baseTitle = $"Build {appVersion} UNITY";
        _instance = this;
        _settings = new BuildSettings(Directory.GetCurrentDirectory());
        InitializeComponent();
        // Reduce Graphics Flicker with Double Buffering for Forms and Controls
        // https://learn.microsoft.com/en-us/dotnet/desktop/winforms/advanced/how-to-reduce-graphics-flicker-with-double-buffering-for-forms-and-controls?view=netframeworkdesktop-4.8
        listView1.DoubleBuffered(true);
        KeyPreview = true;
        KeyDown += OnKeyDown;

        listView1.Font = new Font("Cascadia Mono", 10);
        listView1.Columns.Add("Output", 1920, HorizontalAlignment.Left);
        listView1.FullRowSelect = true;
        //listView1.GridLines = true;
        listView1.View = View.Details;
        label1.Text = "";
        timer1.Interval = 1000;

        SetupMenuCommands(isTesting);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Text = _baseTitle;
        if (Files.HasProjectVersionFile(Directory.GetCurrentDirectory()))
        {
            LoadProject();
            return;
        }
        AddLine("INFO", "");
        AddLine("INFO", "-Use menu File->Set Project Folder to set UNITY project location");
        AddLine("INFO", "");
    }

    private void LoadProject()
    {
        try
        {
            LoadEnvironment();
            Text =
                $"{_baseTitle} {_settings.UnityEditorVersion} - {_settings.ProductName} {_settings.ProductVersion}" +
                $" - Target{(_settings.BuildTargets.Count > 1 ? "" : "s")} [{string.Join(',', _settings.BuildTargets)}]" +
                $" in {_settings.WorkingDirectory}";
            UpdateProjectInfo(_settings.BuildTargets.Count > 0 ? Color.Magenta : Color.Red);
            StartupCommand();
            if (_settings.BuildTargets.Count == 0)
            {
                AddLine("ERROR", "Could not find any build targets");
            }
            return;
        }
        catch (Exception x)
        {
            AddLine($"Failed to LoadEnvironment");
            AddLine("ERROR", $"{x.GetType().Name}: {x.Message}");
            if (x is not ApplicationException && x.StackTrace != null)
            {
                foreach (var line in x.StackTrace.Split(Separators, StringSplitOptions.RemoveEmptyEntries))
                {
                    AddLine(line.Trim());
                }
            }
        }
        AddLine("INFO", "");
        AddLine("INFO", "-Use menu File->Set Project Folder to set UNITY project location");
        AddLine("INFO", "");
    }

    private void SetProjectFolder()
    {
        ClearLines();
        var folderBrowserDialog = new FolderBrowserDialog();
        if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }
        var directoryName = folderBrowserDialog.SelectedPath;
        AddLine(".dir", directoryName);
        if (!Directory.Exists(directoryName))
        {
            return;
        }
        Directory.SetCurrentDirectory(directoryName);
        _settings = new BuildSettings(Directory.GetCurrentDirectory());
        LoadProject();
    }

    private void SetupMenuCommands(bool isTesting)
    {
        copyOutputToClipboardToolStripMenuItem.Click += (_, _) => CopyLines();
        exitToolStripMenuItem.Click += (_, _) => Application.Exit();
        setProjectFolderToolStripMenuItem.Click += (_, _) => { SetProjectFolder(); };
        timer1.Tick += (_, _) =>
        {
            var duration = DateTime.Now - _commandStartTime;
            SetStatus($"{_commandLabel} {duration:mm':'ss}", Color.Green);
        };
        var order = -1;
        SetCaption(gitStatusToolStripMenuItem, ++order);
        gitStatusToolStripMenuItem.Click += (_, _) => ExecuteMenuCommandSync("Executing",
            () => { GitCommands.GitStatus(_settings.WorkingDirectory, ReleaseMenuCommandSync); });

        SetCaption(gitPullToolStripMenuItem, ++order);
        gitPullToolStripMenuItem.Click += (_, _) => ExecuteMenuCommandSync("Executing",
            () => { GitCommands.GitPull(_settings.WorkingDirectory, ReleaseMenuCommandSync); });

        SetCaption(updateBuildToolStripMenuItem, ++order);
        updateBuildToolStripMenuItem.Click += (_, _) => ExecuteMenuCommandSync("Updating", () =>
        {
            ProjectCommands.ModifyProject(_settings,
                (updated) =>
                {
                    ReleaseMenuCommandSync();
                    UpdateProjectInfo(updated ? Color.Green : Color.Red);
                    PlayNotification();
                });
        });
        if (isTesting)
        {
            _settings.PushOptions = "--dry-run";
        }
        SetCaption(gitPushToolStripMenuItem, ++order);
        gitPushToolStripMenuItem.Click += (_, _) => ExecuteMenuCommandSync("Executing",
            () => { GitCommands.GitPush(_settings.WorkingDirectory, _settings.PushOptions, ReleaseMenuCommandSync); });

        SetCaption(startBuildToolStripMenuItem, ++order);
        startBuildToolStripMenuItem.Click += (_, _) => ExecuteMenuCommandSync("Building",
            () =>
            {
                // (1) Copy required (secret) files for build.
                // (2) Build the project.
                // (3) Revert copied and/or changed files.
                if (!ProjectCommands.CopyFiles(_settings))
                {
                    AddLine("ERROR", $"Unable to start build: can not copy required 'build project' files");
                    ReleaseMenuCommandSync();
                    return;
                }
                BuildCommands.BuildPlayer(_settings,
                    () =>
                    {
                        GitCommands.GitRevert(
                            _settings.WorkingDirectory, _settings.RevertFiles, ReleaseMenuCommandSync);
                    });
            });
        return;

        void SetCaption(ToolStripItem item, int itemNumber)
        {
            item.Text = $"[{itemNumber}] {item.Text}";
        }
    }

    private void UpdateProjectInfo(Color color)
    {
        projectInfoToolStripMenuItem.Text = $"Version {_settings.ProductVersion} Bundle {_settings.BundleVersion}";
        projectInfoToolStripMenuItem.ForeColor = color;
    }

    private void StartupCommand()
    {
        Thread.Yield();
        ExecuteMenuCommand(() =>
            GitCommands.GitStatus(_settings.WorkingDirectory, () => { SetStatus("Ready", Color.Blue); }));
    }

    private void ExecuteMenuCommandSync(string commandLabel, Action command)
    {
        if (_isCommandExecuting)
        {
            MessageBox.Show("A command is already executing", "UNITY Build", MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);
            return;
        }
        if (_settings.BuildTargets.Count == 0)
        {
            MessageBox.Show("No build target found", "UNITY Build", MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);
            return;
        }
        _isCommandExecuting = true;
        _commandLabel = commandLabel;
        _commandStartTime = DateTime.Now;
        timer1.Start();
        ClearLines();
        try
        {
            command();
        }
        catch (Exception x)
        {
            AddLine("ERROR", $"{x.Message}");
        }
    }

    private void ReleaseMenuCommandSync()
    {
        _isCommandExecuting = false;
        timer1.Stop();
        var duration = DateTime.Now - _commandStartTime;
        SetStatus($"Done in {duration:mm':'ss}", Color.Blue);
    }

    private void ExecuteMenuCommand(Action command)
    {
        try
        {
            command();
        }
        catch (Exception x)
        {
            AddLine("ERROR", $"{x.Message}");
        }
    }

    private static void PlayNotification()
    {
        if (_instance.InvokeRequired)
        {
            _instance.Invoke(() => PlayNotification);
            return;
        }
        SystemSounds.Exclamation.Play();
    }

    private void SetStatus(string statusText, Color color)
    {
        if (InvokeRequired)
        {
            Invoke(() => SetStatus($"[{statusText}]", color));
            return;
        }
        label1.Text = statusText;
        label1.ForeColor = color;
    }

    private static void OnKeyDown(object? sender, KeyEventArgs? e)
    {
        if (e is { Control: true, KeyCode: Keys.C })
        {
            CopyLines();
            e.SuppressKeyPress = true; // Stops other controls on the form receiving event.
        }
    }

    private void LoadEnvironment()
    {
        AddLine(">Project", $"{_settings.WorkingDirectory}");
        try
        {
            Files.LoadProjectVersionFile(_settings.WorkingDirectory, out var unityVersion);
            _settings.UnityEditorVersion = unityVersion;
            AppSettings.SetUnityProjectFolder(_settings.WorkingDirectory);
        }
        catch (DirectoryNotFoundException)
        {
            throw new ApplicationException($"ProjectVersion.txt not found, is this UNITY project folder?");
        }
        AddLine(".unity", $"{_settings.UnityEditorVersion}");
        ProjectSettings.LoadProjectSettingsFile(_settings.WorkingDirectory,
            out var productName, out var productVersion, out var bundleVersion, out var muteOtherAudioSources);
        _settings.ProductName = productName;
        _settings.ProductVersion = productVersion;
        _settings.BundleVersion = bundleVersion;
        _settings.IsMuteOtherAudioSources = muteOtherAudioSources;
        Files.LoadAutoBuildSettings(_settings);
        AddLine("Product", $"{_settings.ProductName}");
        AddLine("Version", $"{_settings.ProductVersion}");
        AddLine("Bundle", $"{_settings.BundleVersion}");
        AddLine("Builds", $"{string.Join(',', _settings.BuildTargets)}");
        bool exists;
        if (_settings.CopyFiles.Count > 0)
        {
            // This checks CopyFiles validity as well.
            var copyFiles = ProjectCommands.GetCopyFiles(_settings);
            foreach (var tuple in copyFiles)
            {
                exists = File.Exists(tuple.Item1);
                AddLine($"{(exists ? ".copy" : "ERROR")}", $"copy {tuple.Item1} to {tuple.Item2}");
            }
        }
        if (_settings.RevertFiles.Count > 0)
        {
            foreach (var file in _settings.RevertFiles)
            {
                var path = Path.Combine(".", file);
                exists = File.Exists(path);
                AddLine($"{(exists ? ".revert" : "ERROR")}", $"git revert {path}");
            }
        }
        var assetFolder = Files.GetAssetFolder(_settings.WorkingDirectory);
        _settings.BuildInfoFilename = BuildInfoUpdater.BuildPropertiesPath(assetFolder);
        exists = File.Exists(_settings.BuildInfoFilename);
        AddLine($"{(exists ? ".file" : "ERROR")}", $"{_settings.BuildInfoFilename}");
        if (!exists)
        {
            AddLine("ERROR", $"assetFolder {assetFolder}");
        }
        var setUnityExecutablePath =
            !string.IsNullOrEmpty(_settings.UnityPath) && !string.IsNullOrEmpty(_settings.UnityEditorVersion);
        if (setUnityExecutablePath)
        {
            _settings.UnityExecutable = _settings.UnityPath.Replace("$VERSION$", _settings.UnityEditorVersion);
            exists = File.Exists(_settings.UnityExecutable);
            AddLine($"{(exists ? ".file" : "ERROR")}", $"{_settings.UnityExecutable}");
            if (!exists)
            {
                AddLine("ERROR",
                    $"UnityExecutable not found for path '{_settings.UnityPath}' and version '{_settings.UnityEditorVersion}'");
            }
        }
        else
        {
            AddLine("ERROR",
                $"UnityExecutable error with path '{_settings.UnityPath}' and version '{_settings.UnityEditorVersion}'");
        }
    }

    [SuppressMessage("ReSharper", "NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract")]
    public static void OutputListener(string prefix, string line)
    {
        if (string.IsNullOrEmpty(line))
        {
            return;
        }
        AddLine(prefix ?? "ERROR", line);
    }

    public static void ExitListener(string processPrefix, string commandPrefix, int exitCode)
    {
        AddLine($">{processPrefix}", $"{commandPrefix} exit: {exitCode}");
    }

    private void ClearLines()
    {
        if (InvokeRequired)
        {
            Invoke(() => ClearLines);
            return;
        }
        listView1.BeginUpdate();
        listView1.Items.Clear();
        listView1.EndUpdate();
    }

    private static void AddLine(string line, Color? color = null)
    {
        if (_instance.InvokeRequired)
        {
            _instance.Invoke(() => AddLine(line, color));
            return;
        }
        line = $"{DateTime.Now:hh:mm:ss} {line}";
        Logger.Trace(line);
        var listView = _instance.listView1;
        listView.BeginUpdate();
        if (color == null)
        {
            listView.Items.Add(line);
        }
        else
        {
            listView.Items.Add(new ListViewItem(line)
            {
                ForeColor = color.Value
            });
        }
        listView.EndUpdate();
        listView.EnsureVisible(listView.Items.Count - 1);
    }

    private static void CopyLines()
    {
        var builder = new StringBuilder();
        var listView = _instance.listView1;
        foreach (var item in listView.Items)
        {
            builder.AppendLine(item is ListViewItem listViewItem ? listViewItem.Text : item.ToString());
        }
        Clipboard.SetText(builder.ToString());
    }

    public static void AddLine(string prefix, string content)
    {
        Color? color = prefix.StartsWith("ERROR") ? Color.Red
            : prefix.StartsWith('.') ? Color.Gray
            : prefix.StartsWith('>') ? Color.Blue
            : content.StartsWith('-') ? Color.Magenta
            : content.StartsWith('+') || content.Contains("SUCCESSFULLY") ? Color.Green
            : null;
        AddLine($"{prefix,-12}: {content}", color);
    }
}

public static class Extensions
{
    [SuppressMessage("ReSharper", "NullableWarningSuppressionIsUsed")]
    public static void DoubleBuffered(this Control control, bool enabled)
    {
        var propertyInfo = control.GetType()
            .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
        propertyInfo!.SetValue(control, enabled, null);
    }
}
