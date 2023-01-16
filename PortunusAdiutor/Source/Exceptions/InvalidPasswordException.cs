namespace PortunusAdiutor.Exceptions;

/// <summary>
/// 	Represents error that occur when an user password is not valid.
/// </summary>
public class InvalidPasswordException : PortunusException
{
	/// <summary>
	/// 	Initializes the exception.
	/// </summary>
	public InvalidPasswordException() : 
		base("Validation failed for this user.") { }
}