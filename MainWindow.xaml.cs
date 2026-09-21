using Microsoft.Win32;
using System.IO;
using System.Windows;
using WPF_UnityInspector.Models;
using WPF_UnityInspector.Services;
using WPF_UnityInspector.ViewModels;
using WPF_UnityInspector.Views;

namespace WPF_UnityInspector
{
    public partial class MainWindow : Window
    {
        private readonly DashboardViewModel _viewModel;
        private readonly IProjectFileService _projectFileService;
        private readonly ProjectSettingsService _projectSettingsService;

        public MainWindow()
        {
            InitializeComponent();

            _projectFileService = new ProjectFileService();
            _projectSettingsService = new ProjectSettingsService();
            _viewModel = new DashboardViewModel(new BuildReportService() , _projectFileService , new ProjectSizeService());

            DataContext = _viewModel;
            Loaded += OnWindowLoaded;
        }

        private async void OnWindowLoaded(object sender , RoutedEventArgs eventArgs)
        {
            Loaded -= OnWindowLoaded;

            try
            {
                string? projectPath = _projectSettingsService.LoadLastProjectPath();

                if ( projectPath is not null && _projectFileService.IsUnityProject(projectPath) )
                {
                    ShowBuildReportGuide(projectPath);
                    await _viewModel.LoadProject_async(projectPath);
                }
            }
            catch ( Exception exception ) when ( exception is IOException or UnauthorizedAccessException )
            {
                MessageBox.Show(
                    this ,
                    "최근 프로젝트 경로를 읽을 수 없습니다." ,
                    "Unity Project Inspector" ,
                    MessageBoxButton.OK ,
                    MessageBoxImage.Warning);
            }
        }

        private async void OnSelectProjectClicked(object sender , RoutedEventArgs eventArgs)
        {
            OpenFolderDialog folderDialog = new()
            {
                Title = "Unity 프로젝트 폴더 선택",
                Multiselect = false
            };

            if ( folderDialog.ShowDialog(this) != true )
            {
                return;
            }

            string projectPath = folderDialog.FolderName;
            bool isUnityProject = _projectFileService.IsUnityProject(projectPath);

            if ( isUnityProject )
            {
                ShowBuildReportGuide(projectPath);
            }

            bool isCurrentProject = await _viewModel.LoadProject_async(projectPath);

            if ( !isCurrentProject || !isUnityProject )
            {
                return;
            }

            try
            {
                _projectSettingsService.SaveLastProjectPath(projectPath);
            }
            catch ( Exception exception ) when (exception is IOException or UnauthorizedAccessException )
            {
                MessageBox.Show(
                    this ,
                    "프로젝트는 열었지만 최근 프로젝트 경로를 저장하지 못했습니다." ,
                    "Unity Project Inspector" ,
                    MessageBoxButton.OK ,
                    MessageBoxImage.Warning);
            }
        }

        private async void OnRefreshProjectClicked(object sender , RoutedEventArgs eventArgs)
        {
            string projectPath = _viewModel.ProjectPath;

            if ( !_projectFileService.IsUnityProject(projectPath) )
            {
                MessageBox.Show(
                    this ,
                    "먼저 Unity 프로젝트를 선택해 주세요." ,
                    "Unity Project Inspector" ,
                    MessageBoxButton.OK ,
                    MessageBoxImage.Information);

                return;
            }

            if(!_projectFileService.HasBuildReport(projectPath))
            {
                ShowBuildReportGuide(projectPath);
                return;
            }

            await _viewModel.ReloadBuildReport_async();
        }

        private void ShowBuildReportGuide(string projectPath)
        {
            if ( _projectFileService.HasBuildReport(projectPath) )
                return;

            UNITY_PACKAGE_STATE packageState =
        _projectFileService.GetInspectorPackageState(projectPath);

            switch ( packageState )
            {
                case UNITY_PACKAGE_STATE.NOT_INSTALLED:
                    ShowPackageInstallationGuide();
                    break;

                case UNITY_PACKAGE_STATE.INSTALLED:
                    ShowBuildRequiredGuide();
                    break;

                case UNITY_PACKAGE_STATE.UNKNOWN:
                    ShowPackageCheckFailedGuide();
                    break;
            }
        }

        private void ShowPackageInstallationGuide()
        {
            PackageInstallationDialog dialog = new()
            {
                Owner = this
            };

            dialog.ShowDialog();
        }

        private static void ShowBuildRequiredGuide()
        {
            MessageBox.Show(
                """
        Unity Project Inspector 패키지는 설치되어 있지만
        Build Report가 아직 생성되지 않았습니다.

        Unity 프로젝트를 한 번 빌드한 후
        새로고침 버튼을 눌러 주세요.
        """ ,
                "Build Report 없음" ,
                MessageBoxButton.OK ,
                MessageBoxImage.Information);
        }

        private static void ShowPackageCheckFailedGuide()
        {
            MessageBox.Show(
                """
        Build Report가 없으며 패키지 설치 상태를 확인할 수 없습니다.

        Unity 프로젝트의 Packages/manifest.json 파일을
        확인해 주세요.
        """ ,
                "패키지 상태 확인 실패" ,
                MessageBoxButton.OK ,
                MessageBoxImage.Warning);
        }
    }
}
