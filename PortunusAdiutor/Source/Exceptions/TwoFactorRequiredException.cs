namespace PortunusAdiutor.Exceptions;

/// <summary>
/// 	Represents error that occur when an user have 2FA
/// 	enabled, but no code was provided.
/// </summary>
public class TwoFactorRequiredException : PortunusException
{
	/// <summary>
	/// 	Initializes the exception.
	/// </summary>
	public TwoFactorRequiredException() 
		: base("Two-Factor Authentication is required for this user.") { }
}