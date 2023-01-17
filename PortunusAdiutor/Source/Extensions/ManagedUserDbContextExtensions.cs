using System.Security.Cryptography;

using PortunusAdiutor.Data;
using PortunusAdiutor.Exceptions;
using PortunusAdiutor.Models;
using PortunusAdiutor.Static;

/// <summary>
/// 	Extensions on <see cref="ManagedUserDbContext{TUser, TKey}"/>.
/// </summary>
static public class ManagedUserDbContextExtensions
{
	/// <summary>
	/// 	Generates a <see cref="SingleUseToken{TUser, TKey}"/> for an 
	/// 	<see cref="IManagedUser{TUser, TKey}"/> for an access of type 
	/// 	<paramref name="type"/> and saves it on the database.
	/// </summary>
	/// <param name="context"></param>
	///
	/// <param name="userId">
	/// 	Id of the <see cref="IManagedUser{TUser, TKey}"/>.
	/// </param>
	///
	/// <param name="type">
	/// 	Type of access granted by the the returning SUT.
	/// </param>
	///
	/// <param name="xdc">
	/// 	The generated "X"-Digits Code
	/// </param>
	///
	/// <returns>
	/// 	The generated <see cref="SingleUseToken{TUser, TKey}"/>.
	/// </returns>
	static public SingleUseToken<TUser, TKey> GenAndSaveToken<TUser, TKey>(
		this ManagedUserDbContext<TUser, TKey> context,
		TKey userId,
		MessageType type,
		out string xdc
	)
	where TUser : class, IManagedUser<TUser, TKey>
	where TKey : IEquatable<TKey>
	{
		xdc = RandomNumberGenerator.GetInt32(1000000).ToString("000000");

		var userSut = new SingleUseToken<TUser, TKey>(userId, xdc, type.ToTypeString());

		context.SingleUseTokens.Add(userSut);
		context.SaveChanges();

		return userSut;
	}

	/// <summary>
	/// 	Consumes a sent message.
	/// </summary>
	/// 
	/// <param name="context">
	/// </param>
	///
	/// <param name="token">
	/// 	The access key sent by the message.
	/// </param>
	///
	/// <param name="messageType">
	/// 	The type of message that was sent.
	/// </param>
	/// 
	/// <param name="singleUse">
	/// 	If the token should be deleted after use.
	/// </param>
	///
	/// <returns>
	/// 	The key of the user to whom the token gives access to.
	/// </returns>
	static public TKey ConsumeToken<TUser, TKey>(
		this ManagedUserDbContext<TUser, TKey> context,
		string token,
		MessageType messageType,
		bool singleUse = true
	)
	where TUser : class, IManagedUser<TUser, TKey>
	where TKey : IEquatable<TKey>
	{
		var SingleuseToken = context.SingleUseTokens.Find(token);

		if (SingleuseToken is null) {
			throw new TokenNotFoundException();
		}

		var type = messageType.ToTypeString();
		if (SingleuseToken.ExpiresOn < DateTime.UtcNow || SingleuseToken.Type != type) {
			throw new InvalidPasswordException();
		}

		if (singleUse) {
			context.SingleUseTokens.Remove(SingleuseToken);
		}
		context.SaveChanges();

		return SingleuseToken.UserId;
	}
}