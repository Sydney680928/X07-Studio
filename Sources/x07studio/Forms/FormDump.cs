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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace x07studio.Forms
{
    public partial class FormDump : Form
    {
        public FormDump()
        {
            InitializeComponent();
        }

        private async void DumpButton_Click(object sender, EventArgs e)
        {
            DumpButton.Enabled = false;

            var addr = ParseValue(AddressTextBox.Text);

            if (addr != null)
            {
                var size = ParseValue(DumpSizeTextBox.Text);

                if (size != null)
                {
                    var bytes = await SerialManager.Default.GetDumpAsync(Properties.Settings.Default.PortName, 4800, addr.Value, size.Value, TransferProgressBar);

                    if (bytes.Length > 0)
                    {
                        OutputTextBox.Text = OutputDumpBytes(bytes, addr.Value);
                    }
                    else
                    {
                        MessageBox.Show("Une erreur s'est produite pendant le transfert des données !");
                    }
                }
                else
                {
                    MessageBox.Show("Veuillez saisir une longueur valide !");
                    DumpSizeTextBox.Focus();
                }
            }
            else
            {
                MessageBox.Show("Veuillez saisir une adresse valide !");
                AddressTextBox.Focus();
            }

            DumpButton.Enabled = true;
        }

        private string OutputDumpBytes(byte[] bytes, UInt16 baseAddress)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < bytes.Length; i += 16)
            {
                sb.Append(string.Format("{0:X4}  ", i + baseAddress));

                for (int j = i; j < i + 16; j++)
                {
                    var v = j < bytes.Length ? string.Format("{0:X2}", bytes[j]) : "  ";
                    sb.Append(string.Format("{0:X2} ", v));
                }

                sb.Append(" | ");

                for (int j = i; j < i + 16; j++)
                {
                    var c = " ";

                    if (j < bytes.Length)
                    {
                        var v = bytes[j];
                        
                        if (v < 32 || v > 126)
                        {
                            c = ".";
                        }
                        else
                        {
                            byte[] b = [v];
                            c = Encoding.GetEncoding(28591).GetString(b);
                        }
                    }

                    sb.Append(c);
                }

                sb.AppendLine("  |");
            }

            return sb.ToString();
        }

        private UInt16? ParseValue(string value)
        {
            // $300 --> 0x300
            // 300 --> 300

            if (value.StartsWith("$"))
            {
                if (UInt16.TryParse(value.Substring(1), System.Globalization.NumberStyles.HexNumber, null, out var result))
                {
                    return result;
                }
            }
            else
            {
                if (UInt16.TryParse(value, null, out var result))
                {
                    return result;
                }
            }

            return null;
        }
    }
}
