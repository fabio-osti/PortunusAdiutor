using System.Diagnostics.CodeAnalysis;
using PortunusAdiutor.Exceptions;
using PortunusAdiutor.Models;

namespace PortunusAdiutor.Extensions;

/// <summary>
/// 	Extensions to throw when 
/// 	<see cref="IManagedUser{TUser}"/>
/// 	or
/// 	<see cref="SingleUseToken{TUser}"/>
/// 	are not found (null)
/// </summary>
static public class NotFoundExtensions
{
	/// <summary>
	/// 	Checks if <paramref name="user"/> is null.
	/// </summary>
	///
	/// <typeparam name="TUser">
	/// 	Type of the user.
	/// </typeparam>
	///
	///
	/// <param name="user">
	/// 	User to be checked if null.
	/// </param>
	///
	/// <returns>
	/// 	Not null asserted <paramref name="user"/>.
	/// </returns>
	///
	/// <exception cref="UserNotFoundException">
	/// 	Throws if <paramref name="user"/> is null.
	/// </exception>
	public static TUser ThrowIfUserNull<TUser>([NotNull] this TUser? user)
	where TUser : class, IManagedUser<TUser>
	
	{
		return user ?? throw new UserNotFoundException();
	}

	/// <summary>
	/// 	Checks if <paramref name="token"/> is null.
	/// </summary>
	///
	/// <typeparam name="TUser">
	/// 	Type of the user.
	/// </typeparam>
	///
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
	public static SingleUseToken<TUser> ThrowIfTokenNull<TUser>(
		[NotNull] this SingleUseToken<TUser>? token
	)
	where TUser : class, IManagedUser<TUser>
	
	{
		return token ?? throw new TokenNotFoundException();
	}
}