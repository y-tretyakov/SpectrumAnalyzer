using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SpectrumAnalyzer.Utils
{
    /// <summary>
    /// Base class for ViewModels that implements INotifyPropertyChanged
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        /// <param name="propertyName">Name of the property that changed</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Sets the property value and raises PropertyChanged if the value changed
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="field">Reference to the backing field</param>
        /// <param name="value">New value</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>True if the value changed, false otherwise</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}