using System.IO;

namespace WPF_UnityInspector.Services
{
    public sealed class ProjectFileService : IProjectFileService
    {
        private const string REPORT_DIRECTORY_NAME = ".unityprojectinspector";
        private const string REPORT_FILE_NAME = "build_report.json";

        private static readonly string[] REQUIRED_DIRECOTRY_NAMES =
        {
            "Assets",
            "Packages",
            "ProjectSettings"
        };

        public bool IsUnityProject(string projectPath)
        {
            if ( string.IsNullOrWhiteSpace(projectPath) )
            {
                return false;
            }

            if ( !Directory.Exists(projectPath) )
            {
                return false;
            }

            foreach ( string directoryName in REQUIRED_DIRECOTRY_NAMES )
            {
                string requiredDirecotryPath = Path.Combine(projectPath, directoryName);

                if ( !Directory.Exists(requiredDirecotryPath) )
                {
                    return false;
                }
            }

            return true;
        }

        public string GetBuildReportPath(string projectPath)
        {
            if(!IsUnityProject(projectPath))
            {
                throw new InvalidDataException("선택한 폴더는 Unity 프로젝트가 아닙니다.");
            }

            string fullProjectPath = Path.GetFullPath(projectPath);

            return Path.Combine(fullProjectPath,REPORT_DIRECTORY_NAME, REPORT_FILE_NAME);
        }
    }
}
