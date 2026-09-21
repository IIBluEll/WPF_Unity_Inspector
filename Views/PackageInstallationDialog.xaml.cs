using System.Runtime.InteropServices;
using System.Windows;

namespace WPF_UnityInspector.Views
{
    public partial class PackageInstallationDialog : Window
    {
        private const string PACKAGE_GIT_URL =
            "https://github.com/IIBluEll/UnityPackage_Unity_Inspector.git";

        public PackageInstallationDialog()
        {
            InitializeComponent();
            packageUrlTxt.Text = PACKAGE_GIT_URL;
        }

        protected override void OnContentRendered(EventArgs eventArgs)
        {
            base.OnContentRendered(eventArgs);

            packageUrlTxt.Focus();
            packageUrlTxt.SelectAll();
        }

        private void OnCopyPackageUrlClicked(
            object sender,
            RoutedEventArgs eventArgs)
        {
            try
            {
                Clipboard.SetText(PACKAGE_GIT_URL);
                copyUrlBtn.Content = "복사 완료";
            }
            catch ( COMException )
            {
                MessageBox.Show(
                    this,
                    "클립보드를 사용할 수 없습니다. 주소를 직접 선택해서 복사해 주세요.",
                    "주소 복사 실패",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }
}
