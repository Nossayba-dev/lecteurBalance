using System.Configuration;

namespace LecteurBalance.Properties;

/// <summary>
/// Provides access to user settings that persist between application launches.
/// </summary>
public sealed class Settings
{
    private static readonly Lazy<Settings> _instance = new(() => new Settings());

    public static Settings Default => _instance.Value;

    private Settings()
    {
    }

    /// <summary>
    /// Gets or sets the COM port name (e.g., "COM3").
    /// </summary>
    public string ComPort
    {
        get => GetSetting("ComPort", "COM3");
        set => SaveSetting("ComPort", value);
    }

    /// <summary>
    /// Gets or sets the baud rate for serial communication.
    /// </summary>
    public int BaudRate
    {
        get
        {
            var value = GetSetting("BaudRate", "9600");
            return int.TryParse(value, out var result) ? result : 9600;
        }
        set => SaveSetting("BaudRate", value.ToString());
    }

    /// <summary>
    /// Gets or sets the server URL for data transmission.
    /// </summary>
    public string ServerUrl
    {
        get => GetSetting("ServerUrl", "http://localhost:5000");
        set => SaveSetting("ServerUrl", value);
    }

    private static string GetSetting(string key, string defaultValue)
    {
        try
        {
            var value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error reading setting {key}: {ex.Message}");
            return defaultValue;
        }
    }

    /// <summary>
    /// Saves a setting to the configuration file.
    /// </summary>
    private static void SaveSetting(string key, string value)
    {
        try
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSettings = config.AppSettings.Settings;

            if (appSettings[key] is null)
            {
                appSettings.Add(key, value);
            }
            else
            {
                appSettings[key]!.Value = value;
            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving setting {key}: {ex.Message}");
        }
    }
}
