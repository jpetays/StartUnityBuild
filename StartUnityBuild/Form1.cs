using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace StartUnityBuild;

[SuppressMessage("ReSharper", "LocalizableElement")]
public partial class Form1 : Form
{
    private static readonly char[] Separators = ['\r', '\n'];

    private static Form1 _instance = null!;
    private readonly string _currentDirectory;
    private string? _unityVersion;
    private string? _productName;
    private string? _productVersion;
    private string? _bundleVersion;
    private readonly List<string> _buildTargets = new();

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

        copyOutputToClipboardToolStripMenuItem.Click += (_, _) => CopyLines();
        exitToolStripMenuItem.Click += (_, _) => Application.Exit();
        var isCommandExecuting = false;
        gitStatusToolStripMenuItem.Click += (_, _) => ExecuteMenuCommand(() =>
        {
            if (isCommandExecuting)
            {
                MessageBox.Show("A command is already executing", "UNITY Build", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }
            SetStatus("Executing 00:00", Color.Green);
            ClearLines();
            isCommandExecuting = true;
            Commands.GitStatus(_currentDirectory, () =>
            {
                SetStatus("Done", Color.Blue);
                isCommandExecuting = false;
            });
        });
        startBuildToolStripMenuItem.Click += (_, _) => ExecuteMenuCommand(() =>
        {
            if (isCommandExecuting)
            {
                MessageBox.Show("A command is already executing", "UNITY Build", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }
            SetStatus("Building 00:00", Color.Green);
            ClearLines();
            isCommandExecuting = true;
            var startTime = DateTime.Now;
            Commands.UnityBuild(_currentDirectory, _buildTargets, () =>
            {
                var duration = DateTime.Now - startTime;
                SetStatus($"Done in {duration:mm':'ss}", Color.Blue);
                isCommandExecuting = false;
            });
        });
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Text = "UNITY Build";
        try
        {
            LoadEnvironment();
            Text = $"UNITY {_unityVersion} Build : {_productName} ver {_productVersion} bundle {_bundleVersion}";
            StartupCommand();
        }
        catch (Exception x)
        {
            AddLine($"Failed to LoadEnvironment");
            AddLine("Error", $"{x.Message}");
            if (x.StackTrace != null)
            {
                foreach (var line in x.StackTrace.Split(Separators, StringSplitOptions.RemoveEmptyEntries))
                {
                    AddLine(line.Trim());
                }
            }
        }
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
            AddLine("Error", $"{x.Message}");
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
        LoadProjectVersionFile();
        AddLine("Unity", $"{_unityVersion}");
        LoadProjectSettingsFile();
        AddLine("Product", $"{_productName}");
        AddLine("Version", $"{_productVersion}");
        AddLine("Bundle", $"{_bundleVersion}");
        _buildTargets.Add("Win64");
        AddLine("Builds", $"{string.Join(", ", _buildTargets)}");
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
        Color? color = prefix == "ERROR" ? Color.Red
            : prefix.StartsWith('.') ? Color.Gray
            : prefix.StartsWith('>') ? Color.Blue
            : content.StartsWith('-') ? Color.Magenta
            : content.Contains("SUCCESSFULLY") ? Color.Green
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

    public static void AddLine(string line, Color? color = null)
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

    private void LoadProjectSettingsFile()
    {
        var path = Path.Combine(_currentDirectory, "ProjectSettings", "ProjectSettings.asset");
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
}
