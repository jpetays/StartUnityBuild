using System.Diagnostics.CodeAnalysis;

namespace StartUnityBuild;

[SuppressMessage("ReSharper", "LocalizableElement")]
public partial class Form1 : Form
{
    private static Form1 _instance = null!;
    private string _currentDirectory;
    private string _unityVersion;
    private string _productName;
    private string _productVersion;
    private string _bundleVersion;

    public Form1()
    {
        _instance = this;
        _currentDirectory = Directory.GetCurrentDirectory();
        InitializeComponent();
        listView1.Font = new Font("Cascadia Mono", 10);
        listView1.View = View.List;
        fileToolStripMenuItem.SubMenu("Git Status", () =>
        {
            ClearLines();
            Commands.GitStatus();
        });
        exitToolStripMenuItem.Click += (o, e) => Application.Exit();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Text = "UNITY Build";
        try
        {
            LoadEnvironment();
            AddLine($">git status");
            Commands.GitStatus();
        }
        catch (Exception x)
        {
            AddLine($"Failed to LoadEnvironment");
            AddLine("Error", $"{x.Message}");
            if (x.StackTrace != null)
            {
                foreach (var line in x.StackTrace.Split(new char[] { '\r', '\n' },
                             StringSplitOptions.RemoveEmptyEntries))
                {
                    AddLine(line.Trim());
                }
            }
        }
        Text = $"UNITY {_unityVersion} Build : {_productName} ver {_productVersion} bundle {_bundleVersion}";
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
    }

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
        AddLine($"{prefix,-12}: {content}");
    }

    public static void ClearLines()
    {
        if (_instance.InvokeRequired)
        {
            _instance.Invoke(() => ClearLines);
            return;
        }
        var listView = _instance.listView1;
        listView.BeginUpdate();
        listView.Items.Clear();
        listView.EndUpdate();
        listView.EnsureVisible(listView.Items.Count - 1);
    }

    public static void AddLine(string line)
    {
        if (_instance.InvokeRequired)
        {
            _instance.Invoke(() => AddLine(line));
            return;
        }
        var listView = _instance.listView1;
        listView.BeginUpdate();
        listView.Items.Add(line);
        listView.EndUpdate();
        listView.EnsureVisible(listView.Items.Count - 1);
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
