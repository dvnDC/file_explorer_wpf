using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace WpfApp1.Model
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public ViewModelBase() { }

        // Dodaj konstruktor przyjmujący ViewModelBase jako argument
        public ViewModelBase(ViewModelBase owner)
        {
            // Logika inicjalizacji, jeśli jest potrzebna
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}