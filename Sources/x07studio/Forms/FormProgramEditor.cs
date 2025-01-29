using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using x07studio.Classes;

namespace x07studio.Forms
{
    public partial class FormProgramEditor : Form
    {
        private string? _CurrentProgramFilename;
        private bool _CodeIsModified;

        public FormProgramEditor()
        {
            InitializeComponent();

            InitializeCodeEditor();
            UpdateTitleFromProgram();
        }

        public FormProgramEditor(string code, string name)
        {
            InitializeComponent();
            InitializeCodeEditor();

            CodeEditor.Text = code;
            _CurrentProgramFilename = Path.Combine(AppGlobal.ProgramsFolder, name) + ".BAS";
            _CodeIsModified = true;

            UpdateTitleFromProgram();
        }

        private void InitializeCodeEditor()
        {
            // Chargement de la syntaxe BASIC X-07 de base

            string? dir = Path.GetDirectoryName(Application.ExecutablePath);

            if (Directory.Exists(dir))
            {
                var fsmProvider = new FileSyntaxModeProvider(dir);
                HighlightingManager.Manager.AddSyntaxModeFileProvider(fsmProvider);
                CodeEditor.SetHighlighting("X07");
            }
        }

        private bool ProtectCurrentProgram()
        {
            if (_CodeIsModified)
            {
                if (MessageBox.Show("Le code a changé. Voulez-vous l'enregistrer ?\n\nAttention, si vous répondez NON vous perdrez vos modifications !", "X07 STUDIO", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    if (_CurrentProgramFilename != null)
                    {
                        if (!SaveProgram(_CurrentProgramFilename))
                        {
                            MessageBox.Show("Impossible d'enregistrer le programme !", "X07 STUDIO");
                            return false;
                        }
                        else
                        {
                            _CodeIsModified = false;
                            UpdateTitleFromProgram();
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
                            Filter = "Programmes BASIC|*.BAS",
                            InitialDirectory = AppGlobal.ProgramsFolder,
                        };

                        var r = dialog.ShowDialog();
                        Application.DoEvents();

                        if (r == DialogResult.OK)
                        {
                            var ok = SaveProgram(dialog.FileName);

                            if (!ok)
                            {
                                MessageBox.Show("Impossible d'enregistrer le programme !", "X07 STUDIO");
                                return false;
                            }
                            else
                            {
                                _CurrentProgramFilename = dialog.FileName;
                                _CodeIsModified = false;
                                UpdateTitleFromProgram();
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
            if (ProtectCurrentProgram())
            {
                _CurrentProgramFilename = null;
                CodeEditor.Text = "";
                CodeEditor.Refresh();
                _CodeIsModified = false;
                UpdateTitleFromProgram();
            }
        }

        private void UpdateTitleFromProgram()
        {
            if (_CurrentProgramFilename != null)
            {
                var filename = Path.GetFileName(_CurrentProgramFilename);
                Text = $"PROGRAMME BASIC - {filename}";
            }
            else
            {
                Text = "PROGRAMME BASIC - SANS NOM";
            }
        }

        private void FileOpenMenu_Click(object sender, EventArgs e)
        {
            if (ProtectCurrentProgram())
            {
                using var dialog = new OpenFileDialog()
                {
                    Multiselect = false,
                    AddExtension = true,
                    AddToRecent = true,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    DefaultExt = "BAS",
                    Filter = "Programmes BASIC|*.BAS",
                    InitialDirectory = AppGlobal.ProgramsFolder
                };

                var r = dialog.ShowDialog();
                Application.DoEvents();

                if (r == DialogResult.OK)
                {
                    var ok = OpenProgram(dialog.FileName);

                    if (!ok)
                    {
                        MessageBox.Show("Impossible de charger le programme !", "X07 STUDIO");
                        return;
                    }
                    else
                    {
                        _CurrentProgramFilename = dialog.FileName;
                        _CodeIsModified = false;
                    }

                    UpdateTitleFromProgram();
                }
            }
        }

        private void SaveAs()
        {
            var filename = Path.GetFileName(_CurrentProgramFilename);

            using var dialog = new SaveFileDialog()
            {
                CheckPathExists = true,
                AddExtension = true,
                AddToRecent = true,
                DefaultExt = "BAS",
                Filter = "Programmes BASIC|*.BAS",
                InitialDirectory = AppGlobal.ProgramsFolder,
                FileName = filename
            };

            var r = dialog.ShowDialog();
            Application.DoEvents();

            if (r == DialogResult.OK && dialog.FileName != null)
            {
                var ok = SaveProgram(dialog.FileName);

                if (!ok)
                {
                    MessageBox.Show("Impossible d'enregistrer le programme !", "X07 STUDIO");
                }
                else
                {
                    _CodeIsModified = false;
                    _CurrentProgramFilename = dialog.FileName;
                    UpdateTitleFromProgram();
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

            if (_CurrentProgramFilename != null)
            {
                r = SaveProgram(_CurrentProgramFilename);

                if (!r)
                {
                    MessageBox.Show("Impossible d'enregistrer le programme !", "X07 STUDIO");
                }
                else
                {
                    _CodeIsModified = false;
                    UpdateTitleFromProgram();
                }
            }
            else
            {
                SaveAs();
            }
        }

        private bool SaveProgram(string filename)
        {
            try
            {
                using var writer = new StreamWriter(filename);
                writer.Write(CodeEditor.Text);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool OpenProgram(string filename)
        {
            try
            {
                using var reader = new StreamReader(filename);
                var code = reader.ReadToEnd();
                CodeEditor.Text = code;
                CodeEditor.Refresh();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void CodeEditor_TextChanged(object sender, EventArgs e)
        {
            _CodeIsModified = true;
        }

        private void FormProgramEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ProtectCurrentProgram())
            {
                e.Cancel = true;
            }
        }

        private void TranfertMenu_Click(object sender, EventArgs e)
        {
            var f = new FormTransfert(CodeEditor.Text);
            f.ShowDialog(this);
        }

        private void FormProgramEditor_Load(object sender, EventArgs e)
        {
            LoadStatements();
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

        private void FormProgramEditor_Shown(object sender, EventArgs e)
        {
            Application.DoEvents();
        }

        private void PrintMenu_Click(object sender, EventArgs e)
        {
            FormMain.Default.PrintDocument(CodeEditor);
        }
    }
}
