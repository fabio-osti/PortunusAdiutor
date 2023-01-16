using System.Diagnostics.CodeAnalysis;
using PortunusAdiutor.Exceptions;
using PortunusAdiutor.Models;

namespace PortunusAdiutor.Extensions;

/// <summary>
/// 	Extensions to throw when 
/// 	<see cref="IManagedUser{TUser, TKey}"/>
/// 	or
/// 	<see cref="SingleUseToken{TUser, TKey}"/>
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
	/// <typeparam name="TKey">
	/// 	Type of the user primary key.
	/// </typeparam>
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
	public static TUser ThrowIfUserNull<TUser, TKey>([NotNull] this TUser? user)
	where TUser : class, IManagedUser<TUser, TKey>
	where TKey : IEquatable<TKey>
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
	public static SingleUseToken<TUser, TKey> ThrowIfTokenNull<TUser, TKey>(
		[NotNull] this SingleUseToken<TUser, TKey>? token
	)
	where TUser : class, IManagedUser<TUser, TKey>
	where TKey : IEquatable<TKey>
	{
		return token ?? throw new TokenNotFoundException();
	}
}