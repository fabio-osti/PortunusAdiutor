using System.Security.Cryptography;
using System.Text;

using Microsoft.IdentityModel.Tokens;
using PortunusAdiutor.Static;

namespace PortunusAdiutor.Models;

/// <summary>
/// 	Class representing a single use password for special access.
/// </summary>
///
/// <typeparam name="TUser">
/// 	Type of the user.
/// </typeparam>
///
/// <typeparam name="TKey">
/// 	Type of the user primary key.
/// </typeparam>
public class SingleUseToken<TUser, TKey>
where TKey : IEquatable<TKey>
{
	/// <summary>
	/// 	Gets a unique token representing the <paramref name="userId"/>, 
	/// 	<paramref name="xdc"/> and <paramref name="type"/>.
	/// </summary>
	///
	/// <param name="userId">
	/// 	The primary key of the user that the token will authenticate.
	/// </param>
	///
	/// <param name="xdc">
	/// 	An 'X' digits code.
	/// </param>
	///
	/// <param name="type">
	/// 	The string representation of the <see cref="MessageType"/> 
	/// 	that will include this token.
	/// </param>
	///
	/// <returns>
	/// 	The token.
	/// </returns>
	public static string GetTokenFrom(TKey userId, string xdc, string type)
	{
		var concat = Encoding.UTF8.GetBytes(userId.ToString() + type + xdc);
		return Base64UrlEncoder.Encode(SHA512.HashData(concat));
	}

	/// <summary>
	/// 	Initializes an instance of the class.
	/// </summary>
	///
	/// <param name="userId">
	/// 	The primary key of the user that the token will authenticate.
	/// </param>
	///
	/// <param name="xdc">
	/// 	A 'X' digits code.
	/// </param>
	///
	/// <param name="type">
	/// 	The string representation of the <see cref="MessageType"/> 
	/// 	that will include this token.
	/// </param>
	public SingleUseToken(TKey userId, string xdc, string type)
	{
		Token = GetTokenFrom(userId, xdc, type);
		UserId = userId;
		Type = type;
		ExpiresOn = DateTime.UtcNow.AddMinutes(15);
	}

	private SingleUseToken(TKey userId, string token, string type, DateTime expiresOn)
	{
		UserId = userId;
		Token = token;
		Type = type;
		ExpiresOn = expiresOn;
	}

	/// <summary>
	///  	The user this <see cref="SingleUseToken{TUser, TKey}"/> 
	///  	gives access to.
	/// </summary>
	public TUser? User { get; init; }

	/// <summary>
	/// 	The primary key of the user this 
	/// 	<see cref="SingleUseToken{TUser, TKey}"/> gives access to.
	/// </summary>
	public TKey UserId { get; init; }

	/// <summary>
	/// 	The one use password.
	/// </summary>
	public string Token { get; init; }

	/// <summary>
	/// 	The type of access given to
	/// 	by this <see cref="SingleUseToken{TUser, TKey}"/>.
	/// </summary>
	public string Type { get; init; }

	/// <summary>
	///		Expiration <see cref="DateTime"/> 
	///		of this <see cref="SingleUseToken{TUser, TKey}"/>.
	/// </summary>
	public DateTime ExpiresOn { get; init; }
}