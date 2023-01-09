namespace PortunusAdiutor.Exceptions;

/// <summary>
/// 	Represents error that occur when an user tries to
/// 	confirm an already confirmed email.
/// </summary>
public class EmailAlreadyConfirmedException : PortunusException
{
	/// <summary>
	/// 	Initializes the exception.
	/// </summary>
	public EmailAlreadyConfirmedException() : 
		base("Email already confirmed for this user.") { }
}