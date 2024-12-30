using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x07studio.Classes
{
    internal class FormSettingsManager
    {
        // Une form possède 2 entrées dans le dictionnaire
        // En mode non MDI, il y a une entrée avec son nom (ex FormMain)
        // En mode MDI, il y a une entée avev son nom précédé d'un marqueur (ex MDI:FormMain)

        public static readonly FormSettingsManager Instance = new FormSettingsManager();

        private FormSettingsDictionary _FormSettingsDictionary = new();

        public FormSettingsManager()
        {
            // On charge les infos depuis les paramètres de l'app
            // Les données sont au format JSON

            Load();
        }

        private void Load()
        {
            // On charge les infos au format JSON depuis les paramètres de l'app

            var s = Properties.Settings.Default.FormSettings;
            var obj = JsonConvert.DeserializeObject<FormSettingsDictionary>(s);

            if (obj is FormSettingsDictionary dico)
            {
                _FormSettingsDictionary = dico;
            }
            else
            {
                _FormSettingsDictionary = new();
            }
        }

        public void Save()
        {
            // On sauvegarde les infos au format JSON dans les paramètres de l'app

            var s = JsonConvert.SerializeObject(_FormSettingsDictionary);
            Properties.Settings.Default.FormSettings = s;
            Properties.Settings.Default.Save();
        }

        public void UpdateFormSettings(Form form)
        {
            var formSettings = new FormSettings(form);
            _FormSettingsDictionary[formSettings.Name] = formSettings;
            Save();
        }

        public void ApplyFormSettings(Form form)
        {
            var name = form.IsMdiChild ? $"MDI:{form.Name}" : form.Name;

            if (_FormSettingsDictionary.ContainsKey(name))
            {
                var s = _FormSettingsDictionary[name];
                form.SuspendLayout();
                form.Location = new Point(s.Left < 0 ? 0 : s.Left, s.Top < 0 ? 0 : s.Top);
                form.Size = new Size(s.Width < 100 ? 100 : s.Width, s.Height < 100 ? 100 : s.Height);
                form.WindowState = s.WindowState;
                form.ResumeLayout(true);
            }
        }

        public void UpdateFormState(Form form)
        {
            // On ne fait l'opération QUE si la form possède déjà des valeurs et que le windows state = max ou normal
            // On ne garde pas de trace du passage en mini
            // Si c'est le cas on met à jour QUE son FormState
            // Dans le cas contraire on laisse tomber la mise à jour

            if (form.WindowState != FormWindowState.Minimized)
            {
                var name = form.IsMdiChild ? $"MDI:{form.Name}" : form.Name;

                if (_FormSettingsDictionary.ContainsKey(name))
                {
                    var s = _FormSettingsDictionary[name];
                    s.WindowState = form.WindowState;
                    Save();
                }
            }
        }
    }
}
