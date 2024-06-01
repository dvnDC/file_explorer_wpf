using System.ComponentModel;
using System.Runtime.CompilerServices;
using WpfApp1.Enums;


namespace WpfApp1.Model
{
    public class SortOptions : INotifyPropertyChanged
    {
        private bool _keepFoldersOnTop;
        public bool KeepFoldersOnTop
        {
            get => _keepFoldersOnTop;
            set
            {
                if (_keepFoldersOnTop != value)
                {
                    _keepFoldersOnTop = value;
                    OnPropertyChanged();
                }
            }
        }

        private SortBy _sortBy;
        private Direction _direction;

        public SortBy SortBy
        {
            get => _sortBy;
            set
            {
                if (_sortBy != value)
                {
                    _sortBy = value;
                    OnPropertyChanged();
                }
            }
        }

        public Direction Direction
        {
            get => _direction;
            set
            {
                if (_direction != value)
                {
                    _direction = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}