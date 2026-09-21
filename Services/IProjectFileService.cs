using WPF_UnityInspector.Models;

namespace WPF_UnityInspector.Services
{
    public interface IProjectFileService
    {
        bool IsUnityProject(string projectPath);
        bool HasBuildReport(string projectPath);

        UNITY_PACKAGE_STATE GetInspectorPackageState(string projectPath);

        string GetBuildReportPath(string projectPath);
        string GetProjectName(string projectPath);
        string GetUnityVersion(string projectPath);
    }
}
