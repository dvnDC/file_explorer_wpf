
using System;
using System.Windows;
using WpfApp1.Enums;
using WpfApp1.Model;

namespace WpfApp1.View
{
    public partial class SortDialog : Window
    {
        private FileExplorer _fileExplorer;

        public SortDialog(FileExplorer fileExplorer)
        {
            InitializeComponent();
            _fileExplorer = fileExplorer;
        }

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            // Parsowanie stringów na enum
            var sortType = (SortBy)Enum.Parse(typeof(SortBy), GetSelectedSortType());
            var sortOrder = (Direction)Enum.Parse(typeof(Direction), GetSelectedSortOrder());
            bool keepFoldersOnTop = SeparateFoldersCheckBox.IsChecked ?? false;

            _fileExplorer.SortDirectory(sortType, sortOrder, keepFoldersOnTop); // Przekazanie trzech argumentów
            this.DialogResult = true;
        }


        private string GetSelectedSortType()
        {
            if (AlphabeticallyRadioButton.IsChecked == true) return "Alphabetically";
            if (ByExtensionRadioButton.IsChecked == true) return "ByExtension";
            if (BySizeRadioButton.IsChecked == true) return "BySize";
            if (ByDateModifiedRadioButton.IsChecked == true) return "ByDateModified";
            return "Alphabetically"; // Domyślnie sortuj alfabetycznie
        }

        private string GetSelectedSortOrder()
        {
            return AscendingRadioButton.IsChecked == true ? "Ascending" : "Descending";
        }
    }
}