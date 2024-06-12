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
        menuStrip1 = new MenuStrip();
        fileToolStripMenuItem = new ToolStripMenuItem();
        copyOutputToClipboardToolStripMenuItem = new ToolStripMenuItem();
        exitToolStripMenuItem = new ToolStripMenuItem();
        gitStatusToolStripMenuItem = new ToolStripMenuItem();
        startBuildToolStripMenuItem = new ToolStripMenuItem();
        listView1 = new ListView();
        label1 = new Label();
        menuStrip1.SuspendLayout();
        SuspendLayout();
        // 
        // menuStrip1
        // 
        menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, gitStatusToolStripMenuItem, startBuildToolStripMenuItem });
        menuStrip1.Location = new Point(0, 0);
        menuStrip1.Name = "menuStrip1";
        menuStrip1.Size = new Size(1063, 24);
        menuStrip1.TabIndex = 0;
        menuStrip1.Text = "menuStrip1";
        // 
        // fileToolStripMenuItem
        // 
        fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { copyOutputToClipboardToolStripMenuItem, exitToolStripMenuItem });
        fileToolStripMenuItem.Name = "fileToolStripMenuItem";
        fileToolStripMenuItem.Size = new Size(37, 20);
        fileToolStripMenuItem.Text = "File";
        // 
        // copyOutputToClipboardToolStripMenuItem
        // 
        copyOutputToClipboardToolStripMenuItem.Name = "copyOutputToClipboardToolStripMenuItem";
        copyOutputToClipboardToolStripMenuItem.Size = new Size(212, 22);
        copyOutputToClipboardToolStripMenuItem.Text = "Copy Output to Clipboard";
        // 
        // exitToolStripMenuItem
        // 
        exitToolStripMenuItem.Name = "exitToolStripMenuItem";
        exitToolStripMenuItem.Size = new Size(212, 22);
        exitToolStripMenuItem.Text = "Exit";
        // 
        // gitStatusToolStripMenuItem
        // 
        gitStatusToolStripMenuItem.Name = "gitStatusToolStripMenuItem";
        gitStatusToolStripMenuItem.Size = new Size(69, 20);
        gitStatusToolStripMenuItem.Text = "Git Status";
        // 
        // startBuildToolStripMenuItem
        // 
        startBuildToolStripMenuItem.Name = "startBuildToolStripMenuItem";
        startBuildToolStripMenuItem.Size = new Size(73, 20);
        startBuildToolStripMenuItem.Text = "Start Build";
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
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1063, 563);
        Controls.Add(label1);
        Controls.Add(listView1);
        Controls.Add(menuStrip1);
        MainMenuStrip = menuStrip1;
        Name = "Form1";
        Text = "Form1";
        menuStrip1.ResumeLayout(false);
        menuStrip1.PerformLayout();
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
}
