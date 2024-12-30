using Newtonsoft.Json;

namespace x07studio.Classes
{
    internal class Project
    {
        private string _Code = string.Empty;

        public static Project Default { get; private set; } = new Project();

        [JsonProperty("code")]
        public string? Code
        {
            get => _Code;

            set
            {
                if (_Code != value)
                {
                    _Code = value;
                    CodeIsModified = true;  
                }
                else
                {
                    CodeIsModified = false;
                }
            }
        }

        [JsonIgnore]
        public bool CodeIsModified { get; private set; }

        [JsonIgnore]
        public bool FilenameIsDefined => !string.IsNullOrEmpty(Filename);

        [JsonIgnore]
        public string? Filename { get; set; }   

        public void New()
        {
            Code = string.Empty;
            Filename = null;
            CodeIsModified = false;
        }

        public bool Open(string filename)
        {
            var p = Load(filename);

            if (p != null)
            {
                UpdateFormOtherProject(p);
                return true;
            }

            return false;           
        }

        public static Project? Load(string filename)
        {
            try
            {
                using StreamReader streamReader = new StreamReader(filename);
                var json = streamReader.ReadToEnd();
                var project = JsonConvert.DeserializeObject<Project>(json);

                if (project != null)
                {
                    project.Filename = filename;
                    project.CodeIsModified = false;
                    return project;
                }
            }
            catch
            {

            }

            return null;
        }

        public bool Save(string filename)
        {
            Filename = filename;
            return Save();
        }

        public bool Save()
        {
            try
            {
                if (string.IsNullOrEmpty(Filename))
                {
                    return false;
                }
                else
                {
                    var json = JsonConvert.SerializeObject(this);
                    using var sw = new StreamWriter(Filename);
                    sw.Write(json);
                    CodeIsModified = false;
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static void UpdateFormOtherProject(Project project)
        {
            Default = project;
        }
    }
}
