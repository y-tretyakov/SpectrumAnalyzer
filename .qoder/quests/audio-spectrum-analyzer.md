# Audio Spectrum Analyzer Design Document

## Overview

The Audio Spectrum Analyzer is a real-time audio visualization application built with .NET 9 and WPF. The application captures audio input from the default microphone device, performs Fast Fourier Transform (FFT) analysis, and displays the frequency spectrum as animated vertical bars with a mirror reflection effect.

**Target Platform**: .NET 9  
**UI Framework**: WPF  
**Programming Language**: C#  
**Architecture Pattern**: MVVM  

## Technology Stack & Dependencies

### Core Libraries
- **NAudio** - Primary audio processing library for audio capture and manipulation
- **MathNet.Numerics** - Mathematical operations library, specifically for FFT calculations
- **WPF** - Windows Presentation Foundation for UI framework

### Development Dependencies
- .NET 9 SDK
- Visual Studio 2022 or compatible IDE

## Architecture

### MVVM Architecture Overview

```mermaid
graph TB
    V[View - MainWindow.xaml] --> VM[ViewModel - SpectrumAnalyzerViewModel]
    VM --> M[Model - AudioProcessor]
    M --> NAudio[NAudio WaveInEvent]
    M --> Math[MathNet.Numerics FFT]
    VM --> UI[UI Data Binding]
    UI --> Visual[Spectrum Visualization]
```

### Component Architecture

#### Component Definition

**AudioProcessor (Model)**
- Responsible for audio capture using NAudio
- Handles real-time audio data processing
- Performs FFT analysis using MathNet.Numerics
- Applies windowing functions (Hann/Hamming)

**SpectrumAnalyzerViewModel**
- Manages application state and UI data
- Contains ObservableCollection for spectrum bar heights
- Handles start/stop commands
- Coordinates between audio processing and UI updates

**MainWindow (View)**
- Primary UI containing spectrum visualization
- Control buttons (Start/Stop)
- Optional sensitivity/scale controls

#### Component Hierarchy

```mermaid
graph TD
    MW[MainWindow] --> SAV[SpectrumAnalyzerViewModel]
    SAV --> AP[AudioProcessor]
    MW --> SC[SpectrumCanvas]
    SC --> SB[SpectrumBar Items]
    MW --> CP[ControlPanel]
    CP --> StartBtn[Start Button]
    CP --> StopBtn[Stop Button]
    CP --> SensSlider[Sensitivity Slider]
```

#### Props/State Management

**SpectrumAnalyzerViewModel Properties:**
- `ObservableCollection<double> SpectrumData` - Bar heights data
- `bool IsCapturing` - Audio capture state
- `double Sensitivity` - Amplitude sensitivity setting
- `ICommand StartCommand` - Start audio capture
- `ICommand StopCommand` - Stop audio capture

**AudioProcessor Configuration:**
- Sample Rate: 44,100 Hz
- Buffer Size: 1024 samples
- Channels: Mono
- Bit Depth: 16-bit

### Data Flow Architecture

```mermaid
sequenceDiagram
    participant UI as MainWindow
    participant VM as ViewModel
    participant AP as AudioProcessor
    participant NAudio as NAudio Engine
    participant FFT as MathNet FFT
    
    UI->>VM: Start Command
    VM->>AP: StartCapture()
    AP->>NAudio: Initialize WaveInEvent
    NAudio->>AP: DataAvailable Event
    AP->>FFT: Perform FFT Analysis
    FFT->>AP: Frequency Domain Data
    AP->>VM: Update SpectrumData
    VM->>UI: NotifyPropertyChanged
    UI->>UI: Animate Spectrum Bars
```

## Component Architecture Details

### AudioProcessor Implementation

```mermaid
classDiagram
    class AudioProcessor {
        -WaveInEvent waveIn
        -float[] audioBuffer
        -Complex[] fftBuffer
        -double[] windowFunction
        +event Action~double[]~ SpectrumDataAvailable
        +StartCapture()
        +StopCapture()
        -OnDataAvailable()
        -ApplyWindow()
        -PerformFFT()
        -CalculateSpectrum()
    }
    
    class WaveInEvent {
        +event DataAvailableEventHandler DataAvailable
        +StartRecording()
        +StopRecording()
    }
    
    AudioProcessor --> WaveInEvent
```

### ViewModel Data Binding

```mermaid
classDiagram
    class SpectrumAnalyzerViewModel {
        +ObservableCollection~double~ SpectrumData
        +bool IsCapturing
        +double Sensitivity
        +ICommand StartCommand
        +ICommand StopCommand
        -AudioProcessor audioProcessor
        +StartCapture()
        +StopCapture()
        -OnSpectrumDataReceived()
    }
    
    class MainWindow {
        +SpectrumAnalyzerViewModel DataContext
    }
    
    SpectrumAnalyzerViewModel --> AudioProcessor
    MainWindow --> SpectrumAnalyzerViewModel
```

## Styling Strategy

### Color Palette
- **Background**: Dark gray (#1E1E1E) or black (#000000)
- **Spectrum Bars**: Orange gradient (#FF8C00 to #FFD700)
- **Reflection**: Semi-transparent orange with vertical gradient
- **Controls**: Light gray (#CCCCCC) text with dark backgrounds

### Visual Effects
- **Gradient Brushes**: For spectrum bar coloring
- **Reflection Transform**: ScaleTransform with Y-axis flip
- **Smooth Animations**: DoubleAnimation for bar height transitions
- **Opacity Masks**: For reflection fade effect

## State Management

### Application State Flow

```mermaid
stateDiagram-v2
    [*] --> Idle
    Idle --> Capturing : Start Command
    Capturing --> Processing : Audio Data Available
    Processing --> Updating : FFT Complete
    Updating --> Capturing : UI Updated
    Capturing --> Idle : Stop Command
    Idle --> [*]
```

### Data Binding Strategy
- **OneWay Binding**: Spectrum data from ViewModel to View
- **TwoWay Binding**: Sensitivity slider value
- **Command Binding**: Start/Stop button actions
- **Property Change Notifications**: INotifyPropertyChanged implementation

## API Integration Layer

### Audio Device Integration

**WaveInEvent Configuration:**
```
- DeviceNumber: -1 (default device)
- WaveFormat: 44100 Hz, 16-bit, Mono
- BufferMilliseconds: 23ms (1024 samples)
- NumberOfBuffers: 3
```

**FFT Configuration:**
```
- Window Function: Hann Window
- FFT Size: 1024 points
- Frequency Resolution: 43.066 Hz per bin
- Frequency Range: 0 - 22,050 Hz
- Display Bars: 64 or 128 (configurable)
```

### Real-time Data Processing Pipeline

```mermaid
flowchart LR
    A[Raw Audio Samples] --> B[Apply Window Function]
    B --> C[Convert to Complex Numbers]
    C --> D[Perform FFT]
    D --> E[Calculate Magnitude]
    E --> F[Logarithmic Scaling]
    F --> G[Bin Grouping]
    G --> H[UI Data Update]
```

## Testing Strategy

### Unit Testing Components

**AudioProcessor Tests:**
- Audio device initialization
- FFT calculation accuracy
- Window function application
- Data format conversion

**ViewModel Tests:**
- Command execution
- Property change notifications
- Data collection updates
- State management

**UI Tests:**
- Data binding validation
- Animation performance
- Visual element rendering
- User interaction handling

### Performance Testing
- Real-time audio processing latency
- UI rendering frame rate (target: 60 FPS)
- Memory usage monitoring
- CPU utilization optimization

### Integration Testing
- End-to-end audio capture to visualization
- Multiple audio device compatibility
- Long-running stability tests

## UI Architecture & Navigation

### Visual Layout Structure

```mermaid
graph TD
    MW[MainWindow 800x600] --> TitleBar[Title Bar]
    MW --> MainGrid[Main Grid]
    MainGrid --> SpectrumArea[Spectrum Display Area]
    MainGrid --> ControlArea[Control Panel]
    SpectrumArea --> SpectrumCanvas[Canvas for Bars]
    SpectrumArea --> ReflectionCanvas[Reflection Canvas]
    ControlArea --> ButtonStack[Button StackPanel]
    ControlArea --> SettingsStack[Settings StackPanel]
    ButtonStack --> StartBtn[Start Button]
    ButtonStack --> StopBtn[Stop Button]
    SettingsStack --> SensSlider[Sensitivity Slider]
```

### Spectrum Visualization Details

**Bar Rendering:**
- ItemsControl with horizontal StackPanel
- Rectangle elements with data-bound Height
- Orange gradient fill with glow effect
- Smooth height transitions with animations

**Reflection Effect:**
- Duplicate ItemsControl with ScaleTransform (ScaleY = -1)
- OpacityMask with LinearGradientBrush (top: 0.5, bottom: 0.0)
- Positioned below main spectrum with negative margin

### Responsive Design
- Minimum window size: 600x400
- Spectrum bars auto-scale with window width
- Control panel remains fixed height
- Reflection maintains proportional relationship