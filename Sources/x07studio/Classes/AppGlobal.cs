using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x07studio.Classes
{
    internal class AppGlobal
    {
        private static string _RootFolder;
        private static string _ProjectsFolder;
        private static string _SourcesFolder;
        private static string _LibrariesFolder;
        private static string _ProgramsFolder;
        private static string _StorageFolder;
        private static string _AsmFolder;

        private static bool _Initialized = false;

        public static string RootFolder => _RootFolder;

        public static string ProjectsFolder => _ProjectsFolder;

        public static string SourcesFolder => _SourcesFolder;

        public static string LibrarysFolder => _LibrariesFolder;

        public static string ProgramsFolder => _ProgramsFolder;

        public static string StorageFolder => _StorageFolder;   

        public static string AsmFolder => _AsmFolder;
        
        public static bool Initialized => _Initialized;

        static AppGlobal()
        {
            _RootFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "X07 STUDIO");
            _ProjectsFolder = Path.Combine(_RootFolder, "PROJECTS");
            _SourcesFolder = Path.Combine(_ProjectsFolder, "SOURCES");
            _LibrariesFolder = Path.Combine(_ProjectsFolder, "LIBS");
            _ProgramsFolder = Path.Combine(_RootFolder, "PROGRAMS");
            _StorageFolder = Path.Combine(_RootFolder, "STORAGE");
            _AsmFolder = Path.Combine(_RootFolder, "ASM");

            try
            {
                Directory.CreateDirectory(_RootFolder);
                Directory.CreateDirectory(_ProjectsFolder);
                Directory.CreateDirectory(_SourcesFolder);
                Directory.CreateDirectory(_LibrariesFolder);
                Directory.CreateDirectory(_ProgramsFolder);
                Directory.CreateDirectory(StorageFolder);
                Directory.CreateDirectory(AsmFolder);
                
                _Initialized = true;
            }
            catch
            {

            }
        }
    }
}
