using Microsoft.Win32;
using System.IO;
using System.Windows;
using WPF_UnityInspector.Services;
using WPF_UnityInspector.ViewModels;

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
            _viewModel = new DashboardViewModel(new BuildReportService(), _projectFileService, new ProjectSizeService());

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
                    await _viewModel.LoadProject_async(projectPath);
                }
            }
            catch ( Exception exception ) when (exception is IOException or UnauthorizedAccessException )
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

            bool isCurrentProject = await _viewModel.LoadProject_async(folderDialog.FolderName);

            if ( !isCurrentProject )
            {
                return;
            }

            if ( !_projectFileService.IsUnityProject(folderDialog.FolderName) )
            {
                return;
            }

            try
            {
                _projectSettingsService.SaveLastProjectPath(folderDialog.FolderName);
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
    }
}