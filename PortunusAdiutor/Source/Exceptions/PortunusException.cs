namespace PortunusAdiutor.Exceptions;

/// <summary>
///     Represents any error that may occur within this lib.
/// </summary>
public class PortunusException : Exception
{
	/// <summary>
	///     Initializes the exception.
	/// </summary>
	public PortunusException(string shortMessage)
	{
		ShortMessage = shortMessage;
	}

	/// <summary>
	///     Gets or sets a short message describing the problem.
	/// </summary>
	public string ShortMessage { get; protected set; }
}