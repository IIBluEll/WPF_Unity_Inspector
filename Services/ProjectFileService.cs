using System.IO;
using System.Text.Json;
using WPF_UnityInspector.Models;

namespace WPF_UnityInspector.Services
{
    public sealed class ProjectFileService : IProjectFileService
    {
        private const string REPORT_DIRECTORY_NAME = ".unityprojectinspector";
        private const string REPORT_FILE_NAME = "build_report.json";
        private const string PROJECT_VERSION_FILE_NAME = "ProjectVersion.txt";
        private const string EDITOR_VERSION_PREFIX = "m_EditorVersion:";

        private const string INSPECTOR_PACKAGE_NAME = "com.hm.unity-project-inspector";
        private const string MANIFEST_FILE_NAME = "manifest.json";
        private const string PACKAGE_FILE_NAME = "package.json";

        private static readonly string[] REQUIRED_DIRECOTRY_NAMES =
        {
            "Assets",
            "Packages",
            "ProjectSettings"
        };

        public string GetProjectName(string projectPath)
        {
            return new DirectoryInfo(Path.GetFullPath(projectPath)).Name;
        }

        public string GetUnityVersion(string projectPath)
        {
            string versionPath = Path.Combine(projectPath, "ProjectSettings", PROJECT_VERSION_FILE_NAME);

            try
            {
                foreach(string line in File.ReadLines(versionPath))
                {
                    if(!line.StartsWith(EDITOR_VERSION_PREFIX, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    string version = line[EDITOR_VERSION_PREFIX.Length..].Trim();
                    return version.Length > 0 ? version : "-";
                }
            }
            catch(Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                return "-";
            }

            return "-";
        }

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

        public bool HasBuildReport(string projectPath)
        {
            return File.Exists(GetBuildReportPath(projectPath));
        }

        public UNITY_PACKAGE_STATE GetInspectorPackageState(string projectPath)
        {
            if(!IsUnityProject(projectPath))
            {
                return UNITY_PACKAGE_STATE.UNKNOWN;
            }

            string packagesPath = Path.Combine(Path.GetFullPath(projectPath),"Packages");

            string embeddedPackagePath = Path.Combine(packagesPath, INSPECTOR_PACKAGE_NAME, PACKAGE_FILE_NAME);

            if(File.Exists(embeddedPackagePath))
            {
                return UNITY_PACKAGE_STATE.INSTALLED;
            }

            string manifestPath = Path.Combine(packagesPath, MANIFEST_FILE_NAME);

            if(!File.Exists(manifestPath))
            {
                return UNITY_PACKAGE_STATE.UNKNOWN;
            }

            try
            {
                using FileStream stream = File.OpenRead(manifestPath);

                using JsonDocument manifest = JsonDocument.Parse(
            stream,
            new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            });

                bool hasDependencies = manifest.RootElement.TryGetProperty("dependencies", out JsonElement dependencies);

                if ( !hasDependencies || dependencies.ValueKind != JsonValueKind.Object ) return UNITY_PACKAGE_STATE.UNKNOWN;

                return dependencies.TryGetProperty(INSPECTOR_PACKAGE_NAME , out _) ? UNITY_PACKAGE_STATE.INSTALLED :  UNITY_PACKAGE_STATE.NOT_INSTALLED;
            }
            catch ( Exception exception ) when (
                exception is IOException or
                UnauthorizedAccessException or
                JsonException )
            {
                return UNITY_PACKAGE_STATE.UNKNOWN;
            }
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
