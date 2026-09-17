using System.IO;
using System.Text.Json;
using WPF_UnityInspector.Models;

namespace WPF_UnityInspector.Services
{
    // build_report.json 파일을 읽어서 BuildReportData로 변환
    public sealed class BuildReportService : IBuildReportService
    {
        private const int SUPPORTED_SCHEMA_VERSION = 2;

        private static readonly JsonSerializerOptions JSON_OPTIONS = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<BuildReportData> LoadBuildReport_async(string reportPath, CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(reportPath))
            {
                throw new ArgumentException("Build Report 경로가 비어있습니다.", nameof(reportPath));
            }

            if(!File.Exists(reportPath))
            {
                throw new FileNotFoundException("Build Report 파일이 존재하지 않습니다." , reportPath);
            }

            await using FileStream fileStream = new(
                reportPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite,
                4096,
                FileOptions.Asynchronous |
                FileOptions.SequentialScan);

            // Json -> C# 객체 변환
            BuildReportData? reportData = await JsonSerializer.DeserializeAsync<BuildReportData>(fileStream, JSON_OPTIONS, cancellationToken);

            if(reportData == null)
            {
                throw new InvalidDataException("Bulid Report 데이터가 비어있습니다.");
            }

            if(reportData.SchemaVersion != SUPPORTED_SCHEMA_VERSION)
            {
                throw new NotSupportedException(
                    $"지원하지 않는 Build Report Schema입니다. " +
                    $"현재: {reportData.SchemaVersion}, " +
                    $"지원: {SUPPORTED_SCHEMA_VERSION}");
            }

            if ( string.IsNullOrWhiteSpace(reportData.ProjectName) )
            {
                throw new InvalidDataException("Build Report에 프로젝트 이름이 없습니다.");
            }

            return reportData;
        }
    }
}
