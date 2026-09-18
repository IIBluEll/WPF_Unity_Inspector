using System.Windows.Data;
using System.ComponentModel;
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

        private static readonly string[] MESSAGE_TYPES =
        {
            ALL_MESSAGE_TYPES,
            "Warning",
            "Error"
        };

        private List<string> _assetTypes = new() {ALL_ASSET_TYPES};
        private List<AssetBuildInfo> _assets = new();
        private List<BuildMessage> _messages = new();
        private List<AssetTypeSummary> _assetTypeSummaries = new();

        private readonly IBuildReportService _buildReportService;
        private readonly IProjectFileService _projectFileService;
        private readonly ProjectSizeService _projectSizeService;
        private CancellationTokenSource? _loadCancellationTokenSource;

        private ICollectionView _assetsView;
        private ICollectionView _messagesView;

        private const string ALL_ASSET_TYPES = "전체";
        private const string ALL_MESSAGE_TYPES = "전체";

        private string _assetSearchText = string.Empty;
        private string _selectedAssetType = ALL_ASSET_TYPES;
        private string _messageSearchText = string.Empty;
        private string _selectedMessageType = ALL_MESSAGE_TYPES;

        private string _reportGeneratedAt = "-";
        private string _projectDiskSize = "-";
        private string _projectSourceSize = "-";
        private string _assetsSize = "-";
        private string _storageStatus = string.Empty;

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

        public DashboardViewModel(
            IBuildReportService buildReportService,
            IProjectFileService projectFileService,
            ProjectSizeService projectSizeService)
        {
            _buildReportService = buildReportService;
            _projectFileService = projectFileService;
            _projectSizeService = projectSizeService;

            _assetsView = CollectionViewSource.GetDefaultView(_assets);
            _assetsView.Filter = FilterAsset;

            _messagesView = CollectionViewSource.GetDefaultView(_messages);
            _messagesView.Filter = FilterMessage;
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

        public string ReportGeneratedAt
        {
            get => _reportGeneratedAt;
            private set => SetProperty(ref _reportGeneratedAt , value);
        }

        public string ProjectDiskSize
        {
            get => _projectDiskSize;
            private set => SetProperty(ref _projectDiskSize, value);
        }

        public string ProjectSourceSize
        {
            get => _projectSourceSize;
            private set => SetProperty(ref _projectSourceSize, value);
        }

        public string AssetsSize
        {
            get => _assetsSize;
            private set => SetProperty(ref _assetsSize, value);
        }

        public string StorageStatus
        {
            get => _storageStatus;
            private set => SetProperty(ref _storageStatus, value);
        }

        public List<AssetBuildInfo> Assets
        {
            get => _assets;
            private set
            {
                if ( !SetProperty(ref _assets , value) )
                {
                    return;
                }

                AssetSearchText = string.Empty;
                SelectedAssetType = ALL_ASSET_TYPES;

                AssetTypes = new List<string> { ALL_ASSET_TYPES }
                    .Concat(_assets.Select(asset => asset.Type)
                        .Where(type => !string.IsNullOrWhiteSpace(type))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(type => type , StringComparer.OrdinalIgnoreCase))
                    .ToList();

                AssetsView = CollectionViewSource.GetDefaultView(_assets);
                AssetsView.Filter = FilterAsset;
                OnPropertyChanged(nameof(SelectedAssetType));
            }
        }

        public ICollectionView AssetsView
        {
            get => _assetsView;
            private set => SetProperty(ref _assetsView , value);
        }

        public List<string> AssetTypes
        {
            get => _assetTypes;
            private set => SetProperty(ref _assetTypes , value);
        }

        public List<AssetTypeSummary> AssetTypeSummaries
        {
            get => _assetTypeSummaries;
            private set => SetProperty(ref _assetTypeSummaries , value);
        }

        public string AssetSearchText
        {
            get => _assetSearchText;
            set
            {
                if ( SetProperty(ref _assetSearchText , value) )
                {
                    AssetsView.Refresh();
                }
            }
        }

        public IReadOnlyList<string> MessageTypes => MESSAGE_TYPES;

        public List<BuildMessage> Messages
        {
            get => _messages;
            private set
            {
                if ( !SetProperty(ref _messages , value) )
                {
                    return;
                }

                MessageSearchText = string.Empty;
                SelectedMessageType = ALL_MESSAGE_TYPES;

                MessagesView = CollectionViewSource.GetDefaultView(_messages);
                MessagesView.Filter = FilterMessage;
            }
        }

        public ICollectionView MessagesView
        {
            get => _messagesView;
            private set => SetProperty(ref _messagesView , value);
        }

        public string MessageSearchText
        {
            get => _messageSearchText;
            set
            {
                if ( SetProperty(ref _messageSearchText , value) )
                {
                    MessagesView.Refresh();
                }
            }
        }

        public string SelectedMessageType
        {
            get => _selectedMessageType;
            set
            {
                if ( SetProperty(ref _selectedMessageType , value ?? ALL_MESSAGE_TYPES) )
                {
                    MessagesView.Refresh();
                }
            }
        }

        public string SelectedAssetType
        {
            get => _selectedAssetType;
            set
            {
                if ( SetProperty(ref _selectedAssetType , value ?? ALL_ASSET_TYPES) )
                {
                    AssetsView.Refresh();
                }
            }
        }

        public async Task<bool> LoadProject_async(
            string projectPath,
            CancellationToken cancellationToken = default)
        {
            _loadCancellationTokenSource?.Cancel();

            using CancellationTokenSource loadCancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _loadCancellationTokenSource = loadCancellationTokenSource;

            ResetBuildSummary();
            ProjectPath = projectPath;
            StatusMessage = "Build Report를 불러오는 중입니다...";
            ErrorMessage = string.Empty;

            try
            {
                string reportPath = _projectFileService.GetBuildReportPath(projectPath);
                ProjectPath = Path.GetFullPath(projectPath);

                ProjectName = _projectFileService.GetProjectName(ProjectPath);
                UnityVersion = _projectFileService.GetUnityVersion(ProjectPath);

                StorageStatus = "프로젝트 용량을 계산하는 중입니다...";

                Task<ProjectSizeResult> sizeTask = _projectSizeService.CalculateProjectSize_async(
                    ProjectPath,
                    loadCancellationTokenSource.Token);

                Task reportTask = LoadBuildReportIntoView_async(
                    reportPath,
                    loadCancellationTokenSource);

                Task storageTask = LoadProjectSizeIntoView_async(
                    sizeTask,
                    loadCancellationTokenSource);

                await Task.WhenAll(reportTask, storageTask);

                return IsCurrentLoad(loadCancellationTokenSource);
            }
            catch ( Exception exception )
            {
                if ( IsCurrentLoad(loadCancellationTokenSource) )
                {
                    ErrorMessage = exception is InvalidDataException
                        ? exception.Message
                        : $"프로젝트를 분석할 수 없습니다. {exception.Message}";

                    StatusMessage = string.Empty;
                    StorageStatus = string.Empty;
                }

                return false;
            }
            finally
            {
                if ( ReferenceEquals(_loadCancellationTokenSource, loadCancellationTokenSource) )
                {
                    _loadCancellationTokenSource = null;
                }
            }
        }

        private async Task LoadBuildReportIntoView_async(
            string reportPath,
            CancellationTokenSource loadCancellationTokenSource)
        {
            try
            {
                BuildReportData reportData = await _buildReportService.LoadBuildReport_async(
                    reportPath,
                    loadCancellationTokenSource.Token);

                if ( !IsCurrentLoad(loadCancellationTokenSource) )
                {
                    return;
                }

                ApplyBuildReport(reportData);
                StatusMessage = "Build Report를 불러왔습니다.";
            }
            catch ( OperationCanceledException ) when ( loadCancellationTokenSource.IsCancellationRequested )
            {
                if ( ReferenceEquals(_loadCancellationTokenSource, loadCancellationTokenSource) )
                {
                    StatusMessage = "프로젝트 분석이 취소되었습니다.";
                }
            }
            catch ( Exception exception )
            {
                if ( !IsCurrentLoad(loadCancellationTokenSource) )
                {
                    return;
                }

                ErrorMessage = exception switch
                {
                    FileNotFoundException => "Build Report가 없습니다. Unity에서 Build 후 다시 확인하십시오.",
                    JsonException => "Build Report JSON 형식이 올바르지 않습니다.",
                    NotSupportedException or InvalidDataException => exception.Message,
                    _ => $"Build Report를 불러올 수 없습니다. {exception.Message}"
                };

                StatusMessage = string.Empty;
            }
        }

        private async Task LoadProjectSizeIntoView_async(
            Task<ProjectSizeResult> sizeTask,
            CancellationTokenSource loadCancellationTokenSource)
        {
            try
            {
                ProjectSizeResult sizeResult = await sizeTask;

                if ( !IsCurrentLoad(loadCancellationTokenSource) )
                {
                    return;
                }

                ProjectDiskSize = FormatByteSize(sizeResult.DiskSizeBytes);
                ProjectSourceSize = FormatByteSize(sizeResult.SourceSizeBytes);
                AssetsSize = FormatByteSize(sizeResult.AssetsSizeBytes);

                StorageStatus = sizeResult.IsPartial
                    ? $"읽지 못한 항목 {sizeResult.SkippedEntryCount}개: 표시된 용량은 확인된 파일의 합계입니다."
                    : "프로젝트 용량 계산 완료";

                if ( sizeResult.SkippedLinkCount > 0 )
                {
                    StorageStatus += $" 링크 {sizeResult.SkippedLinkCount}개 제외.";
                }
            }
            catch ( OperationCanceledException ) when ( loadCancellationTokenSource.IsCancellationRequested )
            {
                if ( ReferenceEquals(_loadCancellationTokenSource, loadCancellationTokenSource) )
                {
                    StorageStatus = "프로젝트 용량 계산이 취소되었습니다.";
                }
            }
            catch ( Exception exception )
            {
                if ( IsCurrentLoad(loadCancellationTokenSource) )
                {
                    StorageStatus = $"프로젝트 용량을 계산할 수 없습니다. {exception.Message}";
                }
            }
        }

        private bool IsCurrentLoad(CancellationTokenSource loadCancellationTokenSource)
        {
            return ReferenceEquals(_loadCancellationTokenSource, loadCancellationTokenSource) &&
                   !loadCancellationTokenSource.IsCancellationRequested;
        }

        private void ApplyBuildReport(BuildReportData reportData)
        {
            BuildResult = reportData.Build.Result;
            Platform = reportData.Build.Platform;

            ArtifactSize = FormatByteSize(reportData.Build.ArtifactSizeBytes);
            BuildTime = FormatBuildTime(reportData.Build.BuildTimeSeconds);

            ReportGeneratedAt = reportData.Build.ReportGeneratedAtUtc == default
                ? "-"
                : reportData.Build.ReportGeneratedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

            WarningCount = reportData.Build.WarningCount;
            ErrorCount = reportData.Build.ErrorCount;

            Assets = reportData.Assets.OrderByDescending(asset => asset.PackedSizeBytes).ToList();
            Messages = reportData.Messages;

            long totalPackedSizeBytes = reportData.Assets.Sum(asset => asset.PackedSizeBytes);

            AssetTypeSummaries = reportData.Assets
                .GroupBy(
                    asset => string.IsNullOrWhiteSpace(asset.Type) ? "Other" : asset.Type,
                    StringComparer.OrdinalIgnoreCase)
                .Select(group =>
                {
                    long packedSizeBytes = group.Sum(asset => asset.PackedSizeBytes);

                    return new AssetTypeSummary
                    {
                        Type = group.Key,
                        AssetCount = group.Count(),
                        PackedSizeBytes = packedSizeBytes,
                        SharePercent = totalPackedSizeBytes > 0
                            ? packedSizeBytes * 100.0 / totalPackedSizeBytes
                            : 0
                    };
                })
                .OrderByDescending(summary => summary.PackedSizeBytes)
                .ToList();
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

        private bool FilterAsset(object item)
        {
            if ( item is not AssetBuildInfo asset )
            {
                return false;
            }

            if ( SelectedAssetType != ALL_ASSET_TYPES &&
                 !string.Equals(asset.Type , SelectedAssetType , StringComparison.OrdinalIgnoreCase) )
            {
                return false;
            }

            string keyword = AssetSearchText.Trim();

            return keyword.Length == 0 ||
                   asset.Name.Contains(keyword , StringComparison.OrdinalIgnoreCase) || asset.Path.Contains(keyword , StringComparison.OrdinalIgnoreCase);
        }

        private bool FilterMessage(object item)
        {
            if ( item is not BuildMessage message )
            {
                return false;
            }

            if ( SelectedMessageType != ALL_MESSAGE_TYPES && !string.Equals(message.Type , SelectedMessageType , StringComparison.OrdinalIgnoreCase) )
            {
                return false;
            }

            string keyword = MessageSearchText.Trim();

            return keyword.Length == 0 || message.Message.Contains(keyword , StringComparison.OrdinalIgnoreCase);
        }

        private void ResetBuildSummary()
        {
            ProjectName = "-";
            UnityVersion = "-";
            BuildResult = "-";
            Platform = "-";
            ArtifactSize = "-";
            BuildTime = "-";
            ReportGeneratedAt = "-";
            ProjectDiskSize = "-";
            ProjectSourceSize = "-";
            AssetsSize = "-";
            StorageStatus = string.Empty;
            WarningCount = 0;
            ErrorCount = 0; 
            Assets = new List<AssetBuildInfo>();
            Messages = new List<BuildMessage>();
            AssetTypeSummaries = new List<AssetTypeSummary>();
        }
    }
}
