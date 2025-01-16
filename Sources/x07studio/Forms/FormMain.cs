using System.Diagnostics;
using System.Text;
using System.Xml.Serialization;
using x07studio.Classes;

namespace x07studio.Forms
{
    public partial class FormMain : Form
    {
        private static FormMain _Default = new FormMain();

        private FormProject? _FormProject;

        public static FormMain Default => _Default;

        public FormMain()
        {
            InitializeComponent();

            foreach (var control in Controls)
            { 
                if (control is MdiClient c) 
                {
                    c.BackColor = Color.FromArgb(255, 93, 107, 153);
                    break; 
                } 
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            FormSettingsManager.Instance.ApplyFormSettings(this);

            if (!AppGlobal.Initialized)
            {
                MessageBox.Show("Un problème est survenu pendant le démarrage de X07 STUDIO !", "X07 STUDIO");
                Application.Exit();
            }
        }

        private void OpenFormProject()
        {
            if (_FormProject == null)
            {
                _FormProject = new FormProject();
                _FormProject.MdiParent = this;
                _FormProject.FormClosed += FormProject_FormClosed;
            }

            _FormProject.Show();
            _FormProject.WindowState = FormWindowState.Normal;
        }

        private void FormProject_FormClosed(object? sender, FormClosedEventArgs e)
        {
            _FormProject = null;
        }

        private void DisplayProjectMenu_Click(object sender, EventArgs e)
        {
            OpenFormProject();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void FormMain_ResizeEnd(object sender, EventArgs e)
        {
            FormSettingsManager.Instance.UpdateFormSettings(this);
        }

        private void FontEditorMenu_Click(object sender, EventArgs e)
        {
            var f = new FormMakeFont();
            f.MdiParent = this;
            f.Show();
        }

        private void ProgramEditorMenu_Click(object sender, EventArgs e)
        {
            var f = new FormProgramEditor();
            f.MdiParent = this;
            f.Show();
        }

        private void LoadProgramMenu_Click(object sender, EventArgs e)
        {
            var f = new FormLoadFile();
            f.ShowDialog(this);
        }

        private void SaveProgramMenu_Click(object sender, EventArgs e)
        {
            var f = new FormSaveFile();
            f.ShowDialog(this);
        }

        private void Z80EditorMenu_Click(object sender, EventArgs e)
        {
            var f = new FormAsmEditor();
            f.MdiParent = this;
            f.Show();
        }

        private void SettingsMenu_Click(object sender, EventArgs e)
        {
            SerialManager.Default.Close();

            var f = new FormSettings();
            f.ShowDialog(this);
            Application.DoEvents();

            SerialManager.Default.Open(Properties.Settings.Default.PortName, 4800);
        }

        private async void AboutMenu_Click(object sender, EventArgs e)
        {
#if DEBUG
            var r = new Random();
            var l = (UInt16)r.Next(100, 5000);
            var bytes = await SerialManager.Default.GetDumpAsync(Properties.Settings.Default.PortName, 4800,0x1000, l);
            Debug.WriteLine($"DUMP {bytes.Length}");
            return;
#endif

            var f = new FormAbout();
            f.ShowDialog(this);
            Application.DoEvents();
        }
    }
}
