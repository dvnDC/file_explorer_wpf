using System;
using System.IO;
using System.Linq;
using WpfApp1.Model;
using WpfApp1.ViewModel;


namespace WpfApp1.ViewModel
{
    public class FileSystemInfoViewModel : ViewModelBase
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

        private FileSystemInfo _fileSystemInfo;
        private string _caption;
        public bool IsSelected { get; set; }

        public FileExplorer OwnerExplorer => Owner as FileExplorer;

        public FileSystemInfo Model
        {
            get => _fileSystemInfo;
            set
            {
                if (_fileSystemInfo != value)
                {
                    _fileSystemInfo = value;
                    Caption = GenerateCaption(value);
                    NotifyPropertyChanged(nameof(Model));
                }
            }
        }
        public ViewModelBase Owner { get; private set; }

        protected FileSystemInfoViewModel(ViewModelBase owner)
        {
            Owner = owner;
        }

        public long Size => Model is FileInfo fileInfo ? fileInfo.Length : (Model as DirectoryInfo)?.GetFiles().Sum(f => f.Length) ?? 0;
        public string Extension => (Model as FileInfo)?.Extension.ToLower() ?? "";
        public DateTime LastModified => Model.LastWriteTime;

        public string Caption
        {
            get => _caption;
            set
            {
                if (_caption != value)
                {
                    _caption = value;
                    NotifyPropertyChanged(nameof(Caption));
                }
            }
        }

        // generuje tekst do wyświetlenia na podstawie obiektu FileSystemInfo
        private string GenerateCaption(FileSystemInfo fileInfo)
        {
            // Sprawdzenie, czy obiekt fileInfo nie jest null przed próbą dostępu do jego właściwości
            return fileInfo != null ? fileInfo.Name : string.Empty;
        }

        public virtual void UpdateFrom(FileSystemInfo fileSystemInfo)
        {
            Model = fileSystemInfo;
            // Możesz dodać więcej logiki, jeśli potrzebujesz aktualizować więcej właściwości.
        }
    }

}