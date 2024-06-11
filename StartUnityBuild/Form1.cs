using System.Diagnostics.CodeAnalysis;

namespace StartUnityBuild;

[SuppressMessage("ReSharper", "LocalizableElement")]
public partial class Form1 : Form
{
    private static Form1 _instance = null!;
    private string _currentDirectory;
    private string _unityVersion;

    public Form1()
    {
        _instance = this;
        _currentDirectory = Directory.GetCurrentDirectory();
        InitializeComponent();
        listView1.Font = new Font("Cascadia Mono", 10);
        listView1.View = View.List;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Text = "UNITY Build";
        exitToolStripMenuItem.Click += (o, e) => Application.Exit();
        try
        {
            LoadEnvironment();
        }
        catch (Exception x)
        {
            AddLine($"Failed to LoadEnvironment");
            AddLine($"Error: {x.Message}");
        }
        Text = $"UNITY {_unityVersion} Build";
    }

    private void LoadEnvironment()
    {
        AddLine("CWD", $"{_currentDirectory}");
        LoadProjectVersionFile();
        AddLine("Unity", $"{_unityVersion}");
    }

    public static void AddLine(string prefix, string content)
    {
        AddLine($"{prefix,-12}: {content}");
    }

    public static void AddLine(string line)
    {
        if (_instance.InvokeRequired)
        {
            _instance.Invoke(() => AddLine(line));
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
}
