using System.Diagnostics.CodeAnalysis;
using PortunusAdiutor.Models;

namespace PortunusAdiutor.Exceptions;

/// <summary>
/// 	Represents error that occur when a 
/// 	token is not found for the user.
/// </summary>
public class InvalidTokenException : PortunusException
{
	/// <summary>
	/// 	Initializes the exception.
	/// </summary>
	public InvalidTokenException() 
		: base("Validation for this token failed.") { }
}