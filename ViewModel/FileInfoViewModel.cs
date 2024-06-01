using System;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using WpfApp1.Helper;
using System.Windows;
using WpfApp1.Model;

namespace WpfApp1.ViewModel
{
    public class FileInfoViewModel : FileSystemInfoViewModel
    {
        private ImageSource _icon;
        public ICommand OpenFileCommand { get; }

        private FileExplorer _fileExplorer;

        public FileInfoViewModel(FileExplorer owner) : base(owner)
        {
            _fileExplorer = owner;
            OpenFileCommand = new RelayCommand(OpenFileExecute, OpenFileCanExecute);
        }

        private bool OpenFileCanExecute(object parameter)
        {
            return _fileExplorer.OpenFileCommand.CanExecute(this);
        }

        private void OpenFileExecute(object parameter)
        {
            _fileExplorer.OpenFileCommand.Execute(this);
        }

        public ImageSource Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                NotifyPropertyChanged(nameof(Icon));
            }
        }

        public new FileInfo Model
        {
            get => (FileInfo)base.Model;
            set
            {
                base.Model = value;
                if (value != null)
                {
                    LoadIcon(value);
                }
            }
        }
        private void ExecuteOpenFile(object parameter)
        {
            if (parameter is FileInfoViewModel fileInfoViewModel)
            {
                var fileExplorer = this.Owner as FileExplorer; // Zakładając, że Owner jest przekazany poprawnie
                if (fileExplorer != null && fileInfoViewModel.Model is FileInfo fileInfo)
                {
                    fileExplorer.OpenFile(fileInfo.FullName);
                }
            }
        }


        private bool CanExecuteOpenFile(object parameter)
        {
            return true; // Tutaj można dodać bardziej szczegółowe warunki, jeśli potrzebne
        }

        private bool CanOpenFile(object parameter)
        {
            return true; // Na razie zawsze zwracaj true, umożliwiając otwarcie pliku
        }

        private void OpenFile(object parameter)
        {
            if (Model is FileInfo fileInfo)
            {
                try
                {
                    string fileContent = File.ReadAllText(fileInfo.FullName);
                    _fileExplorer.SelectedItemContent = fileContent; // Aktualizacja właściwości w FileExplorer
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadIcon(FileInfo fileInfo)
        {
            var extension = fileInfo.Extension.ToLower();
            switch (extension)
            {
                case ".txt":
                    Icon = new BitmapImage(new Uri("pack://application:,,,/Asset/Icon/FileTxt.png"));
                    break;
                case ".png":
                    Icon = new BitmapImage(new Uri("pack://application:,,,/Asset/Icon/FilePhoto.png"));
                    break;
                case ".jpg":
                    Icon = new BitmapImage(new Uri("pack://application:,,,/Asset/Icon/FilePhoto.png"));
                    break;
                default:
                    Icon = new BitmapImage(new Uri("pack://application:,,,/Asset/Icon/File.png"));
                    break;
            }
        }
        public override void UpdateFrom(FileSystemInfo fileInfo)
        {
            base.UpdateFrom(fileInfo); // Wywołanie metody klasy bazowej
                                       // Tu możesz dodać specyficzną logikę dla plików, jeśli potrzebujesz
        }
    }
}