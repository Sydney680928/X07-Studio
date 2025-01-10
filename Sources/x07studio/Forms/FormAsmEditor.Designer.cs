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
            menuStrip1.ImageScalingSize = new Size(24, 24);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fichierToolStripMenuItem, CompileMenu, CreateLoaderMenu });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(854, 24);
            menuStrip1.TabIndex = 6;
            menuStrip1.Text = "menuStrip1";
            // 
            // fichierToolStripMenuItem
            // 
            fichierToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { FileNewMenu, FileOpenMenu, FileSaveMenu, FileSaveAsMenu });
            fichierToolStripMenuItem.Name = "fichierToolStripMenuItem";
            fichierToolStripMenuItem.Size = new Size(54, 20);
            fichierToolStripMenuItem.Text = "Fichier";
            // 
            // FileNewMenu
            // 
            FileNewMenu.Name = "FileNewMenu";
            FileNewMenu.Size = new Size(162, 22);
            FileNewMenu.Text = "Nouveau";
            FileNewMenu.Click += FileNewMenu_Click;
            // 
            // FileOpenMenu
            // 
            FileOpenMenu.Name = "FileOpenMenu";
            FileOpenMenu.Size = new Size(162, 22);
            FileOpenMenu.Text = "Ouvrir...";
            FileOpenMenu.Click += FileOpenMenu_Click;
            // 
            // FileSaveMenu
            // 
            FileSaveMenu.Name = "FileSaveMenu";
            FileSaveMenu.Size = new Size(162, 22);
            FileSaveMenu.Text = "Enregistrer";
            FileSaveMenu.Click += FileSaveMenu_Click;
            // 
            // FileSaveAsMenu
            // 
            FileSaveAsMenu.Name = "FileSaveAsMenu";
            FileSaveAsMenu.Size = new Size(162, 22);
            FileSaveAsMenu.Text = "Enregister sous...";
            FileSaveAsMenu.Click += FileSaveAsMenu_Click;
            // 
            // CompileMenu
            // 
            CompileMenu.Name = "CompileMenu";
            CompileMenu.Size = new Size(68, 20);
            CompileMenu.Text = "Compiler";
            CompileMenu.Click += CompileMenu_Click;
            // 
            // CreateLoaderMenu
            // 
            CreateLoaderMenu.Name = "CreateLoaderMenu";
            CreateLoaderMenu.Size = new Size(110, 20);
            CreateLoaderMenu.Text = "Générer chargeur";
            CreateLoaderMenu.Click += CreateLoaderMenu_Click;
            // 
            // Column0
            // 
            Column0.Text = "Source";
            // 
            // Column1
            // 
            Column1.Text = "Address";
            // 
            // Column2
            // 
            Column2.Text = "Code";
            // 
            // Column3
            // 
            Column3.Text = "Hexa";
            // 
            // Column4
            // 
            Column4.Text = "";
            // 
            // CodeEditor
            // 
            CodeEditor.BorderStyle = BorderStyle.FixedSingle;
            CodeEditor.Dock = DockStyle.Fill;
            CodeEditor.Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            CodeEditor.IsReadOnly = false;
            CodeEditor.Location = new Point(2, 2);
            CodeEditor.Margin = new Padding(2);
            CodeEditor.Name = "CodeEditor";
            CodeEditor.Size = new Size(842, 453);
            CodeEditor.TabIndex = 4;
            CodeEditor.TextChanged += CodeEditor_TextChanged;
            // 
            // AsmEditorTab
            // 
            AsmEditorTab.Controls.Add(CodePage);
            AsmEditorTab.Controls.Add(ComputerPage);
            AsmEditorTab.Dock = DockStyle.Fill;
            AsmEditorTab.Location = new Point(0, 24);
            AsmEditorTab.Margin = new Padding(2);
            AsmEditorTab.Name = "AsmEditorTab";
            AsmEditorTab.Padding = new Point(30, 5);
            AsmEditorTab.SelectedIndex = 0;
            AsmEditorTab.Size = new Size(854, 489);
            AsmEditorTab.TabIndex = 7;
            // 
            // CodePage
            // 
            CodePage.Controls.Add(CodeEditor);
            CodePage.Location = new Point(4, 28);
            CodePage.Margin = new Padding(2);
            CodePage.Name = "CodePage";
            CodePage.Padding = new Padding(2);
            CodePage.Size = new Size(846, 457);
            CodePage.TabIndex = 0;
            CodePage.Text = "Code Assembleur";
            CodePage.UseVisualStyleBackColor = true;
            // 
            // ComputerPage
            // 
            ComputerPage.Controls.Add(AsmListView);
            ComputerPage.Location = new Point(4, 28);
            ComputerPage.Margin = new Padding(2);
            ComputerPage.Name = "ComputerPage";
            ComputerPage.Padding = new Padding(2);
            ComputerPage.Size = new Size(846, 457);
            ComputerPage.TabIndex = 1;
            ComputerPage.Text = "Code Machine";
            ComputerPage.UseVisualStyleBackColor = true;
            // 
            // AsmListView
            // 
            AsmListView.Columns.AddRange(new ColumnHeader[] { Column0, Column1, Column2, Column3, Column4 });
            AsmListView.Dock = DockStyle.Fill;
            AsmListView.Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            AsmListView.FullRowSelect = true;
            AsmListView.GridLines = true;
            AsmListView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            AsmListView.Location = new Point(2, 2);
            AsmListView.Margin = new Padding(2);
            AsmListView.MultiSelect = false;
            AsmListView.Name = "AsmListView";
            AsmListView.ShowGroups = false;
            AsmListView.Size = new Size(842, 453);
            AsmListView.TabIndex = 0;
            AsmListView.UseCompatibleStateImageBehavior = false;
            AsmListView.View = View.Details;
            // 
            // FormAsmEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(854, 513);
            Controls.Add(AsmEditorTab);
            Controls.Add(menuStrip1);
            Margin = new Padding(2);
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
            PerformLayout();
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
    }
}