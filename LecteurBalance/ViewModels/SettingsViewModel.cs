using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LecteurBalance.Properties;

namespace LecteurBalance.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private string comPort = string.Empty;

    [ObservableProperty]
    private int baudRate;

    [ObservableProperty]
    private string serverUrl = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public SettingsViewModel()
    {
        LoadSettings();
    }

    /// <summary>
    /// Loads settings from the configuration file.
    /// </summary>
    private void LoadSettings()
    {
        ComPort = Settings.Default.ComPort ?? string.Empty;
        BaudRate = Settings.Default.BaudRate;
        ServerUrl = Settings.Default.ServerUrl ?? string.Empty;
    }

    /// <summary>
    /// Command to save settings to the configuration file.
    /// </summary>
    [RelayCommand]
    private void Save()
    {
        try
        {
            Settings.Default.ComPort = ComPort ?? string.Empty;
            Settings.Default.BaudRate = BaudRate;
            Settings.Default.ServerUrl = ServerUrl ?? string.Empty;

            StatusMessage = "Settings saved successfully!";
            System.Diagnostics.Debug.WriteLine("Settings saved.");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving settings: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Command to reload settings from the configuration file (discard unsaved changes).
    /// </summary>
    [RelayCommand]
    private void Reset()
    {
        LoadSettings();
        StatusMessage = "Settings reset to saved values.";
    }
}

