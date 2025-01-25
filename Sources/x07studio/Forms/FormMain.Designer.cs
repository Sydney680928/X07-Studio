namespace x07studio.Forms
{
    partial class FormMain
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
            ToolStripMenuItem DisplayMenu;
            ToolStripMenuItem StockageMenu;
            DisplayProjectMenu = new ToolStripMenuItem();
            ProgramEditorMenu = new ToolStripMenuItem();
            FontEditorMenu = new ToolStripMenuItem();
            Z80EditorMenu = new ToolStripMenuItem();
            LoadProgramMenu = new ToolStripMenuItem();
            SaveProgramMenu = new ToolStripMenuItem();
            menuStrip1 = new MenuStrip();
            SettingsMenu = new ToolStripMenuItem();
            AboutMenu = new ToolStripMenuItem();
            MemoryManagerMenu = new ToolStripMenuItem();
            DisplayMenu = new ToolStripMenuItem();
            StockageMenu = new ToolStripMenuItem();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // DisplayMenu
            // 
            DisplayMenu.DropDownItems.AddRange(new ToolStripItem[] { DisplayProjectMenu, ProgramEditorMenu, FontEditorMenu, Z80EditorMenu, MemoryManagerMenu });
            DisplayMenu.Name = "DisplayMenu";
            DisplayMenu.Size = new Size(61, 24);
            DisplayMenu.Text = "Afficher";
            // 
            // DisplayProjectMenu
            // 
            DisplayProjectMenu.Name = "DisplayProjectMenu";
            DisplayProjectMenu.Size = new Size(207, 22);
            DisplayProjectMenu.Text = "Projet X07";
            DisplayProjectMenu.Click += DisplayProjectMenu_Click;
            // 
            // ProgramEditorMenu
            // 
            ProgramEditorMenu.Name = "ProgramEditorMenu";
            ProgramEditorMenu.Size = new Size(207, 22);
            ProgramEditorMenu.Text = "Editeur BASIC";
            ProgramEditorMenu.Click += ProgramEditorMenu_Click;
            // 
            // FontEditorMenu
            // 
            FontEditorMenu.Name = "FontEditorMenu";
            FontEditorMenu.Size = new Size(207, 22);
            FontEditorMenu.Text = "Editeur de symboles";
            FontEditorMenu.Click += FontEditorMenu_Click;
            // 
            // Z80EditorMenu
            // 
            Z80EditorMenu.Name = "Z80EditorMenu";
            Z80EditorMenu.Size = new Size(207, 22);
            Z80EditorMenu.Text = "Editeur Z80";
            Z80EditorMenu.Click += Z80EditorMenu_Click;
            // 
            // StockageMenu
            // 
            StockageMenu.DropDownItems.AddRange(new ToolStripItem[] { LoadProgramMenu, SaveProgramMenu });
            StockageMenu.Name = "StockageMenu";
            StockageMenu.Size = new Size(67, 24);
            StockageMenu.Text = "Stockage";
            // 
            // LoadProgramMenu
            // 
            LoadProgramMenu.Name = "LoadProgramMenu";
            LoadProgramMenu.Size = new Size(213, 22);
            LoadProgramMenu.Text = "Charger un programme";
            LoadProgramMenu.Click += LoadProgramMenu_Click;
            // 
            // SaveProgramMenu
            // 
            SaveProgramMenu.Name = "SaveProgramMenu";
            SaveProgramMenu.Size = new Size(213, 22);
            SaveProgramMenu.Text = "Enregistrer un programme";
            SaveProgramMenu.Click += SaveProgramMenu_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.AutoSize = false;
            menuStrip1.BackColor = Color.FromArgb(204, 213, 240);
            menuStrip1.ImageScalingSize = new Size(24, 24);
            menuStrip1.Items.AddRange(new ToolStripItem[] { DisplayMenu, StockageMenu, SettingsMenu, AboutMenu });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(4, 3, 0, 3);
            menuStrip1.Size = new Size(1107, 30);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // SettingsMenu
            // 
            SettingsMenu.Name = "SettingsMenu";
            SettingsMenu.Size = new Size(87, 24);
            SettingsMenu.Text = "Paramètres...";
            SettingsMenu.Click += SettingsMenu_Click;
            // 
            // AboutMenu
            // 
            AboutMenu.Name = "AboutMenu";
            AboutMenu.Size = new Size(76, 24);
            AboutMenu.Text = "A propos...";
            AboutMenu.Click += AboutMenu_Click;
            // 
            // MemoryManagerMenu
            // 
            MemoryManagerMenu.Name = "MemoryManagerMenu";
            MemoryManagerMenu.Size = new Size(207, 22);
            MemoryManagerMenu.Text = "Gestionnaire de mémoire";
            MemoryManagerMenu.Click += MemoryManagerMenu_Click;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1107, 591);
            Controls.Add(menuStrip1);
            IsMdiContainer = true;
            MainMenuStrip = menuStrip1;
            Margin = new Padding(2, 2, 2, 2);
            Name = "FormMain";
            Text = "X07 STUDIO";
            FormClosing += FormMain_FormClosing;
            Load += FormMain_Load;
            ResizeEnd += FormMain_ResizeEnd;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem ProjectMenu;
        private ToolStripMenuItem FileNewMenu;
        private ToolStripMenuItem FileOpenMenu;
        private ToolStripMenuItem FileExitMenu;
        private ToolStripMenuItem FileSaveMenu;
        private ToolStripMenuItem FileSaveAsMenu;
        private ToolStripMenuItem DisplayMenu;
        private ToolStripMenuItem DisplayProjectMenu;
        private ToolStripMenuItem AboutMenu;
        private ToolStripMenuItem StockageMenu;
        private ToolStripMenuItem LoadProgramMenu;
        private ToolStripMenuItem SaveProgramMenu;
        private ToolStripMenuItem FontEditorMenu;
        private ToolStripMenuItem ProgramEditorMenu;
        private ToolStripMenuItem Z80EditorMenu;
        private ToolStripMenuItem SettingsMenu;
        private ToolStripMenuItem MemoryManagerMenu;
    }
}
