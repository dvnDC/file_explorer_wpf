using System;
using System.Windows.Input;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using WpfApp1.ViewModel;
using WpfApp1.Helper;
using WpfApp1.View;
using WpfApp1.Dictionary.Language;
using WpfApp1.Enums;
using System.ComponentModel;

namespace WpfApp1.Model
{
    public class FileExplorer : ViewModelBase
    {
        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                NotifyPropertyChanged(nameof(StatusMessage));
            }
        }

        public FileExplorer()
        {
            OpenCommand = new RelayCommand(Open);
            ExitCommand = new RelayCommand(Exit);
            SortRootFolderCommand = new RelayCommand(o => ShowSortDialog(), o => Root != null && Root.Items.Any());
            OpenFileCommand = new RelayCommand(OpenFileExecute, OpenFileCanExecute);
            OpenRootFolderCommand = new AsyncRelayCommand(param => OpenRootFolderAsync());

            SelectedLanguage = "en-US";
        }

        public ICommand OpenCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand SortRootFolderCommand { get; private set; }
        public ICommand OpenFileCommand { get; }
        public ICommand OpenRootFolderCommand { get; }
        public static readonly string[] TextFilesExtensions = { ".txt", ".ini", ".log" };

        private DirectoryInfoViewModel _root;
        public DirectoryInfoViewModel Root
        {
            get => _root;
            set
            {
                _root = value;
                NotifyPropertyChanged(nameof(Root));
            }
        }

        private string _selectedItemContent;
        public string SelectedItemContent
        {
            get => _selectedItemContent;
            set
            {
                _selectedItemContent = value;
                NotifyPropertyChanged(nameof(SelectedItemContent));
            }
        }

        private string _selectedLanguage;
        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                _selectedLanguage = value;
                NotifyPropertyChanged(nameof(SelectedLanguage));
                NotifyPropertyChanged(nameof(IsPolishSelected));
                NotifyPropertyChanged(nameof(IsEnglishSelected));
            }
        }

        public bool IsPolishSelected => SelectedLanguage == "pl-PL";
        public bool IsEnglishSelected => SelectedLanguage == "en-US";

        public async Task OpenRootFolderAsync()
        {
            var dlg = new FolderBrowserDialog() { Description = "Select directory to open" };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var path = dlg.SelectedPath;
                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        OpenRoot(path, ignoreUnauthorizedAccess: true);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        System.Windows.MessageBox.Show($"Access to the path '{path}' is denied.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (IOException ex)
                    {
                        System.Windows.MessageBox.Show($"An I/O error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
        }

        private void Open(object parameter)
        {
            var description = Strings.SelectDirectoryDescription;

            var dialog = new FolderBrowserDialog() { Description = description };
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string path = dialog.SelectedPath;
                OpenRoot(path, ignoreUnauthorizedAccess: true);
            }
        }

        public void OpenFile(string filePath)
        {
            try
            {
                // Odczytanie zawartości pliku
                string fileContent = File.ReadAllText(filePath);

                // Przypisanie zawartości pliku do właściwości w ViewModel, która jest powiązana z widokiem
                SelectedItemContent = fileContent;
            }
            catch (Exception ex)
            {
                // Obsługa wyjątków, np. gdy plik nie istnieje, nie ma uprawnień itp.
                System.Windows.MessageBox.Show($"Error reading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Exit(object parameter)
        {
            System.Windows.Application.Current.Shutdown();
        }

        /*public void OpenRoot(string path)
        {
            Root = new DirectoryInfoViewModel(this);
            bool isOpened = Root.Open(path);

            if (isOpened)
            {
                NotifyPropertyChanged(nameof(Root)); // Poinformuj, że Root się zmienił
            }
        }*/

        public void OpenRoot(string path, bool ignoreUnauthorizedAccess)
        {
            Root = new DirectoryInfoViewModel(this);
            bool isOpened = Root.Open(path, ignoreUnauthorizedAccess);

            if (isOpened)
            {
                NotifyPropertyChanged(nameof(Root)); // Poinformuj, że Root się zmienił
                Root.PropertyChanged += Root_PropertyChanged;
                StatusMessage = "Ready";
            }
        }
        private void Root_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(StatusMessage) && sender is FileSystemInfoViewModel viewModel)
            {
                StatusMessage = viewModel.StatusMessage;
            }
        }

        private bool OpenFileCanExecute(object parameter)
        {
            if (parameter is FileInfoViewModel viewModel)
            {
                var extension = viewModel.Extension?.ToLower();
                return TextFilesExtensions.Contains(extension);
            }
            return false;
        }

        private void OpenFileExecute(object parameter)
        {
            if (parameter is FileInfoViewModel viewModel)
            {
                OpenFile(viewModel.Model.FullName);
            }
        }

        private SortOptions _sortOptions = new SortOptions();
        public SortOptions SortOptions
        {
            get => _sortOptions;
            set
            {
                _sortOptions = value;
                NotifyPropertyChanged(nameof(SortOptions));
            }
        }

        private bool CanSortRootFolderExecute(object parameter)
        {
            // Komenda może być wykonana tylko jeśli drzewo katalogów jest załadowane
            return Root != null && Root.Items.Any();
        }

        private void SortRootFolderExecute(object parameter)
        {
            if (Root != null)
            {
                // Przykładowo, sortuj alfabetycznie i rosnąco z folderami na górze
                Root.Sort(SortBy.Alphabetically, Direction.Ascending, true);
                NotifyPropertyChanged(nameof(Root)); // Odświeżenie widoku
            }
        }

        public void ChangeCulture(string cultureName)
        {
            CultureInfo.CurrentCulture = new CultureInfo(cultureName);
            CultureInfo.CurrentUICulture = new CultureInfo(cultureName);

            NotifyPropertyChanged("Lang");
        }

        public string Lang => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        public void ChangeLanguage(string language)
        {
            CultureResources.ChangeCulture(new CultureInfo(language));
            SelectedLanguage = language;
        }

        private void ShowSortDialog()
        {
            var dialog = new SortDialog(this); // Przekazuj 'this' jako FileExplorer ViewModel
            if (dialog.ShowDialog() == true)
            {
                // Można dodać dodatkowe działania po zamknięciu dialogu, jeśli jest to potrzebne
            }
        }

        public void SortDirectory(SortBy sortType, Direction sortOrder, bool keepFoldersOnTop)
        {
            if (Root != null)
            {
                Root.Sort(sortType, sortOrder, keepFoldersOnTop);
                NotifyPropertyChanged(nameof(Root)); // Odświeżenie widoku
            }
        }
    }
}