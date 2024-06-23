using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using NLog;

namespace StartUnityBuild;

[SuppressMessage("ReSharper", "LocalizableElement")]
public partial class Form1 : Form
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly string _appVersion;

    private static readonly char[] Separators = ['\r', '\n'];

    private static Form1 _instance = null!;
    private readonly string _currentDirectory;
    private string? _unityVersion;
    private string? _productName;
    private string? _productVersion;
    private string? _bundleVersion;
    private readonly List<string> _buildTargets = new();
    private string? _unityPath;
    private string? _unityExecutable;
    private long _totalFileSize;

    public Form1()
    {
        _appVersion = Application.ProductVersion.Split('+')[0];
        _instance = this;
        _currentDirectory = Directory.GetCurrentDirectory();
        InitializeComponent();
        // Reduce Graphics Flicker with Double Buffering for Forms and Controls
        // https://learn.microsoft.com/en-us/dotnet/desktop/winforms/advanced/how-to-reduce-graphics-flicker-with-double-buffering-for-forms-and-controls?view=netframeworkdesktop-4.8
        listView1.DoubleBuffered(true);
        KeyPreview = true;
        KeyDown += OnKeyDown!;

        listView1.Font = new Font("Cascadia Mono", 10);
        listView1.Columns.Add("Output", 1920, HorizontalAlignment.Left);
        listView1.FullRowSelect = true;
        //listView1.GridLines = true;
        listView1.View = View.Details;
        label1.Text = "";
        label2.Text = "";
        timer1.Interval = 1000;

        copyOutputToClipboardToolStripMenuItem.Click += (_, _) => CopyLines();
        exitToolStripMenuItem.Click += (_, _) => Application.Exit();
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
            label2.Text = "";
            _totalFileSize = 0;
            isCommandExecuting = true;
            Commands.GitStatus(_currentDirectory, () =>
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
            if (_buildTargets.Count == 0)
            {
                MessageBox.Show("No build target found", "UNITY Build", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }
            timerLabel = "Updating";
            startTime = DateTime.Now;
            timer1.Start();
            ClearLines();
            label2.Text = "";
            _totalFileSize = 0;
            isCommandExecuting = true;
            Commands.UnityUpdate(_currentDirectory, _productVersion!, _bundleVersion!,
                (updated, productVersion, bundleVersion) =>
                {
                    isCommandExecuting = false;
                    timer1.Stop();
                    var duration = DateTime.Now - startTime;
                    if (updated)
                    {
                        _productVersion = productVersion;
                        _bundleVersion = bundleVersion;
                    }
                    SetStatus($"Done in {duration:mm':'ss}", Color.Blue);
                    UpdateProjectInfo(updated ? Color.Green : Color.Red);
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
            if (_buildTargets.Count == 0)
            {
                MessageBox.Show("No build target found", "UNITY Build", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }
            timerLabel = "Building";
            startTime = DateTime.Now;
            timer1.Start();
            ClearLines();
            label2.Text = "";
            _totalFileSize = 0;
            isCommandExecuting = true;
            Commands.UnityBuild(_currentDirectory, _unityExecutable!, _bundleVersion!, _buildTargets, () =>
            {
                isCommandExecuting = false;
                timer1.Stop();
                var duration = DateTime.Now - startTime;
                SetStatus($"Done in {duration:mm':'ss}", Color.Blue);
            }, fileSystemWatcher1, SetFileSizeProgress);
        });
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
#if DEBUG
        Commands.IsDryRun = true;
#endif
        Text = $"{(Commands.IsDryRun ? "TEST " : "")}Build {_appVersion} UNITY";
        try
        {
            LoadEnvironment();
            Text = $"{Text} {_unityVersion} - App {_productName} - Targets {string.Join(',', _buildTargets)}";
            UpdateProjectInfo(Color.Magenta);
            StartupCommand();
            if (_buildTargets.Count == 0)
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

    private void UpdateProjectInfo(Color color)
    {
        projectInfoToolStripMenuItem.Text = $"Version {_productVersion} Bundle {_bundleVersion}";
        projectInfoToolStripMenuItem.ForeColor = color;
    }

    private void StartupCommand()
    {
        Thread.Yield();
        ExecuteMenuCommand(() => Commands.GitStatus(_currentDirectory, () => { SetStatus("Ready", Color.Blue); }));
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

    private void SetFileSizeProgress(long fileSize)
    {
        if (InvokeRequired)
        {
            Invoke(() => SetFileSizeProgress(fileSize));
            return;
        }
        _totalFileSize = fileSize;
        label2.Text = $"bytes {_totalFileSize:N0}";
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e is { Control: true, KeyCode: Keys.C })
        {
            CopyLines();
            e.SuppressKeyPress = true; // Stops other controls on the form receiving event.
        }
    }

    private void LoadEnvironment()
    {
        AddLine("CWD", $"{_currentDirectory}");
        try
        {
            Files.LoadProjectVersionFile(_currentDirectory, ref _unityVersion!);
        }
        catch (DirectoryNotFoundException)
        {
            throw new ApplicationException($"ProjectVersion.txt not found, is this UNITY project folder?");
        }
        AddLine("Unity", $"{_unityVersion}");
        ProjectSettings.LoadProjectSettingsFile(_currentDirectory,
            ref _productName!, ref _productVersion!, ref _bundleVersion!);
        AddLine(">Product", $"{_productName}");
        AddLine(">Version", $"{_productVersion}");
        AddLine(">Bundle", $"{_bundleVersion}");
        Files.LoadAutoBuildTargets(_currentDirectory, ref _unityPath!, _buildTargets);
        AddLine("Builds", $"{string.Join(',', _buildTargets)}");
        var setUnityExecutablePath = !string.IsNullOrEmpty(_unityPath) && !string.IsNullOrEmpty(_unityVersion);
        if (setUnityExecutablePath)
        {
            _unityExecutable = _unityPath!.Replace("$VERSION$", _unityVersion);
            AddLine("Executable", $"{_unityExecutable}");
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
}

public static class Extensions
{
    public static void DoubleBuffered(this Control control, bool enabled)
    {
        var propertyInfo = control.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
        propertyInfo!.SetValue(control, enabled, null);
    }
}
