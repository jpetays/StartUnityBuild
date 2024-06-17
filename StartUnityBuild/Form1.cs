using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace StartUnityBuild;

[SuppressMessage("ReSharper", "LocalizableElement")]
public partial class Form1 : Form
{
    private const string AppVersion = "1.1";

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
        _instance = this;
        _currentDirectory = Directory.GetCurrentDirectory();
        InitializeComponent();
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
                timer1.Stop();
                var duration = DateTime.Now - startTime;
                SetStatus($"Done in {duration:mm':'ss}", Color.Blue);
                isCommandExecuting = false;
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
                    timer1.Stop();
                    var duration = DateTime.Now - startTime;
                    if (updated)
                    {
                        _productVersion = productVersion;
                        _bundleVersion = bundleVersion;
                    }
                    SetStatus($"Done in {duration:mm':'ss}", Color.Blue);
                    UpdateProjectInfo(updated ? Color.Green : Color.Red);
                    isCommandExecuting = false;
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
            Commands.UnityBuild(_currentDirectory, _unityExecutable!, _buildTargets, () =>
            {
                timer1.Stop();
                var duration = DateTime.Now - startTime;
                SetStatus($"Done in {duration:mm':'ss}", Color.Blue);
                isCommandExecuting = false;
            }, fileSystemWatcher1, SetFileSizeProgress);
        });
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Text = $"Build {AppVersion} UNITY";
        try
        {
            LoadEnvironment();
            Text = $"{Text} {_unityVersion} - App {_productName} Targets {string.Join(',', _buildTargets)}";
            UpdateProjectInfo(Color.Magenta);
            StartupCommand();
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
            LoadProjectVersionFile();
        }
        catch (DirectoryNotFoundException)
        {
            throw new ApplicationException($"ProjectVersion.txt not found, is this UNITY project folder?");
        }
        AddLine("Unity", $"{_unityVersion}");
        LoadProjectSettingsFile();
        AddLine(">Product", $"{_productName}");
        AddLine(">Version", $"{_productVersion}");
        AddLine(">Bundle", $"{_bundleVersion}");
        LoadAutoBuildTargets();
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

    private void LoadProjectVersionFile()
    {
        var path = Path.Combine(_currentDirectory, "ProjectSettings", "ProjectVersion.txt");
        AddLine(".file", $"{path}");
        var lines = File.ReadAllLines(path);
        foreach (var line in lines)
        {
            var tokens = line.Split(':');
            if (tokens[0].Trim() == "m_EditorVersion")
            {
                _unityVersion = tokens[1].Trim();
                return;
            }
        }
    }

    private void LoadAutoBuildTargets()
    {
        var path = Path.Combine(_currentDirectory, "etc", "batchBuild", "_auto_build.env");
        AddLine(".file", $"{path}");
        var lines = File.ReadAllLines(path);
        foreach (var line in lines)
        {
            var tokens = line.Split('=');
            switch (tokens[0].Trim())
            {
                case "buildTargets":
                {
                    var targets = tokens[1].Split(',');
                    foreach (var target in targets)
                    {
                        _buildTargets.Add(target.Trim());
                    }
                    break;
                }
                case "unityPath":
                    _unityPath = tokens[1].Trim();
                    break;
            }
        }
    }

    private void LoadProjectSettingsFile()
    {
        var path = Path.Combine(_currentDirectory, "ProjectSettings", "ProjectSettings.asset");
        AddLine(".file", $"{path}");
        var lines = File.ReadAllLines(path);
        foreach (var line in lines)
        {
            var tokens = line.Split(':');
            if (tokens[0] == "  productName")
            {
                _productName = tokens[1].Trim();
            }
            else if (tokens[0] == "  bundleVersion")
            {
                _productVersion = tokens[1].Trim();
            }
            else if (tokens[0] == "  AndroidBundleVersionCode")
            {
                _bundleVersion = tokens[1].Trim();
            }
        }
    }

    public static bool UpdateProjectSettingsFile(string workingDirectory,
        ref string productVersion, ref string bundleVersion, bool versionIsDate = true)

    {
        var path = Path.Combine(workingDirectory, "ProjectSettings", "ProjectSettings.asset");
        var lines = File.ReadAllLines(path);
        var curProductVersion = "";
        var curBundleVersion = "";
        foreach (var line in lines)
        {
            var tokens = line.Split(':');
            if (tokens[0] == "  bundleVersion")
            {
                curProductVersion = tokens[1].Trim();
            }
            else if (tokens[0] == "  AndroidBundleVersionCode")
            {
                curBundleVersion = tokens[1].Trim();
            }
        }
        if (curProductVersion == "" || curBundleVersion == "")
        {
            AddLine("ERROR", $"Could not find 'version' or 'bundle' from {path}");
            return false;
        }
        if (curProductVersion != productVersion || curBundleVersion != bundleVersion)
        {
            AddLine("ERROR",
                $"ProjectSettings.asset does not have 'version' {productVersion} or 'bundle' {bundleVersion}");
            AddLine(".ERROR", $"Current values are 'version' {curProductVersion} or 'bundle' {curBundleVersion}");
            return false;
        }
        if (versionIsDate)
        {
            curProductVersion = $"{DateTime.Today:dd.MM.yyyy}";
        }
        var bundleVersionValue = int.Parse(curBundleVersion) + 1;

        productVersion = curProductVersion;
        bundleVersion = bundleVersionValue.ToString();
        var updateCount = 0;
        for (var i = 0; i < lines.Length; ++i)
        {
            var line = lines[i];
            var tokens = line.Split(':');
            if (tokens[0] == "  bundleVersion")
            {
                lines[i] = $"  bundleVersion: {productVersion}";
                updateCount += 1;
            }
            else if (tokens[0] == "  AndroidBundleVersionCode")
            {
                lines[i] = $"  AndroidBundleVersionCode: {bundleVersion}";
                updateCount += 1;
            }
        }
        if (updateCount != 2)
        {
            AddLine("ERROR", $"Unable to update 'version' or 'bundle' in {path}");
            return false;
        }
        var output = string.Join('\n', lines);
        File.WriteAllText(path, output);
        return true;
    }
}
