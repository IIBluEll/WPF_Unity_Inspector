using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using WPF_UnityInspector.Models;
using WPF_UnityInspector.Services;

namespace WPF_UnityInspector.ViewModels
{
    public sealed class DashboardViewModel : INotifyPropertyChanged
    {
        private static readonly string[] SIZE_UNITS =
        {
            "B",
            "KB",
            "MB",
            "GB",
            "TB"
        };

        private readonly IBuildReportService _buildReportService;
        private readonly IProjectFileService _projectFileService;

        private List<AssetBuildInfo> _assets = new();

        private string _projectPath = "-";
        private string _projectName = "-";
        private string _unityVersion = "-";
        private string _buildResult = "-";
        private string _platform = "-";
        private string _artifactSize = "-";
        private string _buildTime = "-";
        private int _warningCount;
        private int _errorCount;
        private string _statusMessage = string.Empty;
        private string _errorMessage = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public DashboardViewModel(IBuildReportService buildReportService , IProjectFileService projectFileService)
        {
            _buildReportService = buildReportService;
            _projectFileService = projectFileService;
        }

        public string ProjectName
        {
            get => _projectName;
            private set => SetProperty(ref _projectName , value);
        }

        public string UnityVersion
        {
            get => _unityVersion;
            private set => SetProperty(ref _unityVersion , value);
        }

        public string BuildResult
        {
            get => _buildResult;
            private set => SetProperty(ref _buildResult , value);
        }

        public string Platform
        {
            get => _platform;
            set => SetProperty(ref _platform , value);
        }

        public string ArtifactSize
        {
            get => _artifactSize;
            private set => SetProperty(ref _artifactSize , value);
        }

        public string BuildTime
        {
            get => _buildTime;
            private set => SetProperty(ref _buildTime , value);
        }

        public int WarningCount
        {
            get => _warningCount;
            private set => SetProperty(ref _warningCount , value);
        }

        public int ErrorCount
        {
            get => _errorCount;
            private set => SetProperty(ref _errorCount , value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage , value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            private set => SetProperty(ref _errorMessage , value);
        }

        public string ProjectPath
        {
            get => _projectPath;
            private set => SetProperty(ref _projectPath , value);
        }

        public List<AssetBuildInfo> Assets
        {
            get => _assets;
            private set => SetProperty(ref _assets , value);
        }

        public async Task LoadProject_async(string projectPath , CancellationToken cancellationToken = default)
        {
            ResetBuildSummary();

            ProjectPath = projectPath;
            StatusMessage = "Unity 프로젝트를 분석하는 중입니다...";
            ErrorMessage = string.Empty;

            try
            {
                string reportPath = _projectFileService.GetBuildReportPath(projectPath);

                BuildReportData reportData = await _buildReportService.LoadBuildReport_async(reportPath, cancellationToken);

                ProjectPath = Path.GetFullPath(projectPath);

                ProjectName = reportData.ProjectName;
                UnityVersion = reportData.UnityVersion;
                BuildResult = reportData.Build.Result;
                Platform = reportData.Build.Platform;

                ArtifactSize = FormatByteSize(reportData.Build.ArtifactSizeBytes);
                BuildTime = FormatBuildTime(reportData.Build.BuildTimeSeconds);

                WarningCount = reportData.Build.WarningCount;
                ErrorCount = reportData.Build.ErrorCount;

                Assets = reportData.Assets.OrderByDescending(asset => asset.PackedSizeBytes).ToList();

                StatusMessage = "Build Report를 불러왔습니다.";
            }
            catch ( FileNotFoundException )
            {
                ErrorMessage = "Build Report가 없습니다. Unity에서 Build 후 다시 확인하십시오.";
                StatusMessage = string.Empty;
            }
            catch ( JsonException )
            {
                ErrorMessage = "Build Report JSON 형식이 올바르지 않습니다.";
                StatusMessage = string.Empty;
            }
            catch ( NotSupportedException exception )
            {
                ErrorMessage = exception.Message;
                StatusMessage = string.Empty;
            }
            catch ( InvalidDataException exception )
            {
                ErrorMessage = exception.Message;
                StatusMessage = string.Empty;
            }
            catch ( OperationCanceledException )
            {
                StatusMessage = "프로젝트 분석이 취소되었습니다.";
            }
            catch ( Exception exception )
            {
                ErrorMessage = $"프로젝트를 분석할 수 없습니다. {exception.Message}";
                StatusMessage = string.Empty;
            }
        }

        private static string FormatByteSize(long sizeBytes)
        {
            if ( sizeBytes < 0 )
            {
                return "-";
            }

            double displaySize = sizeBytes;
            int unitIndex = 0;

            while ( displaySize >= 1024 && unitIndex < SIZE_UNITS.Length - 1 )
            {
                displaySize /= 1024;
                unitIndex++;
            }

            return $"{displaySize:0.##} {SIZE_UNITS[ unitIndex ]}";
        }

        private static string FormatBuildTime(double buildTimeSeconds)
        {
            TimeSpan buildTime = TimeSpan.FromSeconds(buildTimeSeconds);

            if ( buildTime.TotalHours >= 1 )
            {
                return buildTime.ToString(@"hh\:mm\:ss");
            }

            return buildTime.ToString(@"mm\:ss");
        }

        private bool SetProperty<T>(ref T field , T value , [CallerMemberName] string? propertyName = null)
        {
            if ( EqualityComparer<T>.Default.Equals(field , value) )
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);

            return true;
        }

        private void OnPropertyChanged(string? propertyName)
        {
            PropertyChanged?.Invoke(this , new PropertyChangedEventArgs(propertyName));
        }

        private void ResetBuildSummary()
        {
            ProjectName = "-";
            UnityVersion = "-";
            BuildResult = "-";
            Platform = "-";
            ArtifactSize = "-";
            BuildTime = "-";
            WarningCount = 0;
            ErrorCount = 0; 
            Assets = new List<AssetBuildInfo>();
        }
    }
}
