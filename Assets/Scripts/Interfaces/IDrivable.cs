public interface IDrivable
{
    /// <summary>
    /// Steering input: -1 = full left, 1 = full right.
    /// </summary>
    float DriveX { get; set; }

    /// <summary>
    /// Throttle input: -1 = full reverse, 1 = full forward.
    /// </summary>
    float DriveY { get; set; }
}