namespace StartUnityBuild;

public static class MenuExtensions
{
    public static void SubMenu(this ToolStripMenuItem menu, string text, Action? onCLick,
        bool enabled = true, bool @checked = false)
    {
        var menuItem = new ToolStripMenuItem(text);
        menuItem.Enabled = enabled;
        menuItem.Checked = @checked;
        menuItem.Click += (_, _) => onCLick?.Invoke();
        menu.DropDownItems.Add(menuItem);
    }

    public static void SubMenu(this ToolStripMenuItem menu, string text, Action<object, EventArgs> onCLick,
        bool enabled = true, bool @checked = false)
    {
        var menuItem = new ToolStripMenuItem(text);
        menuItem.Enabled = enabled;
        menuItem.Checked = @checked;
        menuItem.Click += (o, e) => onCLick(o!, e);
        menu.DropDownItems.Add(menuItem);
    }
}
