namespace WPF_UnityInspector.Services
{
    public interface IProjectFileService
    {
        bool IsUnityProject(string projectPath);

        string GetBuildReportPath(string projectPath);
        string GetProjectName(string projectPath);
        string GetUnityVersion(string projectPath);
    }
}
