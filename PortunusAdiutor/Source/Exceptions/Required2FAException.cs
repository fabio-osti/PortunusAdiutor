namespace PortunusAdiutor.Exceptions;

/// <summary>
/// 	Represents error that occur when an user have 2FA
/// 	enabled, but no code was provided.
/// </summary>
public class Required2FAException : PortunusException
{
	/// <summary>
	/// 	Initializes the exception.
	/// </summary>
	public Required2FAException() 
		: base("2FA is required for this user.") { }
}