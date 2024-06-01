using System;
using System.Collections.Generic;
using System.IO;
using WpfApp1.ViewModel;

namespace WpfApp1.Helper
{
    public class AlphabeticalComparer : IComparer<FileSystemInfoViewModel>
    {
        public int Compare(FileSystemInfoViewModel x, FileSystemInfoViewModel y)
        {
            return string.Compare(x.Caption, y.Caption, StringComparison.OrdinalIgnoreCase);
        }
    }

    public class ReverseComparer : IComparer<FileSystemInfoViewModel>
    {
        private IComparer<FileSystemInfoViewModel> originalComparer;

        public ReverseComparer(IComparer<FileSystemInfoViewModel> original)
        {
            originalComparer = original;
        }

        public int Compare(FileSystemInfoViewModel x, FileSystemInfoViewModel y)
        {
            return -originalComparer.Compare(x, y);
        }
    }



    public class SizeComparer : IComparer<FileSystemInfoViewModel>
    {
        public int Compare(FileSystemInfoViewModel x, FileSystemInfoViewModel y)
        {
            // Porównaj rozmiary plików lub folderów
            return x.Size.CompareTo(y.Size);
        }
    }

    public class ExtensionComparer : IComparer<FileSystemInfoViewModel>
    {
        public int Compare(FileSystemInfoViewModel x, FileSystemInfoViewModel y)
        {
            // Porównaj rozszerzenia plików
            return string.Compare(x.Extension, y.Extension);
        }
    }

    public class DateModifiedComparer : IComparer<FileSystemInfoViewModel>
    {
        public int Compare(FileSystemInfoViewModel x, FileSystemInfoViewModel y)
        {
            // Porównaj daty modyfikacji
            return DateTime.Compare(x.LastModified, y.LastModified);
        }
    }
}
