using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using WpfApp1.Enums;
using WpfApp1.Helper;
using WpfApp1.Model;

namespace WpfApp1.ViewModel
{
    public class DirectoryInfoViewModel : FileSystemInfoViewModel
    {
        public DispatchedObservableCollection<FileSystemInfoViewModel> Items { get; private set; }
            = new DispatchedObservableCollection<FileSystemInfoViewModel>();

        private FileSystemWatcher _watcher;

        // Flaga wskazująca, czy folder jest rozwinięty
        public bool IsExpanded { get; set; }
        // Flaga wskazująca, czy element jest wybrany
        public bool IsSelected { get; set; }

        private FileExplorer _fileExplorer;

        public DirectoryInfoViewModel(FileExplorer owner) : base(owner)
        {
            _fileExplorer = owner;
            Items.CollectionChanged += Items_CollectionChanged;
        }
        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in args.NewItems.Cast<FileSystemInfoViewModel>())
                    {
                        item.PropertyChanged += Item_PropertyChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in args.OldItems.Cast<FileSystemInfoViewModel>())
                    {
                        item.PropertyChanged -= Item_PropertyChanged;
                    }
                    break;
            }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(StatusMessage) && sender is FileSystemInfoViewModel viewModel)
            {
                StatusMessage = viewModel.StatusMessage;
            }
        }

        public bool Open(string path, bool ignoreUnauthorizedAccess = false)
        {
            if (_watcher != null)
            {
                _watcher.Dispose();
            }

            _watcher = new FileSystemWatcher(path)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                EnableRaisingEvents = true
            };

            _watcher.Changed += (sender, e) => OnFileSystemChanged(e);
            _watcher.Created += (sender, e) => OnFileSystemChanged(e);
            _watcher.Deleted += (sender, e) => OnFileSystemChanged(e);
            _watcher.Renamed += (sender, e) => OnFileSystemRenamed(e);
            _watcher.Error += Watcher_Error;

            Items.Clear();
            var directoryInfo = new DirectoryInfo(path);
            return LoadDirectory(directoryInfo, ignoreUnauthorizedAccess);
        }

        private bool LoadDirectory(DirectoryInfo directoryInfo, bool ignoreUnauthorizedAccess)
        {
            if (directoryInfo.Exists)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Items.Clear();

                    foreach (var directory in directoryInfo.GetDirectories())
                    {
                        try
                        {
                            if (directory.Exists)
                            {
                                var directoryVM = new DirectoryInfoViewModel(_fileExplorer) { Model = directory };
                                Items.Add(directoryVM);
                                directoryVM.LoadDirectory(directory, ignoreUnauthorizedAccess);
                            }
                            StatusMessage = $"Loading directory: {directory.FullName}"; // Ustawienie StatusMessage
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // Ignorujemy zastrzeżone katalogi
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show($"Wystąpił błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                    foreach (var file in directoryInfo.GetFiles())
                    {
                        try
                        {
                            if (file.Exists)
                            {
                                Items.Add(new FileInfoViewModel(_fileExplorer) { Model = file });
                            }
                            StatusMessage = $"Loading file: {file.FullName}"; // Ustawienie StatusMessage
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // Ignorujemy zastrzeżone pliki
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show($"Wystąpił błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                    // Ustawienie StatusMessage po załadowaniu katalogu
                    StatusMessage = $"Loaded directory: {directoryInfo.FullName}";
                });
                return true;
            }
            return false;
        }

        private void OnFileSystemChanged(FileSystemEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    // Utworzenie DirectoryInfo na podstawie ścieżki usuniętego pliku/katalogu
                    var fullPath = e.FullPath;
                    var parentDirPath = Path.GetDirectoryName(fullPath); // Pobranie ścieżki do katalogu nadrzędnego
                    var parentDirInfo = new DirectoryInfo(parentDirPath);

                    if (parentDirInfo.Exists)
                    {
                        LoadDirectory(parentDirInfo, ignoreUnauthorizedAccess: true); // Odświeżenie katalogu nadrzędnego
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Wystąpił błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        private void OnFileSystemRenamed(RenamedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    var directoryInfo = new DirectoryInfo(e.FullPath);
                    if (directoryInfo.Exists)
                    {
                        LoadDirectory(directoryInfo.Parent, ignoreUnauthorizedAccess: true);
                    }
                    else
                    {
                        // Jeśli katalog nie istnieje, odśwież katalog nadrzędny
                        LoadDirectory(new DirectoryInfo(e.OldFullPath).Parent, ignoreUnauthorizedAccess: true);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Wystąpił błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        public void Sort(SortBy sortBy, Direction direction, bool keepFoldersOnTop)
        {
            var comparer = GetComparer(sortBy);
            if (direction == Direction.Descending)
            {
                comparer = new ReverseComparer(comparer);
            }

            // Utwórz nową listę, która będzie przechowywać posortowane elementy z oryginalnymi referencjami
            var sortedItems = Items.OrderBy(item => item, comparer).ToList();
            if (keepFoldersOnTop)
            {
                sortedItems = sortedItems.OrderBy(item => !(item is DirectoryInfoViewModel)).ThenBy(item => item, comparer).ToList();
            }

            // Stworzenie mapy stanów dla zachowania IsExpanded i IsSelected
            var stateMap = Items.OfType<DirectoryInfoViewModel>().ToDictionary(
                item => item.Model.FullName,
                item => (IsExpanded: item.IsExpanded, IsSelected: item.IsSelected)
            );

            // Zaktualizuj kolekcję Items, zachowując oryginalne obiekty
            UpdateItemsInPlace(sortedItems, stateMap);
        }

        private void UpdateItemsInPlace(List<FileSystemInfoViewModel> sortedItems, Dictionary<string, (bool IsExpanded, bool IsSelected)> stateMap)
        {
            var currentItems = new List<FileSystemInfoViewModel>(Items);
            Items.Clear();

            foreach (var sortedItem in sortedItems)
            {
                var currentItem = currentItems.FirstOrDefault(i => i.Model.FullName == sortedItem.Model.FullName);
                if (currentItem != null)
                {
                    if (currentItem is DirectoryInfoViewModel dir)
                    {
                        if (stateMap.TryGetValue(currentItem.Model.FullName, out var state))
                        {
                            dir.IsExpanded = state.IsExpanded;
                            currentItem.IsSelected = state.IsSelected;
                        }
                    }
                    Items.Add(currentItem);
                }
                else
                {
                    Items.Add(sortedItem);
                }
            }
        }

        private void RestoreState(List<DirectoryInfoViewModel> expandedItems, FileSystemInfoViewModel selectedItem)
        {
            foreach (var dir in Items.OfType<DirectoryInfoViewModel>())
            {
                dir.IsExpanded = expandedItems.Contains(dir); // Ustaw stan rozwinięcia

                // Ustaw stan zaznaczenia
                if (selectedItem != null && dir.Model.FullName == selectedItem.Model.FullName)
                {
                    dir.IsSelected = true;
                    // Jeśli aplikacja to obsługuje, zapewnij, że zaznaczony element jest widoczny
                }
            }
        }

        private IComparer<FileSystemInfoViewModel> GetComparer(SortBy sortBy)
        {
            switch (sortBy)
            {
                case SortBy.ByExtension:
                    return new ExtensionComparer();
                case SortBy.BySize:
                    return new SizeComparer();
                case SortBy.ByDateModified:
                    return new DateModifiedComparer();
                default:
                    return new AlphabeticalComparer();
            }
        }

        public void SortItems()
        {
            //TODO Items = new ObservableCollection<FileSystemInfoViewModel>(Items.OrderBy(i => i.Caption));
            foreach (var item in Items.OfType<DirectoryInfoViewModel>())
            {
                item.SortItems(); // Rekurencyjne sortowanie podkatalogów
            }
        }

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            // Obsługa błędów monitorowania
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Tutaj możesz wyświetlić komunikat o błędzie lub podjąć inne działania
            });
        }

        public override void UpdateFrom(FileSystemInfo fileSystemInfo)
        {
            base.UpdateFrom(fileSystemInfo);  // Wywołuje implementację klasy bazowej
                                              // Specyficzna dla katalogu logika aktualizacji, jeśli potrzebna
                                              // Na przykład załadowanie dodatkowych właściwości
        }
    }
}