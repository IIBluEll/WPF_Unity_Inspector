using Microsoft.Win32;
using System.Windows;
using WPF_UnityInspector.Services;
using WPF_UnityInspector.ViewModels;

namespace WPF_UnityInspector
{
    public partial class MainWindow : Window
    {
        private readonly DashboardViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new DashboardViewModel(new BuildReportService(), new ProjectFileService());

            DataContext = _viewModel;
        }

        private async void OnSelectProjectClicked(object sender, RoutedEventArgs eventArgs)
        {
            OpenFolderDialog folderDialog = new()
            {
                Title = "Unity 프로젝트 폴더 선택",
                Multiselect = false
            };

            bool? dialogResult = folderDialog.ShowDialog(this);

            if(dialogResult != true)
            {
                return;
            }

            await _viewModel.LoadProject_async(folderDialog.FolderName);
        }
    }
}