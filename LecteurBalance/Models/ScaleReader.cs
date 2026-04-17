using System.IO.Ports;
using System.Text.RegularExpressions;

namespace LecteurBalance.Models;

public class ScaleReader : IDisposable
{
    private SerialPort? _serialPort;
    private bool _isConnected;

    /// <summary>
    /// Raised when new weight data is received from the scale.
    /// </summary>
    public event EventHandler<WeightReceivedEventArgs>? WeightReceived;

    public ScaleReader()
    {
        _isConnected = false;
    }

    /// <summary>
    /// Opens a connection to the weight scale on the specified COM port.
    /// </summary>
    /// <param name="comPort">The COM port name (e.g., "COM1", "COM3")</param>
    /// <param name="baudRate">The baud rate for communication (e.g., 9600, 19200)</param>
    /// <exception cref="InvalidOperationException">Thrown if connection fails</exception>
    public void OpenConnection(string comPort, int baudRate)
    {
        try
        {
            if (_isConnected)
            {
                CloseConnection();
            }

            _serialPort = new SerialPort(comPort, baudRate, Parity.None, 8, StopBits.One)
            {
                Handshake = Handshake.None,
                ReadTimeout = 1000,
                WriteTimeout = 1000
            };

            _serialPort.DataReceived += SerialPort_DataReceived;
            _serialPort.Open();
            _isConnected = true;
        }
        catch (Exception ex)
        {
            _isConnected = false;
            throw new InvalidOperationException($"Failed to open connection on {comPort} at {baudRate} baud.", ex);
        }
    }

    /// <summary>
    /// Closes the serial port connection.
    /// </summary>
    public void CloseConnection()
    {
        if (_serialPort != null && _serialPort.IsOpen)
        {
            _serialPort.Close();
            _isConnected = false;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the serial port is currently connected.
    /// </summary>
    public bool IsConnected => _isConnected && _serialPort?.IsOpen == true;

    /// <summary>
    /// Handles incoming data from the serial port.
    /// </summary>
    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (_serialPort == null || !_serialPort.IsOpen)
            return;

        try
        {
            string rawData = _serialPort.ReadExisting();
            decimal? weight = ParseWeight(rawData);

            if (weight.HasValue)
            {
                OnWeightReceived(weight.Value);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error reading from serial port: {ex.Message}");
        }
    }

    /// <summary>
    /// Parses ASCII data from the scale to extract the weight value.
    /// Removes non-numeric characters (except decimal point) and converts to decimal.
    /// </summary>
    /// <param name="rawData">The raw ASCII data from the scale</param>
    /// <returns>The parsed weight as a decimal, or null if parsing fails</returns>
    private decimal? ParseWeight(string rawData)
    {
        if (string.IsNullOrWhiteSpace(rawData))
            return null;

        // Remove whitespace and non-printable characters
        string cleaned = Regex.Replace(rawData, @"[\r\n\t\x00-\x1F]", "");

        // Extract only digits and decimal points
        string weightString = Regex.Replace(cleaned, @"[^\d.,]", "");

        if (string.IsNullOrWhiteSpace(weightString))
            return null;

        // Replace comma with period for decimal parsing (supports different locales)
        weightString = weightString.Replace(",", ".");

        // Remove duplicate decimal points, keeping only the last one
        int lastDotIndex = weightString.LastIndexOf('.');
        if (lastDotIndex != -1)
        {
            weightString = weightString.Substring(0, lastDotIndex).Replace(".", "") + weightString.Substring(lastDotIndex);
        }

        if (decimal.TryParse(weightString, out decimal weight))
        {
            return weight;
        }

        return null;
    }

    /// <summary>
    /// Raises the WeightReceived event.
    /// </summary>
    protected virtual void OnWeightReceived(decimal weight)
    {
        WeightReceived?.Invoke(this, new WeightReceivedEventArgs(weight));
    }

    public void Dispose()
    {
        CloseConnection();
        _serialPort?.Dispose();
        GC.SuppressFinalize(this);
    }
}
