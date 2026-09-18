using System.IO;

namespace WPF_UnityInspector.Services
{
    public sealed class ProjectSettingsService
    {
        private static readonly string SETTINGS_DIRECTORY = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HM", "UnityProjectInspector");

        private static readonly string SETTINGS_PATH = Path.Combine(SETTINGS_DIRECTORY, "last-project.txt");

        public string? LoadLastProjectPath()
        {
            if(!File.Exists(SETTINGS_PATH))
            {
                return null;
            }

            string projectPath = File.ReadAllText(SETTINGS_PATH).Trim();
            return string.IsNullOrWhiteSpace(projectPath) ? null : projectPath;
        }

        public void SaveLastProjectPath(string projectPath)
        {
            Directory.CreateDirectory(SETTINGS_DIRECTORY);
            File.WriteAllText(SETTINGS_PATH , Path.GetFullPath(projectPath));
        }
    }
}
