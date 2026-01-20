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
    public partial class FormLoadFile : Form
    {
        public FormLoadFile()
        {
            InitializeComponent();
        }

        private void SelectFileButton_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog()
            {
                Multiselect = false,
                AddExtension = true,
                AddToRecent = true,
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "X07",
                Filter = "Programmes X07 (*.K7)|*.K7|Programmes CAS (*.CAS)|*.CAS|All Files (*.*)|*.*",
                InitialDirectory = AppGlobal.StorageFolder
            };

            var r = dialog.ShowDialog();
            Application.DoEvents();

            if (r == DialogResult.OK)
            {
                try
                {
                    using var stream = new FileStream(dialog.FileName, FileMode.Open);
                    using var reader = new BinaryReader(stream);
                    var bytes = reader.ReadBytes((int)stream.Length);

                    if (MessageBox.Show("Veuillez lancer la commande\n\nLOAD \"COM:\"\n\nsur votre CANON X-07 puis appuyer sur le bouton OK", "STUDIO X07", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        if (!SerialManager.Default.Open(Properties.Settings.Default.PortName, 4800))
                        {
                            MessageBox.Show( "Le port série n'est pas ouvert !", "X07 STUDIO", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            DialogResult = DialogResult.Cancel;
                        }
                        else
                        {
                            var r1 = SerialManager.Default.SendProgram(bytes);

                            if (r1 == SerialManager.ResponseStatus.Success)
                            {
                                MessageBox.Show("Programme transféré avec succes.", "STUDIO X07", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("Impossible de transférer le programme !", "STUDIO X07", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Impossible de transférer le programme !", "STUDIO X07", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
