using System.Diagnostics.CodeAnalysis;
using PortunusAdiutor.Models;

namespace PortunusAdiutor.Exceptions;

/// <summary>
/// 	Represents error that occur when an user is not found.
/// </summary>
public class UserNotFoundException : PortunusException
{
	/// <summary>
	/// 	Initializes the exception.
	/// </summary>
	public UserNotFoundException() : base("User not found.") { }
}