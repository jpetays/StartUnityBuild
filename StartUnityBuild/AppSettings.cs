namespace StartUnityBuild;

public class AppSettings
{
    private static string Filename => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        Application.ProductName ?? "StartUnityBuild",
        "StartUnityBuild.settings.json");

    public static AppSettings Instance;

    public string UnityProjectFolder { get; set; } = "";

    public static void Load()
    {
        Instance = Serializer.LoadStateJson<AppSettings>(Filename) ?? new AppSettings();
    }

    public void Save()
    {
        Serializer.SaveStateJson(this, Filename);
    }
}
