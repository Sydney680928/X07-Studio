namespace x07studio.Forms
{
    partial class FormAsmEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            MenuStrip menuStrip1;
            ColumnHeader Column0;
            ColumnHeader Column1;
            ColumnHeader Column2;
            ColumnHeader Column3;
            ColumnHeader Column4;
            fichierToolStripMenuItem = new ToolStripMenuItem();
            FileNewMenu = new ToolStripMenuItem();
            FileOpenMenu = new ToolStripMenuItem();
            FileSaveMenu = new ToolStripMenuItem();
            FileSaveAsMenu = new ToolStripMenuItem();
            CompileMenu = new ToolStripMenuItem();
            CreateLoaderMenu = new ToolStripMenuItem();
            CodeEditor = new ICSharpCode.TextEditor.TextEditorControl();
            AsmEditorTab = new TabControl();
            CodePage = new TabPage();
            ComputerPage = new TabPage();
            AsmListView = new ListView();
            Column5 = new ColumnHeader();
            menuStrip1 = new MenuStrip();
            Column0 = new ColumnHeader();
            Column1 = new ColumnHeader();
            Column2 = new ColumnHeader();
            Column3 = new ColumnHeader();
            Column4 = new ColumnHeader();
            menuStrip1.SuspendLayout();
            AsmEditorTab.SuspendLayout();
            CodePage.SuspendLayout();
            ComputerPage.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.AllowMerge = false;
            menuStrip1.AutoSize = false;
            menuStrip1.BackColor = Color.FromArgb(204, 213, 240);
            menuStrip1.ImageScalingSize = new Size(24, 24);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fichierToolStripMenuItem, CompileMenu, CreateLoaderMenu });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(9, 5, 0, 5);
            menuStrip1.Size = new Size(1220, 50);
            menuStrip1.TabIndex = 6;
            menuStrip1.Text = "menuStrip1";
            // 
            // fichierToolStripMenuItem
            // 
            fichierToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { FileNewMenu, FileOpenMenu, FileSaveMenu, FileSaveAsMenu });
            fichierToolStripMenuItem.Name = "fichierToolStripMenuItem";
            fichierToolStripMenuItem.Size = new Size(78, 40);
            fichierToolStripMenuItem.Text = "Fichier";
            // 
            // FileNewMenu
            // 
            FileNewMenu.Name = "FileNewMenu";
            FileNewMenu.Size = new Size(246, 34);
            FileNewMenu.Text = "Nouveau";
            FileNewMenu.Click += FileNewMenu_Click;
            // 
            // FileOpenMenu
            // 
            FileOpenMenu.Name = "FileOpenMenu";
            FileOpenMenu.Size = new Size(246, 34);
            FileOpenMenu.Text = "Ouvrir...";
            FileOpenMenu.Click += FileOpenMenu_Click;
            // 
            // FileSaveMenu
            // 
            FileSaveMenu.Name = "FileSaveMenu";
            FileSaveMenu.Size = new Size(246, 34);
            FileSaveMenu.Text = "Enregistrer";
            FileSaveMenu.Click += FileSaveMenu_Click;
            // 
            // FileSaveAsMenu
            // 
            FileSaveAsMenu.Name = "FileSaveAsMenu";
            FileSaveAsMenu.Size = new Size(246, 34);
            FileSaveAsMenu.Text = "Enregister sous...";
            FileSaveAsMenu.Click += FileSaveAsMenu_Click;
            // 
            // CompileMenu
            // 
            CompileMenu.Name = "CompileMenu";
            CompileMenu.Size = new Size(100, 40);
            CompileMenu.Text = "Compiler";
            CompileMenu.Click += CompileMenu_Click;
            // 
            // CreateLoaderMenu
            // 
            CreateLoaderMenu.Name = "CreateLoaderMenu";
            CreateLoaderMenu.Size = new Size(163, 40);
            CreateLoaderMenu.Text = "Générer chargeur";
            CreateLoaderMenu.Click += CreateLoaderMenu_Click;
            // 
            // Column0
            // 
            Column0.Text = "Source";
            // 
            // Column1
            // 
            Column1.Text = "Adresse";
            Column1.TextAlign = HorizontalAlignment.Center;
            // 
            // Column2
            // 
            Column2.Text = "Opération";
            // 
            // Column3
            // 
            Column3.Text = "Code machine";
            // 
            // Column4
            // 
            Column4.Text = "Taille";
            Column4.TextAlign = HorizontalAlignment.Right;
            // 
            // CodeEditor
            // 
            CodeEditor.BorderStyle = BorderStyle.FixedSingle;
            CodeEditor.Dock = DockStyle.Fill;
            CodeEditor.Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            CodeEditor.IsReadOnly = false;
            CodeEditor.Location = new Point(3, 3);
            CodeEditor.Name = "CodeEditor";
            CodeEditor.Size = new Size(1173, 731);
            CodeEditor.TabIndex = 4;
            CodeEditor.TextChanged += CodeEditor_TextChanged;
            // 
            // AsmEditorTab
            // 
            AsmEditorTab.Controls.Add(CodePage);
            AsmEditorTab.Controls.Add(ComputerPage);
            AsmEditorTab.Location = new Point(12, 64);
            AsmEditorTab.Name = "AsmEditorTab";
            AsmEditorTab.Padding = new Point(30, 5);
            AsmEditorTab.SelectedIndex = 0;
            AsmEditorTab.Size = new Size(1187, 779);
            AsmEditorTab.TabIndex = 7;
            // 
            // CodePage
            // 
            CodePage.Controls.Add(CodeEditor);
            CodePage.Location = new Point(4, 38);
            CodePage.Name = "CodePage";
            CodePage.Padding = new Padding(3);
            CodePage.Size = new Size(1179, 737);
            CodePage.TabIndex = 0;
            CodePage.Text = "Code Assembleur";
            CodePage.UseVisualStyleBackColor = true;
            // 
            // ComputerPage
            // 
            ComputerPage.Controls.Add(AsmListView);
            ComputerPage.Location = new Point(4, 38);
            ComputerPage.Name = "ComputerPage";
            ComputerPage.Padding = new Padding(3);
            ComputerPage.Size = new Size(1179, 737);
            ComputerPage.TabIndex = 1;
            ComputerPage.Text = "Code Machine";
            ComputerPage.UseVisualStyleBackColor = true;
            // 
            // AsmListView
            // 
            AsmListView.Columns.AddRange(new ColumnHeader[] { Column0, Column1, Column2, Column3, Column4, Column5 });
            AsmListView.Dock = DockStyle.Fill;
            AsmListView.Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            AsmListView.FullRowSelect = true;
            AsmListView.GridLines = true;
            AsmListView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            AsmListView.Location = new Point(3, 3);
            AsmListView.MultiSelect = false;
            AsmListView.Name = "AsmListView";
            AsmListView.ShowGroups = false;
            AsmListView.Size = new Size(1173, 731);
            AsmListView.TabIndex = 0;
            AsmListView.UseCompatibleStateImageBehavior = false;
            AsmListView.View = View.Details;
            // 
            // Column5
            // 
            Column5.Text = "";
            // 
            // FormAsmEditor
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1220, 855);
            Controls.Add(AsmEditorTab);
            Controls.Add(menuStrip1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormAsmEditor";
            Text = "EDITEUR ASSEMBLEUR Z80";
            FormClosing += FormAsmEditor_FormClosing;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            AsmEditorTab.ResumeLayout(false);
            CodePage.ResumeLayout(false);
            ComputerPage.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private ICSharpCode.TextEditor.TextEditorControl CodeEditor;
        private ToolStripMenuItem fichierToolStripMenuItem;
        private ToolStripMenuItem FileNewMenu;
        private ToolStripMenuItem FileOpenMenu;
        private ToolStripMenuItem FileSaveMenu;
        private ToolStripMenuItem FileSaveAsMenu;
        private ToolStripMenuItem CompileMenu;
        private TabControl AsmEditorTab;
        private TabPage CodePage;
        private TabPage ComputerPage;
        private ListView AsmListView;
        private ColumnHeader Column0;
        private ColumnHeader Column1;
        private ColumnHeader Column2;
        private ToolStripMenuItem CreateLoaderMenu;
        private ColumnHeader Column5;
    }
}