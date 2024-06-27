namespace StartUnityBuild;

/// <summary>
/// JSON serialized settings class.
/// </summary>
public class AppSettings
{
    private static string Filename => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        Application.ProductName ?? "StartUnityBuild",
        "StartUnityBuild.settings.json");

    private static AppSettings? _instance;

    public static AppSettings Get()
    {
        return _instance ??= Serializer.LoadStateJson<AppSettings>(Filename) ?? new AppSettings();
    }

    public static void SetUnityProjectFolder(string folderName)
    {
        var appSettings = Get();
        if (appSettings.UnityProjectFolder == folderName)
        {
            return;
        }
        appSettings.UnityProjectFolder = folderName;
        appSettings.Save();
    }

    // ReSharper disable MemberCanBePrivate.Global
    public string UnityProjectFolder { get; set; } = "";
    // ReSharper restore MemberCanBePrivate.Global

    private void Save()
    {
        Serializer.SaveStateJson(this, Filename);
    }
}
