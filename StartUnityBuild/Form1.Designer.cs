namespace StartUnityBuild;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
        menuStrip1 = new MenuStrip();
        fileToolStripMenuItem = new ToolStripMenuItem();
        setProjectFolderToolStripMenuItem = new ToolStripMenuItem();
        deleteUNITYLibraryFolderToolStripMenuItem = new ToolStripMenuItem();
        copyOutputToClipboardToolStripMenuItem = new ToolStripMenuItem();
        exitToolStripMenuItem = new ToolStripMenuItem();
        openDebugLogToolStripMenuItem = new ToolStripMenuItem();
        gitStatusToolStripMenuItem = new ToolStripMenuItem();
        gitPullToolStripMenuItem = new ToolStripMenuItem();
        updateBuildToolStripMenuItem = new ToolStripMenuItem();
        gitPushToolStripMenuItem = new ToolStripMenuItem();
        startBuildToolStripMenuItem = new ToolStripMenuItem();
        postProcessToolStripMenuItem = new ToolStripMenuItem();
        projectInfoToolStripMenuItem = new ToolStripMenuItem();
        listView1 = new ListView();
        label1 = new Label();
        timer1 = new System.Windows.Forms.Timer(components);
        fileSystemWatcher1 = new FileSystemWatcher();
        menuStrip1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize) fileSystemWatcher1).BeginInit();
        SuspendLayout();
        // 
        // menuStrip1
        // 
        menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, gitStatusToolStripMenuItem, gitPullToolStripMenuItem, updateBuildToolStripMenuItem, gitPushToolStripMenuItem, startBuildToolStripMenuItem, postProcessToolStripMenuItem, projectInfoToolStripMenuItem });
        menuStrip1.Location = new Point(0, 0);
        menuStrip1.Name = "menuStrip1";
        menuStrip1.Size = new Size(1063, 24);
        menuStrip1.TabIndex = 0;
        menuStrip1.Text = "menuStrip1";
        // 
        // fileToolStripMenuItem
        // 
        fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { setProjectFolderToolStripMenuItem, copyOutputToClipboardToolStripMenuItem, openDebugLogToolStripMenuItem, deleteUNITYLibraryFolderToolStripMenuItem, exitToolStripMenuItem });
        fileToolStripMenuItem.Name = "fileToolStripMenuItem";
        fileToolStripMenuItem.Size = new Size(37, 20);
        fileToolStripMenuItem.Text = "File";
        // 
        // setProjectFolderToolStripMenuItem
        // 
        setProjectFolderToolStripMenuItem.Name = "setProjectFolderToolStripMenuItem";
        setProjectFolderToolStripMenuItem.Size = new Size(216, 22);
        setProjectFolderToolStripMenuItem.Text = "Set Project Folder";
        // 
        // deleteUNITYLibraryFolderToolStripMenuItem
        // 
        deleteUNITYLibraryFolderToolStripMenuItem.Name = "deleteUNITYLibraryFolderToolStripMenuItem";
        deleteUNITYLibraryFolderToolStripMenuItem.Size = new Size(216, 22);
        deleteUNITYLibraryFolderToolStripMenuItem.Text = "Delete UNITY Library folder";
        // 
        // copyOutputToClipboardToolStripMenuItem
        // 
        copyOutputToClipboardToolStripMenuItem.Name = "copyOutputToClipboardToolStripMenuItem";
        copyOutputToClipboardToolStripMenuItem.Size = new Size(216, 22);
        copyOutputToClipboardToolStripMenuItem.Text = "Copy Output to Clipboard";
        // 
        // exitToolStripMenuItem
        // 
        exitToolStripMenuItem.Name = "exitToolStripMenuItem";
        exitToolStripMenuItem.Size = new Size(216, 22);
        exitToolStripMenuItem.Text = "Exit";
        // 
        // openDebugLogToolStripMenuItem
        // 
        openDebugLogToolStripMenuItem.Name = "openDebugLogToolStripMenuItem";
        openDebugLogToolStripMenuItem.Size = new Size(216, 22);
        openDebugLogToolStripMenuItem.Text = "Open Debug Log";
        // 
        // gitStatusToolStripMenuItem
        // 
        gitStatusToolStripMenuItem.Name = "gitStatusToolStripMenuItem";
        gitStatusToolStripMenuItem.Size = new Size(69, 20);
        gitStatusToolStripMenuItem.Text = "Git Status";
        // 
        // gitPullToolStripMenuItem
        // 
        gitPullToolStripMenuItem.Name = "gitPullToolStripMenuItem";
        gitPullToolStripMenuItem.Size = new Size(57, 20);
        gitPullToolStripMenuItem.Text = "Git Pull";
        // 
        // updateBuildToolStripMenuItem
        // 
        updateBuildToolStripMenuItem.Name = "updateBuildToolStripMenuItem";
        updateBuildToolStripMenuItem.Size = new Size(87, 20);
        updateBuildToolStripMenuItem.Text = "Update Build";
        // 
        // gitPushToolStripMenuItem
        // 
        gitPushToolStripMenuItem.Name = "gitPushToolStripMenuItem";
        gitPushToolStripMenuItem.Size = new Size(63, 20);
        gitPushToolStripMenuItem.Text = "Git Push";
        // 
        // startBuildToolStripMenuItem
        // 
        startBuildToolStripMenuItem.Name = "startBuildToolStripMenuItem";
        startBuildToolStripMenuItem.Size = new Size(73, 20);
        startBuildToolStripMenuItem.Text = "Start Build";
        // 
        // postProcessToolStripMenuItem
        // 
        postProcessToolStripMenuItem.Name = "postProcessToolStripMenuItem";
        postProcessToolStripMenuItem.Size = new Size(85, 20);
        postProcessToolStripMenuItem.Text = "Post Process";
        // 
        // projectInfoToolStripMenuItem
        // 
        projectInfoToolStripMenuItem.Name = "projectInfoToolStripMenuItem";
        projectInfoToolStripMenuItem.Size = new Size(83, 20);
        projectInfoToolStripMenuItem.Text = "Project Info!";
        // 
        // listView1
        // 
        listView1.Dock = DockStyle.Fill;
        listView1.Location = new Point(0, 24);
        listView1.Name = "listView1";
        listView1.Size = new Size(1063, 539);
        listView1.TabIndex = 1;
        listView1.UseCompatibleStateImageBehavior = false;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.BackColor = SystemColors.ControlLightLight;
        label1.Dock = DockStyle.Right;
        label1.Font = new Font("Cascadia Mono", 9.75F, FontStyle.Regular, GraphicsUnit.Point,  0);
        label1.Location = new Point(943, 24);
        label1.Name = "label1";
        label1.Size = new Size(120, 17);
        label1.TabIndex = 2;
        label1.Text = "Building 00:00";
        label1.TextAlign = ContentAlignment.MiddleRight;
        // 
        // fileSystemWatcher1
        // 
        fileSystemWatcher1.EnableRaisingEvents = true;
        fileSystemWatcher1.SynchronizingObject = this;
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1063, 563);
        Controls.Add(label1);
        Controls.Add(listView1);
        Controls.Add(menuStrip1);
        Icon = (Icon) resources.GetObject("$this.Icon");
        MainMenuStrip = menuStrip1;
        Name = "Form1";
        Text = "Form1";
        menuStrip1.ResumeLayout(false);
        menuStrip1.PerformLayout();
        ((System.ComponentModel.ISupportInitialize) fileSystemWatcher1).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private MenuStrip menuStrip1;
    private ToolStripMenuItem fileToolStripMenuItem;
    private ToolStripMenuItem exitToolStripMenuItem;
    private ListView listView1;
    private ToolStripMenuItem gitStatusToolStripMenuItem;
    private ToolStripMenuItem startBuildToolStripMenuItem;
    private ToolStripMenuItem copyOutputToClipboardToolStripMenuItem;
    private Label label1;
    private System.Windows.Forms.Timer timer1;
    private FileSystemWatcher fileSystemWatcher1;
    private ToolStripMenuItem updateBuildToolStripMenuItem;
    private ToolStripMenuItem projectInfoToolStripMenuItem;
    private ToolStripMenuItem setProjectFolderToolStripMenuItem;
    private ToolStripMenuItem gitPullToolStripMenuItem;
    private ToolStripMenuItem gitPushToolStripMenuItem;
    private ToolStripMenuItem postProcessToolStripMenuItem;
    private ToolStripMenuItem deleteUNITYLibraryFolderToolStripMenuItem;
    private ToolStripMenuItem openDebugLogToolStripMenuItem;
}
