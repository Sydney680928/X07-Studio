using ICSharpCode.TextEditor;
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
using System.Xml.Linq;
using x07studio.Classes;

namespace x07studio.Forms
{
    public partial class FormAsmEditor : Form
    {
        private string? _CurrentProgramFilename;
        private bool _CodeIsModified;
        private ASM80 _ASM80 = new();
        private ASM80.AssembleResult? _CurrentAssembleResult;

        public FormAsmEditor()
        {
            InitializeComponent();
            InitializeCodeEditor();
            UpdateTitleFromProgram();
        }

        private void InitializeCodeEditor()
        {
            // Chargement de la syntaxe Z80 de base

            string? dir = Path.GetDirectoryName(Application.ExecutablePath);

            if (Directory.Exists(dir))
            {
                var fsmProvider = new FileSyntaxModeProvider(dir);
                HighlightingManager.Manager.AddSyntaxModeFileProvider(fsmProvider);
                CodeEditor.SetHighlighting("Z80");
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
                            DefaultExt = "Z80",
                            Filter = "Programmes Z80|*.Z80",
                            InitialDirectory = AppGlobal.AsmFolder
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

        private void FileNewMenu_Click(object sender, EventArgs e)
        {
            if (ProtectCurrentProgram())
            {
                _CurrentProgramFilename = null;
                CodeEditor.Text = "";
                CodeEditor.Refresh();
                _CodeIsModified = false;
                UpdateTitleFromProgram();
                _CurrentAssembleResult = null;
                AsmListView.Items.Clear();
                AsmEditorTab.SelectedTab = CodePage;
            }
        }

        private void UpdateTitleFromProgram()
        {
            if (_CurrentProgramFilename != null)
            {
                var filename = Path.GetFileName(_CurrentProgramFilename);
                Text = $"PROGRAMME Z80 - {filename}";
            }
            else
            {
                Text = "PROGRAMME Z80 - SANS NOM";
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
                    DefaultExt = "Z80",
                    Filter = "Programmes Z80|*.Z80",
                    InitialDirectory = AppGlobal.AsmFolder
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
                DefaultExt = "Z80",
                Filter = "Programmes Z80|*.Z80",
                InitialDirectory = AppGlobal.AsmFolder,
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
                _CurrentAssembleResult = null;
                AsmListView.Items.Clear();
                AsmEditorTab.SelectedTab = CodePage;
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

        private void FormAsmEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ProtectCurrentProgram())
            {
                e.Cancel = true;
            }
        }

        private void CompileMenu_Click(object sender, EventArgs e)
        {
            AsmListView.Items.Clear();

            _CurrentAssembleResult = _ASM80.Assemble(CodeEditor.Text);

            if (_CurrentAssembleResult.Status == ASM80.AssembleResultStatusEnum.Success)
            {
                for (int i = 0; i < _CurrentAssembleResult.Outlines.Count; i++)
                {
                    if (_CurrentAssembleResult.Outlines[i].Hexa.Length > 0)
                    {
                        var item = new AsmListViewItem(_CurrentAssembleResult.Outlines[i]);
                        AsmListView.Items.Add(item);
                    }
                }

                AsmListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                AsmEditorTab.SelectedTab = ComputerPage;
            }
            else
            {
                var sb = new StringBuilder();
                sb.AppendLine("Une erreur est survenue !");
                sb.AppendLine();
                sb.AppendLine(_CurrentAssembleResult.ErrorMessage);
                sb.AppendLine($"Ligne {_CurrentAssembleResult.ErrorLine + 1}");
                sb.AppendLine(_CurrentAssembleResult.ErrorCode);

                MessageBox.Show(this, sb.ToString(), "X07 STUDIO", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                SelectLine(_CurrentAssembleResult.ErrorLine + 1);
            }
        }

        private void CreateLoaderMenu_Click(object sender, EventArgs e)
        {
            if (_CurrentAssembleResult != null)
            {
                var code = _ASM80.CreateBasicLoader(_CurrentAssembleResult.Outlines);

                var name = "";

                if (_CurrentProgramFilename != null)
                {
                    name = "ASM_Loader_" + Path.GetFileNameWithoutExtension(_CurrentProgramFilename);
                }

                var f = new FormProgramEditor(code, name);
                f.MdiParent = FormMain.Default;
                f.Show();
            }
        }

        private void PrintMenu_Click(object sender, EventArgs e)
        {
            FormMain.Default.PrintDocument(CodeEditor);
        }
    }
}
