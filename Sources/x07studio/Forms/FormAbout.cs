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

namespace x07studio.Forms
{
    public partial class FormAbout : Form
    {
        public FormAbout()
        {
            InitializeComponent();

            AboutLabel.Text = $"X07 STUDIO\nDéveloppé par Stéphane Sibué\nhttp://www.coding4phone.com\n\nVersion {Application.ProductVersion}";
        }

        private void AboutLabel_Click(object sender, EventArgs e)
        {
            var psInfo = new ProcessStartInfo
            {
                FileName = "https://www.coding4phone.com",
                UseShellExecute = true
            };

            try
            {
                Process.Start(psInfo);
            }
            catch
            {

            }
        }
    }
}
