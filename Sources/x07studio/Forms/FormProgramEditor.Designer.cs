namespace x07studio.Forms
{
    partial class FormProgramEditor
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
            fichierToolStripMenuItem = new ToolStripMenuItem();
            FileNewMenu = new ToolStripMenuItem();
            FileOpenMenu = new ToolStripMenuItem();
            FileSaveMenu = new ToolStripMenuItem();
            FileSaveAsMenu = new ToolStripMenuItem();
            TranfertMenu = new ToolStripMenuItem();
            StatementsComboBox = new ComboBox();
            CodeEditor = new ICSharpCode.TextEditor.TextEditorControl();
            menuStrip1 = new MenuStrip();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.AllowMerge = false;
            menuStrip1.AutoSize = false;
            menuStrip1.BackColor = Color.FromArgb(204, 213, 240);
            menuStrip1.ImageScalingSize = new Size(24, 24);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fichierToolStripMenuItem, TranfertMenu });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(9, 5, 0, 5);
            menuStrip1.Size = new Size(941, 50);
            menuStrip1.TabIndex = 5;
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
            // TranfertMenu
            // 
            TranfertMenu.Name = "TranfertMenu";
            TranfertMenu.Size = new Size(104, 40);
            TranfertMenu.Text = "Transférer";
            TranfertMenu.Click += TranfertMenu_Click;
            // 
            // StatementsComboBox
            // 
            StatementsComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            StatementsComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            StatementsComboBox.FlatStyle = FlatStyle.System;
            StatementsComboBox.FormattingEnabled = true;
            StatementsComboBox.Location = new Point(20, 75);
            StatementsComboBox.Name = "StatementsComboBox";
            StatementsComboBox.Size = new Size(905, 33);
            StatementsComboBox.Sorted = true;
            StatementsComboBox.TabIndex = 4;
            // 
            // CodeEditor
            // 
            CodeEditor.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            CodeEditor.BorderStyle = BorderStyle.FixedSingle;
            CodeEditor.Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            CodeEditor.IsReadOnly = false;
            CodeEditor.Location = new Point(19, 131);
            CodeEditor.Name = "CodeEditor";
            CodeEditor.Size = new Size(906, 628);
            CodeEditor.TabIndex = 3;
            CodeEditor.TextChanged += CodeEditor_TextChanged;
            // 
            // FormProgramEditor
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(941, 778);
            Controls.Add(StatementsComboBox);
            Controls.Add(CodeEditor);
            Controls.Add(menuStrip1);
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormProgramEditor";
            Text = "EDITEUR DE PROGRAMME";
            FormClosing += FormProgramEditor_FormClosing;
            Load += FormProgramEditor_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private ComboBox StatementsComboBox;
        private ICSharpCode.TextEditor.TextEditorControl CodeEditor;
        private ToolStripMenuItem fichierToolStripMenuItem;
        private ToolStripMenuItem FileNewMenu;
        private ToolStripMenuItem FileOpenMenu;
        private ToolStripMenuItem FileSaveMenu;
        private ToolStripMenuItem FileSaveAsMenu;
        private ToolStripMenuItem TranfertMenu;
    }
}