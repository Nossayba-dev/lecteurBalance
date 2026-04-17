namespace LecteurBalance.Models;

/// <summary>
/// Event arguments for weight data received from the scale.
/// </summary>
public class WeightReceivedEventArgs : EventArgs
{
    public WeightReceivedEventArgs(decimal weight)
    {
        Weight = weight;
        Timestamp = DateTime.Now;
    }

    /// <summary>
    /// Gets the weight value received from the scale.
    /// </summary>
    public decimal Weight { get; }

    /// <summary>
    /// Gets the timestamp when the weight was received.
    /// </summary>
    public DateTime Timestamp { get; }
}
