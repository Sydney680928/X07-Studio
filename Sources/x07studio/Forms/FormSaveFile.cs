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
    public partial class FormSaveFile : Form
    {
        public FormSaveFile()
        {
            InitializeComponent();
        }

        private async void SaveFileForm_Shown(object sender, EventArgs e)
        {
            if (!SerialManager.Default.IsOpen)
            {
                MessageBox.Show( "Le port série n'est pas ouvert !", "X07 STUDIO", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                DialogResult = DialogResult.Cancel;
            }
            else
            {
                var r1 = await SerialManager.Default.GetProgramAsync(60000);

                if (r1.Status == SerialManager.ResponseStatus.Success && r1.Value != null)
                {
                    using var dialog = new SaveFileDialog()
                    {
                        CheckPathExists = true,
                        AddExtension = true,
                        AddToRecent = true,
                        DefaultExt = "PX7",
                        Filter = "Programmes X07|*.K7",
                        InitialDirectory = AppGlobal.StorageFolder
                    };

                    var r2 = dialog.ShowDialog();
                    Application.DoEvents();

                    if (r2 == DialogResult.OK)
                    {
                        try
                        {
                            using var stream = new FileStream(dialog.FileName, FileMode.OpenOrCreate);
                            using var writer = new BinaryWriter(stream);
                            writer.Write(r1.Value);
                            writer.Flush();

                            MessageBox.Show("Programme enregistré avec succès.", "STUDIO X07", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Impossible d'enregistrer le programme !", "STUDIO X07", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }

                DialogResult = DialogResult.OK;
            }
        }

        private void SaveFileForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SerialManager.Default.RequestCancelGetProgram();
        }

        private void FormSaveFile_Load(object sender, EventArgs e)
        {
            SerialManager.Default.Open(Properties.Settings.Default.PortName, 4800);
        }
    }
}
