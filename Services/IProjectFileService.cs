namespace WPF_UnityInspector.Services
{
    public interface IProjectFileService
    {
        bool IsUnityProject(string projectPath);

        string GetBuildReportPath(string projectPath);
    }
}
