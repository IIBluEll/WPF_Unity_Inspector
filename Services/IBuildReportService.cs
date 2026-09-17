using WPF_UnityInspector.Models;


namespace WPF_UnityInspector.Services
{
    public interface IBuildReportService
    {
        Task<BuildReportData> LoadBuildReport_async(string reportPath, CancellationToken cancellationToken = default);
    }
}
