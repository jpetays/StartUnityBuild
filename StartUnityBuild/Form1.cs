using System.Diagnostics.CodeAnalysis;
using System.Media;
using System.Reflection;
using System.Text;
using Editor.Prg.BatchBuild;
using NLog;

namespace StartUnityBuild;

[SuppressMessage("ReSharper", "LocalizableElement")]
public partial class Form1 : Form
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly string _appVersion;

    private static readonly char[] Separators = ['\r', '\n'];

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private static Form1 _instance;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private readonly BuildSettings _settings;

    public Form1()
    {
        _appVersion = Application.ProductVersion.Split('+')[0];
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
        label2.Text = "";
        timer1.Interval = 1000;

        SetupMenuCommands();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Text = $"{(Commands.IsDryRun ? "TEST " : "")}Build {_appVersion} UNITY";
        try
        {
            LoadEnvironment();
            Text =
                $"{Text} {_settings.UnityEditorVersion} - App {_settings.ProductVersion} - Targets {string.Join(',', _settings.BuildTargets)}";
            UpdateProjectInfo(Color.Magenta);
            StartupCommand();
            if (_settings.BuildTargets.Count == 0)
            {
                AddLine("ERROR", "Could not find any build targets");
            }
        }
        catch (Exception x)
        {
            AddLine($"Failed to LoadEnvironment");
            AddLine("ERROR", $"{x.GetType().Name}: {x.Message}");
            if (x is ApplicationException)
            {
                return;
            }
            if (x.StackTrace != null)
            {
                foreach (var line in x.StackTrace.Split(Separators, StringSplitOptions.RemoveEmptyEntries))
                {
                    AddLine(line.Trim());
                }
            }
        }
    }

    private void SetupMenuCommands()
    {
        copyOutputToClipboardToolStripMenuItem.Click += (_, _) => CopyLines();
        exitToolStripMenuItem.Click += (_, _) => Application.Exit();
        resetFolderAndExitToolStripMenuItem.Click += (_, _) =>
        {
            Program.DeleteAppPropertiesFile();
            Application.Exit();
        };
        var isCommandExecuting = false;
        var startTime = DateTime.Now;
        var timerLabel = "";
        timer1.Tick += (_, _) =>
        {
            var duration = DateTime.Now - startTime;
            SetStatus($"{timerLabel} {duration:mm':'ss}", Color.Green);
        };
        gitStatusToolStripMenuItem.Text = $"[{gitStatusToolStripMenuItem.Text}]";
        gitStatusToolStripMenuItem.Click += (_, _) => ExecuteMenuCommand(() =>
        {
            if (isCommandExecuting)
            {
                MessageBox.Show("A command is already executing", "UNITY Build", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }
            timerLabel = "Executing";
            startTime = DateTime.Now;
            timer1.Start();
            ClearLines();
            isCommandExecuting = true;
            Commands.GitStatus(_settings.WorkingDirectory, () =>
            {
                isCommandExecuting = false;
                timer1.Stop();
                var duration = DateTime.Now - startTime;
                SetStatus($"Done in {duration:mm':'ss}", Color.Blue);
            });
        });
        updateBuildToolStripMenuItem.Text = $"[{updateBuildToolStripMenuItem.Text}]";
        updateBuildToolStripMenuItem.Click += (_, _) => ExecuteMenuCommand(() =>
        {
            if (isCommandExecuting)
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
            timerLabel = "Updating";
            startTime = DateTime.Now;
            timer1.Start();
            ClearLines();
            isCommandExecuting = true;
            Commands.UnityUpdate(_settings,
                (updated) =>
                {
                    isCommandExecuting = false;
                    timer1.Stop();
                    var duration = DateTime.Now - startTime;
                    SetStatus($"Done in {duration:mm':'ss}", Color.Blue);
                    UpdateProjectInfo(updated ? Color.Green : Color.Red);
                    PlayNotification();
                });
        });
        startBuildToolStripMenuItem.Text = $"[{startBuildToolStripMenuItem.Text}]";
        startBuildToolStripMenuItem.Click += (_, _) => ExecuteMenuCommand(() =>
        {
            if (isCommandExecuting)
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
            timerLabel = "Building";
            startTime = DateTime.Now;
            timer1.Start();
            ClearLines();
            isCommandExecuting = true;
            Commands.UnityBuild(_settings,
                () =>
                {
                    isCommandExecuting = false;
                    timer1.Stop();
                    var duration = DateTime.Now - startTime;
                    SetStatus($"Done in {duration:mm':'ss}", Color.Blue);
                });
        });
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
            Commands.GitStatus(_settings.WorkingDirectory, () => { SetStatus("Ready", Color.Blue); }));
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
        }
        catch (DirectoryNotFoundException)
        {
            throw new ApplicationException($"ProjectVersion.txt not found, is this UNITY project folder?");
        }
        AddLine("Unity", $"{_settings.UnityEditorVersion}");
        ProjectSettings.LoadProjectSettingsFile(_settings.WorkingDirectory,
            out var productName, out var productVersion, out var bundleVersion, out var muteOtherAudioSources);
        _settings.ProductName = productName;
        _settings.ProductVersion = productVersion;
        _settings.BundleVersion = bundleVersion;
        _settings.IsMuteOtherAudioSources = muteOtherAudioSources;
        AddLine(">Product", $"{_settings.ProductName}");
        AddLine(">Version", $"{_settings.ProductVersion}");
        AddLine(">Bundle", $"{_settings.BundleVersion}");
        if (Commands.IsVersionDate)
        {
            AddLine(".update", "Version is Date dd.mm.yyyy");
        }
        else if (Commands.IsVersionSemantic)
        {
            AddLine(".update", "Version is semantic");
        }
        var buildTargets = new List<string>();
        Files.LoadAutoBuildTargets(_settings.WorkingDirectory, out var unityPath, buildTargets);
        _settings.UnityPath = unityPath;
        _settings.BuildTargets.AddRange(buildTargets);
        AddLine("Builds", $"{string.Join(',', _settings.BuildTargets)}");
        var assetFolder = Path.Combine(_settings.WorkingDirectory, "Assets");
        _settings.BuildInfoFilename = BuildInfoUpdater.BuildInfoFilename(assetFolder);
        var exists = File.Exists(_settings.BuildInfoFilename);
        AddLine($"{(exists ? ".BuildInfo" : "ERROR")}", $"{(exists ? "" : "-")}{_settings.BuildInfoFilename}");
        var setUnityExecutablePath =
            !string.IsNullOrEmpty(_settings.UnityPath) && !string.IsNullOrEmpty(_settings.UnityEditorVersion);
        if (setUnityExecutablePath)
        {
            _settings.UnityExecutable = _settings.UnityPath.Replace("$VERSION$", _settings.UnityEditorVersion);
            exists = File.Exists(_settings.UnityExecutable);
            AddLine($"{(exists ? "." : "")}Executable", $"{(exists ? "" : "-")}{_settings.UnityExecutable}");
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

    public static void ExitListener(string prefix, int exitCode)
    {
        AddLine($">{prefix}", $"exit: {exitCode}");
    }

    private void ClearLines()
    {
        if (InvokeRequired)
        {
            Invoke(() => ClearLines);
            return;
        }
        Logger.Trace("*");
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
