using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SpectrumAnalyzer.Models;
using SpectrumAnalyzer.Utils;

namespace SpectrumAnalyzer.ViewModels
{
    /// <summary>
    /// ViewModel for the spectrum analyzer main window
    /// </summary>
    public class SpectrumAnalyzerViewModel : ViewModelBase, IDisposable
    {
        #region Private Fields

        private readonly AudioProcessor _audioProcessor;
        private bool _isCapturing;
        private double _sensitivity = 1.0;
        private string _statusText = "Готов к запуску";
        private bool _disposed;

        #endregion

        #region Public Properties

        /// <summary>
        /// Collection of spectrum bar heights for data binding
        /// </summary>
        public ObservableCollection<double> SpectrumData { get; private set; }

        /// <summary>
        /// Gets whether audio capture is currently active
        /// </summary>
        public bool IsCapturing
        {
            get => _isCapturing;
            private set => SetProperty(ref _isCapturing, value);
        }

        /// <summary>
        /// Gets or sets the amplitude sensitivity multiplier
        /// </summary>
        public double Sensitivity
        {
            get => _sensitivity;
            set
            {
                if (SetProperty(ref _sensitivity, value))
                {
                    _audioProcessor.Sensitivity = value;
                    OnPropertyChanged(nameof(SensitivityText));
                }
            }
        }

        /// <summary>
        /// Gets the sensitivity display text
        /// </summary>
        public string SensitivityText => $"Чувствительность: {_sensitivity:F1}x";

        /// <summary>
        /// Gets the current status text
        /// </summary>
        public string StatusText
        {
            get => _statusText;
            private set => SetProperty(ref _statusText, value);
        }

        /// <summary>
        /// Gets whether the start command can execute
        /// </summary>
        public bool CanStart => !IsCapturing;

        /// <summary>
        /// Gets whether the stop command can execute
        /// </summary>
        public bool CanStop => IsCapturing;

        #endregion

        #region Commands

        /// <summary>
        /// Command to start audio capture and spectrum analysis
        /// </summary>
        public ICommand StartCommand { get; private set; }

        /// <summary>
        /// Command to stop audio capture and spectrum analysis
        /// </summary>
        public ICommand StopCommand { get; private set; }

        #endregion

        #region Constructor

        public SpectrumAnalyzerViewModel()
        {
            // Initialize audio processor
            _audioProcessor = new AudioProcessor();
            _audioProcessor.SpectrumDataAvailable += OnSpectrumDataReceived;

            // Initialize spectrum data collection
            SpectrumData = new ObservableCollection<double>();
            InitializeSpectrumData();

            // Initialize commands
            StartCommand = new RelayCommand(StartCapture, () => CanStart);
            StopCommand = new RelayCommand(StopCapture, () => CanStop);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes the spectrum data collection with default values
        /// </summary>
        private void InitializeSpectrumData()
        {
            SpectrumData.Clear();
            for (int i = 0; i < _audioProcessor.SpectrumBars; i++)
            {
                SpectrumData.Add(0.0);
            }
        }

        /// <summary>
        /// Starts audio capture and spectrum analysis
        /// </summary>
        private void StartCapture()
        {
            try
            {
                _audioProcessor.StartCapture();
                IsCapturing = true;
                StatusText = "Захват аудио активен";
                
                // Notify command states changed
                OnPropertyChanged(nameof(CanStart));
                OnPropertyChanged(nameof(CanStop));
            }
            catch (Exception ex)
            {
                StatusText = $"Ошибка: {ex.Message}";
                MessageBox.Show($"Не удалось начать захват аудио:\n{ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Stops audio capture and spectrum analysis
        /// </summary>
        private void StopCapture()
        {
            try
            {
                _audioProcessor.StopCapture();
                IsCapturing = false;
                StatusText = "Захват остановлен";
                
                // Clear spectrum display
                ClearSpectrumData();
                
                // Notify command states changed
                OnPropertyChanged(nameof(CanStart));
                OnPropertyChanged(nameof(CanStop));
            }
            catch (Exception ex)
            {
                StatusText = $"Ошибка при остановке: {ex.Message}";
            }
        }

        /// <summary>
        /// Clears the spectrum data display
        /// </summary>
        private void ClearSpectrumData()
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                for (int i = 0; i < SpectrumData.Count; i++)
                {
                    SpectrumData[i] = 0.0;
                }
            });
        }

        /// <summary>
        /// Handles new spectrum data from the audio processor
        /// </summary>
        /// <param name="spectrumValues">Array of spectrum amplitudes</param>
        private void OnSpectrumDataReceived(double[] spectrumValues)
        {
            if (_disposed || !IsCapturing)
                return;

            // Update UI on the main thread
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    // Update spectrum data collection
                    for (int i = 0; i < Math.Min(spectrumValues.Length, SpectrumData.Count); i++)
                    {
                        // Clamp values to reasonable range and smooth transitions
                        double newValue = Math.Max(0.0, Math.Min(1.0, spectrumValues[i]));
                        
                        // Apply some smoothing to reduce jitter
                        double currentValue = SpectrumData[i];
                        double smoothedValue = currentValue * 0.3 + newValue * 0.7;
                        
                        SpectrumData[i] = smoothedValue;
                    }
                    
                    StatusText = "Анализ спектра...";
                }
                catch (Exception ex)
                {
                    StatusText = $"Ошибка обновления UI: {ex.Message}";
                }
            });
        }

        #endregion

        #region IDisposable Implementation

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Stop capture if still running
                    if (IsCapturing)
                    {
                        StopCapture();
                    }
                    
                    // Dispose audio processor
                    _audioProcessor?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}