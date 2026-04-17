using System.Collections.ObjectModel;
using System.IO.Ports;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LecteurBalance.Models;
using LecteurBalance.Properties;

namespace LecteurBalance.ViewModels;

/// <summary>
/// Main view model for the application. Manages the connection to the weight scale
/// and provides commands for user interactions.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private ScaleReader? _scaleReader;
    private string _currentWeight = "0.00";
    private string _statusMessage = "Ready";
    private string _selectedPort = string.Empty;
    private ObservableCollection<string> _availablePorts = new ObservableCollection<string>();

    /// <summary>
    /// The currently displayed weight value from the scale.
    /// </summary>
    public string CurrentWeight
    {
        get => _currentWeight;
        set => SetProperty(ref _currentWeight, value);
    }

    /// <summary>
    /// Status message to display in the UI (e.g., "Connected", "Disconnected", error messages).
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>
    /// The currently selected COM port for connection.
    /// </summary>
    public string SelectedPort
    {
        get => _selectedPort;
        set => SetProperty(ref _selectedPort, value);
    }

    /// <summary>
    /// Collection of available COM ports on the system.
    /// </summary>
    public ObservableCollection<string> AvailablePorts
    {
        get => _availablePorts;
        set => SetProperty(ref _availablePorts, value);
    }

    public MainViewModel()
    {
        _scaleReader = new ScaleReader();
        _scaleReader.WeightReceived += ScaleReader_WeightReceived;

        // Initialize available ports and load saved settings
        _availablePorts = new ObservableCollection<string>(SerialPort.GetPortNames());
        LoadSavedSettings();
    }

    /// <summary>
    /// Loads the saved COM port from the application settings.
    /// </summary>
    private void LoadSavedSettings()
    {
        SelectedPort = Settings.Default.ComPort;
    }

    /// <summary>
    /// Command to connect to the weight scale.
    /// Uses the selected port and baud rate from settings.
    /// </summary>
    [RelayCommand]
    private void Connect()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(SelectedPort))
            {
                StatusMessage = "Please select a COM port first.";
                return;
            }

            int baudRate = Settings.Default.BaudRate;
            _scaleReader?.OpenConnection(SelectedPort, baudRate);
            StatusMessage = $"Connected to {SelectedPort} at {baudRate} baud";
            System.Diagnostics.Debug.WriteLine($"Connected to {SelectedPort} at {baudRate} baud");
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = $"Connection failed: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Connection error: {ex.Message}");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Command to disconnect from the weight scale.
    /// </summary>
    [RelayCommand]
    private void Disconnect()
    {
        try
        {
            _scaleReader?.CloseConnection();
            StatusMessage = "Disconnected";
            CurrentWeight = "0.00";
            System.Diagnostics.Debug.WriteLine("Disconnected from scale");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error disconnecting: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Disconnect error: {ex.Message}");
        }
    }

    /// <summary>
    /// Command to refresh the list of available COM ports.
    /// Useful if a port is connected/disconnected after the app starts.
    /// </summary>
    [RelayCommand]
    private void RefreshPorts()
    {
        try
        {
            AvailablePorts.Clear();
            foreach (var port in SerialPort.GetPortNames())
            {
                AvailablePorts.Add(port);
            }
            StatusMessage = "Available ports refreshed";
            System.Diagnostics.Debug.WriteLine("Ports refreshed");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error refreshing ports: {ex.Message}";
        }
    }

    /// <summary>
    /// Command to open the settings window for configuration.
    /// </summary>
    [RelayCommand]
    private void OpenSettings()
    {
        try
        {
            var settingsWindow = new LecteurBalance.Views.SettingsWindow();
            settingsWindow.ShowDialog();
            
            // Reload saved settings in case they were changed
            LoadSavedSettings();
            StatusMessage = "Settings updated";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening settings: {ex.Message}";
        }
    }

    /// <summary>
    /// Handles weight data received from the scale reader and updates the CurrentWeight property.
    /// </summary>
    private void ScaleReader_WeightReceived(object? sender, WeightReceivedEventArgs e)
    {
        CurrentWeight = $"{e.Weight:F2}";
    }
}
