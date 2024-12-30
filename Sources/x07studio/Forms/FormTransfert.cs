using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using x07studio.Classes;

namespace x07studio.Forms
{
    public partial class FormTransfert : Form
    {
        private string? _Code;
        private bool _RequestStopTransfert;

        public FormTransfert(string? code)
        {
            InitializeComponent();

            _Code = code;
            CancelButton.Visible = false;
            StartButton.Visible = true;
            TransfertProgress.Visible = false;
        }

        private void Transfert(string? code)
        {
            if (!string.IsNullOrEmpty(code))
            {
                SerialManager.Default.SendCommand("CLS:NEW");

                var lines = code.Replace("\r", "").Split("\n");

                int percent;

                for (var i = 0; i < lines.Length; i++)
                {
                    SerialManager.Default.SendCommand(lines[i]);

                    if (_RequestStopTransfert) break;

                    percent = (int)(((double)i / (double)lines.Length) * 100.0);

                    Invoke(() =>
                    {
                        TransfertProgress.Value = percent;
                    });
                }
            }
        }

        private void FormTransfert_Load(object sender, EventArgs e)
        {
            SerialManager.Default.Open(Properties.Settings.Default.PortName, 4800);
        }

        private void FormTransfert_Shown(object sender, EventArgs e)
        {
            if (!SerialManager.Default.IsOpen)
            {
                MessageBox.Show( "Le port série n'est pas ouvert !", "X07 STUDIO", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Close();
            }
            else
            {
                MessageLabel.Text = "Veuillez placer le X07 en mode esclave puis appuyez sur le bouton [Démarrer]...";
            }
        }

        private async void StartButton_Click(object sender, EventArgs e)
        {
            StartButton.Visible = false;
            CancelButton.Visible = true;

            TransfertProgress.Value = 0;
            TransfertProgress.Visible = true;

            MessageLabel.Text = "Transfert en cours...";

            _RequestStopTransfert = false;

            await Task.Run(() =>
            {
                Transfert(_Code);
            });

            if (_RequestStopTransfert)
            {
                MessageLabel.Text = "Transfert abandonné.";

                await Task.Delay(1000);
                TransfertProgress.Visible = false;
            }
            else
            {
                MessageLabel.Text = "Transfert terminé.";
                CancelButton.Enabled = false;

                await Task.Delay(1000);
                TransfertProgress.Visible = false;

                if (MessageBox.Show("Voulez-vous lancer le programme ?", "X07 STUDIO", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    SerialManager.Default.SendCommand("EXEC &HEE33:RUN");
                }
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            CancelButton.Enabled = false;
            _RequestStopTransfert = true;

            MessageLabel.Text = "Annulation en cours...";
        }

        private void FormTransfert_FormClosing(object sender, FormClosingEventArgs e)
        {
            _RequestStopTransfert = true;
        }
    }
}
