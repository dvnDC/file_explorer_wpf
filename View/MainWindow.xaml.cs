using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfApp1.Model;

namespace WpfApp1.View
{
    public partial class MainWindow : Window
    {
        private FileExplorer _fileExplorer;

        public MainWindow()
        {
            InitializeComponent();
            _fileExplorer = new FileExplorer();
            DataContext = _fileExplorer;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await _fileExplorer.OpenRootFolderAsync();
        }

        private async void Window_Loaded2(object sender, RoutedEventArgs e)
        {
            await _fileExplorer.OpenRootFolderAsync();
        }

        private void OnPolishLanguageSelected(object sender, MouseButtonEventArgs e)
        {
            _fileExplorer.ChangeLanguage("pl-PL");
        }

        private void OnEnglishLanguageSelected(object sender, MouseButtonEventArgs e)
        {
            _fileExplorer.ChangeLanguage("en-US");
        }

        private void ChangeCulture(CultureInfo culture)
        {
            _fileExplorer.ChangeCulture(culture.Name);
            RefreshResources();
        }

        private void RefreshResources()
        {
            // Implement logic to refresh UI elements that depend on localized resources
        }
    }
}