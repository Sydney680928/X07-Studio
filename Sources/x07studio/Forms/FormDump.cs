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

            var bytes = await SerialManager.Default.GetDumpAsync(Properties.Settings.Default.PortName, 4800, 0, 512);
            Debug.WriteLine($"DUMP {bytes.Length}");

            OutputTextBox.Text = OutputDumpBytes(bytes);

            DumpButton.Enabled = true;
        }

        private string OutputDumpBytes(byte[] bytes)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < bytes.Length; i += 16)
            {
                sb.Append(string.Format("{0:X4}  ", i));

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
                        
                        if (v < 32)
                        {
                            c = ".";
                        }
                        else
                        {
                            byte[] b = [v];
                            c = Encoding.ASCII.GetString(b);    
                        }
                    }

                    sb.Append(c);
                }

                sb.AppendLine("  |");
            }

            return sb.ToString();
        }
    }
}
