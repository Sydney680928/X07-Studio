using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using x07studio.Classes;

namespace x07studio.Forms
{
    public partial class FormProject : Form
    {
        public FormProject()
        {
            InitializeComponent();

            // Chargement de la syntaxe BASIC X-07 de base

            string? dir = Path.GetDirectoryName(Application.ExecutablePath);

            if (Directory.Exists(dir))
            {
                var fsmProvider = new FileSyntaxModeProvider(dir);
                HighlightingManager.Manager.AddSyntaxModeFileProvider(fsmProvider);
                CodeEditor.SetHighlighting("X07");
            }

            // Initialisation

            // CodeEditor.Font = Properties.Settings.Default.EditorFont;
        }

        private void FormCodeEditor_Load(object sender, EventArgs e)
        {
            FormSettingsManager.Instance.ApplyFormSettings(this);

            LoadStatements();
            RefreshFromProject();
            UpdateTitleFromProject();
        }

        private void LoadStatements()
        {
            string[] items =
                [
                "BEEP",
                "CIRCLE", "CLEAR", "CLOAD", "CLOAD?", "CLS", "CONSOLE", "CONT", "CSAVE",
                "DATA", "DEFFN", "DEFINT", "DEFSNG", "DEFDBL", "DEFSTR", "DELETE", "DIM", "DIR",
                "END", "ERASE", "ERROR", "EXEC", "FOR", "FSET",
                "GOSUB", "GOTO",
                "IF", "INIT", "INPUT",
                "LET", "LINE", "LIST", "LLIST", "LOAD", "LOAD?", "LOCATE", "LPRINT",
                "MOTOR",
                "NEW", "NEXT",
                "OFF", "ON ERROR GOTO", "ON ~ GOSUB", "ON ~ GOTO", "OUT",
                "POKE", "PRESET", "PRINT", "PRINT USING", "PSET",
                "READ", "REM", "RESTORE", "RESUME", "RETURN", "RUN",
                "SAVE", "SLEEP", "STOP",
                "TROFF", "TRON",
                "ABS", "ALM$", "ASC", "ATN",
                "CDBL", "CHR$", "CINT", "COS", "CSNG", "CSRLIN",
                "DATE$",
                "ERL", "ERR", "EXP",
                "FIX", "FONT$", "FRE",
                "HEX$",
                "INKEY$", "INP", "INSTR", "INT",
                "KEY$", "LEFT$", "LEN", "LOG",
                "MID$",
                "PEEK", "POINT", "POS", "RIGHT$", "RND",
                "SCREEN", "SNG", "SIN", "SNS", "SQR", "START$", "STICK", "STR$", "STRIG", "STRING$",
                "TAB", "TAN", "TIME$", "TKEY",
                "USR",
                "VAL", "VARPTR"
                ];

            StatementsComboBox.Items.AddRange(items);
        }

        private void CodeEditor_TextChanged(object sender, EventArgs e)
        {
            Project.Default.Code = CodeEditor.Text;
        }

        public void RefreshFromProject()
        {
            CodeEditor.Text = Project.Default.Code;
            CodeEditor.Refresh();
        }

        public void SelectLine(int lineNumber)
        {
            try
            {
                CodeEditor.ActiveTextAreaControl.SelectionManager.ClearSelection();

                var startLocation = new TextLocation(0, lineNumber - 1);
                var endLocation = new TextLocation(80, lineNumber - 1);

                CodeEditor.ActiveTextAreaControl.SelectionManager.SetSelection(startLocation, endLocation);
                CodeEditor.ActiveTextAreaControl.ScrollTo(endLocation.Line, endLocation.Column);
            }
            catch
            {

            }
        }

        private void FormCodeEditor_ResizeEnd(object sender, EventArgs e)
        {
            FormSettingsManager.Instance.UpdateFormSettings(this);
        }

        private void StatementsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (StatementsComboBox.SelectedItem is string item)
            {
                CodeEditor.ActiveTextAreaControl.TextArea.InsertString(item);
                CodeEditor.Focus();
            }
        }

        private bool ProtectCurrentProject()
        {
            if (Project.Default.CodeIsModified)
            {
                if (MessageBox.Show("Le code a changé. Voulez-vous l'enregistrer ?\n\nAttention, si vous répondez NON vous perdrez vos modifications !", "X07 STUDIO", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    if (Project.Default.FilenameIsDefined)
                    {
                        if (!Project.Default.Save())
                        {
                            MessageBox.Show("Impossible d'enregistrer le projet !", "X07 STUDIO");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        using var dialog = new SaveFileDialog()
                        {
                            CheckPathExists = true,
                            AddExtension = true,
                            AddToRecent = true,
                            DefaultExt = "X07",
                            Filter = "Projets X07|*.X07"
                        };

                        var r = dialog.ShowDialog();
                        Application.DoEvents();

                        if (r == DialogResult.OK)
                        {
                            var ok = Project.Default.Save(dialog.FileName);

                            if (!ok)
                            {
                                MessageBox.Show("Impossible d'enregistrer le projet !", "X07 STUDIO");
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private void FileNewMenu_Click(object sender, EventArgs e)
        {
            if (ProtectCurrentProject())
            {
                Project.Default.New();
                RefreshFromProject();
                UpdateTitleFromProject();
            }
        }

        private void UpdateTitleFromProject()
        {
            if (Project.Default.FilenameIsDefined)
            {
                var filename = Path.GetFileName(Project.Default.Filename);
                Text = $"PROJET - {filename}";
            }
            else
            {
                Text = "PROJET - SANS NOM";
            }
        }

        private void FileOpenMenu_Click(object sender, EventArgs e)
        {
            if (ProtectCurrentProject())
            {
                using var dialog = new OpenFileDialog()
                {
                    Multiselect = false,
                    AddExtension = true,
                    AddToRecent = true,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    DefaultExt = "X07",
                    Filter = "Projets X07|*.X07",
                    InitialDirectory = AppGlobal.ProjectsFolder
                };

                var r = dialog.ShowDialog();
                Application.DoEvents();

                if (r == DialogResult.OK)
                {
                    var ok = Project.Default.Open(dialog.FileName);

                    if (!ok)
                    {
                        MessageBox.Show("Impossible de charger le projet !", "X07 STUDIO");
                        return;
                    }

                    RefreshFromProject();
                    UpdateTitleFromProject();
                }
            }
        }

        private void SaveAs()
        {
            using var dialog = new SaveFileDialog()
            {
                CheckPathExists = true,
                AddExtension = true,
                AddToRecent = true,
                DefaultExt = "X07",
                Filter = "Projets X07|*.X07",
                InitialDirectory = AppGlobal.ProjectsFolder
            };

            var r = dialog.ShowDialog();
            Application.DoEvents();

            if (r == DialogResult.OK)
            {
                var ok = Project.Default.Save(dialog.FileName);

                if (!ok)
                {
                    MessageBox.Show("Impossible d'enregistrer le projet !", "X07 STUDIO");
                }
                else
                {
                    UpdateTitleFromProject();
                }
            }
        }

        private void FileSaveAsMenu_Click(object sender, EventArgs e)
        {
            SaveAs();
        }

        private void FileSaveMenu_Click(object sender, EventArgs e)
        {
            bool r;

            if (Project.Default.FilenameIsDefined)
            {
                r = Project.Default.Save();

                if (!r)
                {
                    MessageBox.Show("Impossible d'enregistrer le projet !", "X07 STUDIO");
                }
            }
            else
            {
                SaveAs();
            }
        }

        private void FormProject_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ProtectCurrentProject())
            {
                e.Cancel = true;
            }
        }

        private void GenerateMenu_Click(object sender, EventArgs e)
        {
            if (Project.Default.Code != null)
            {
                var r = CodeGenerator.Default.Generate(Project.Default.Code);

                if (r.Status != CodeGenerator.ResultStatusEnum.Success)
                {
                    SelectLine(r.ErrorLineNumber);
                    MessageBox.Show(r.ErrorMessage, "X07 STUDIO");
                }
                else
                {
                    var name = "";

                    if (Project.Default.Filename != null)
                    {
                        name = Path.GetFileNameWithoutExtension(Project.Default.Filename);
                    }

                    var f = new FormProgramEditor(r.Code ?? "", name);
                    f.MdiParent = FormMain.Default;
                    f.Show();
                }
            }
        }

        private void FormProject_Shown(object sender, EventArgs e)
        {
            Application.DoEvents();
        }

        private void PrintMenu_Click(object sender, EventArgs e)
        {
            FormMain.Default.PrintDocument(CodeEditor);
        }
    }
}
