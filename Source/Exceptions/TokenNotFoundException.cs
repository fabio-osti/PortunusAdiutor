using System.Diagnostics.CodeAnalysis;
using PortunusAdiutor.Models;

namespace PortunusAdiutor.Exceptions;

/// <summary>
/// 	Represents error that occur when a 
/// 	token is not found.
/// </summary>
public class TokenNotFoundException : PortunusException
{
	/// <summary>
	/// 	Initializes the exception.
	/// </summary>
	public TokenNotFoundException() : base("Token not found.") { }
}