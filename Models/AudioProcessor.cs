using NAudio.Wave;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System.Numerics;

namespace SpectrumAnalyzer.Models
{
    /// <summary>
    /// Handles audio capture and FFT processing for spectrum analysis
    /// </summary>
    public class AudioProcessor : IDisposable
    {
        #region Private Fields
        
        private WaveInEvent? _waveIn;
        private readonly int _sampleRate = 44100;
        private readonly int _channels = 1; // Mono
        private readonly int _bufferSize = 1024;
        private readonly int _fftSize = 1024;
        private readonly int _spectrumBars = 64;
        
        private float[] _audioBuffer;
        private Complex[] _fftBuffer;
        private double[] _windowFunction;
        private double[] _spectrumData;
        private readonly object _lockObject = new();
        
        private bool _isCapturing;
        private bool _disposed;
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Raised when new spectrum data is available
        /// </summary>
        public event Action<double[]>? SpectrumDataAvailable;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Gets whether audio capture is currently active
        /// </summary>
        public bool IsCapturing => _isCapturing;
        
        /// <summary>
        /// Gets the number of spectrum bars
        /// </summary>
        public int SpectrumBars => _spectrumBars;
        
        /// <summary>
        /// Gets or sets the amplitude sensitivity multiplier
        /// </summary>
        public double Sensitivity { get; set; } = 1.0;
        
        #endregion
        
        #region Constructor
        
        public AudioProcessor()
        {
            InitializeBuffers();
            GenerateWindowFunction();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Starts audio capture and processing
        /// </summary>
        public void StartCapture()
        {
            if (_isCapturing || _disposed)
                return;
                
            try
            {
                _waveIn = new WaveInEvent
                {
                    DeviceNumber = -1, // Default device
                    WaveFormat = new WaveFormat(_sampleRate, 16, _channels),
                    BufferMilliseconds = 23 // ~1024 samples at 44.1kHz
                };
                
                _waveIn.DataAvailable += OnDataAvailable;
                _waveIn.StartRecording();
                _isCapturing = true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to start audio capture: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Stops audio capture and processing
        /// </summary>
        public void StopCapture()
        {
            if (!_isCapturing || _disposed)
                return;
                
            _waveIn?.StopRecording();
            _waveIn?.Dispose();
            _waveIn = null;
            _isCapturing = false;
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Initializes audio and FFT buffers
        /// </summary>
        private void InitializeBuffers()
        {
            _audioBuffer = new float[_bufferSize];
            _fftBuffer = new Complex[_fftSize];
            _spectrumData = new double[_spectrumBars];
        }
        
        /// <summary>
        /// Generates Hann window function for FFT preprocessing
        /// </summary>
        private void GenerateWindowFunction()
        {
            _windowFunction = new double[_bufferSize];
            for (int i = 0; i < _bufferSize; i++)
            {
                // Hann window function
                _windowFunction[i] = 0.5 * (1.0 - Math.Cos(2.0 * Math.PI * i / (_bufferSize - 1)));
            }
        }
        
        /// <summary>
        /// Handles incoming audio data from NAudio
        /// </summary>
        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            if (!_isCapturing || _disposed)
                return;
                
            lock (_lockObject)
            {
                // Convert bytes to float samples
                ConvertBytesToFloat(e.Buffer, e.BytesRecorded);
                
                // Apply window function
                ApplyWindowFunction();
                
                // Perform FFT
                PerformFFT();
                
                // Calculate spectrum data
                CalculateSpectrum();
                
                // Notify subscribers
                SpectrumDataAvailable?.Invoke(_spectrumData);
            }
        }
        
        /// <summary>
        /// Converts audio bytes to float samples
        /// </summary>
        private void ConvertBytesToFloat(byte[] buffer, int bytesRecorded)
        {
            int sampleCount = Math.Min(bytesRecorded / 2, _bufferSize); // 16-bit = 2 bytes per sample
            
            for (int i = 0; i < sampleCount; i++)
            {
                // Convert 16-bit PCM to float (-1.0 to 1.0)
                short sample = (short)((buffer[i * 2 + 1] << 8) | buffer[i * 2]);
                _audioBuffer[i] = sample / 32768.0f;
            }
            
            // Zero-pad remaining buffer
            for (int i = sampleCount; i < _bufferSize; i++)
            {
                _audioBuffer[i] = 0.0f;
            }
        }
        
        /// <summary>
        /// Applies Hann window function to reduce spectral leakage
        /// </summary>
        private void ApplyWindowFunction()
        {
            for (int i = 0; i < _bufferSize; i++)
            {
                _audioBuffer[i] *= (float)_windowFunction[i];
            }
        }
        
        /// <summary>
        /// Performs Fast Fourier Transform using MathNet.Numerics
        /// </summary>
        private void PerformFFT()
        {
            // Copy audio data to complex buffer
            for (int i = 0; i < _bufferSize; i++)
            {
                _fftBuffer[i] = new Complex(_audioBuffer[i], 0);
            }
            
            // Zero-pad if necessary
            for (int i = _bufferSize; i < _fftSize; i++)
            {
                _fftBuffer[i] = Complex.Zero;
            }
            
            // Perform FFT
            Fourier.Forward(_fftBuffer, FourierOptions.Matlab);
        }
        
        /// <summary>
        /// Calculates spectrum data from FFT results
        /// </summary>
        private void CalculateSpectrum()
        {
            // Only use first half of FFT (positive frequencies)
            int usableBins = _fftSize / 2;
            int binsPerBar = usableBins / _spectrumBars;
            
            for (int i = 0; i < _spectrumBars; i++)
            {
                double sum = 0.0;
                int startBin = i * binsPerBar;
                int endBin = Math.Min(startBin + binsPerBar, usableBins);
                
                // Sum magnitudes for this frequency range
                for (int j = startBin; j < endBin; j++)
                {
                    double magnitude = _fftBuffer[j].Magnitude;
                    sum += magnitude * magnitude; // Power spectrum
                }
                
                // Average and apply logarithmic scaling
                double average = sum / binsPerBar;
                double logValue = Math.Log10(average + 1e-10) * Sensitivity; // Add small value to avoid log(0)
                
                // Normalize to 0-1 range (approximately)
                _spectrumData[i] = Math.Max(0, (logValue + 10) / 10); // Adjust range as needed
            }
        }
        
        #endregion
        
        #region IDisposable Implementation
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    StopCapture();
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