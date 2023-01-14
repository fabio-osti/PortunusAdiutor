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

	/// <summary>
	/// 	Checks if <paramref name="token"/> is null.
	/// </summary>
	///
	/// <typeparam name="TUser">
	/// 	Type of the user.
	/// </typeparam>
	///
	/// <typeparam name="TKey">
	/// 	Type of the user primary key.
	/// </typeparam>
	///
	/// <param name="token">
	/// 	Token to be checked if null.
	/// </param>
	///
	/// <returns>
	/// 	Not null asserted <paramref name="token"/>.
	/// </returns>
	///
	/// <exception cref="UserNotFoundException">
	/// 	Throws if <paramref name="token"/> is null.
	/// </exception>
	public static SingleUseToken<TUser, TKey> ThrowIfUserNull<TUser, TKey>(
		SingleUseToken<TUser, TKey>? token
	)
	where TUser : class, IManagedUser<TUser, TKey>
	where TKey : IEquatable<TKey>
	{
		return token ?? throw new TokenNotFoundException();
	}
}