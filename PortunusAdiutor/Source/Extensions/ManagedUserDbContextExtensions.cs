using System.Security.Cryptography;

using PortunusAdiutor.Data;
using PortunusAdiutor.Helpers;
using PortunusAdiutor.Models;
using PortunusAdiutor.Static;


/// <summary>
/// 	Extensions on <see cref="ManagedUserDbContext{TUser}"/>.
/// </summary>
static public class ManagedUserDbContextExtensions
{
	/// <summary>
	/// 	Generates a <see cref="UserToken{TUser}"/> for an 
	/// 	<see cref="IManagedUser{TUser}"/> for an access of type 
	/// 	<paramref name="type"/> and saves it on the database.
	/// </summary>
	/// <param name="context"></param>
	///
	/// <param name="userId">
	/// 	Id of the <see cref="IManagedUser{TUser}"/>.
	/// </param>
	///
	/// <param name="type">
	/// 	Type of access granted by the returning TOKEN.
	/// </param>
	///
	/// <param name="token">
	/// 	The generated "X"-Digits Code
	/// </param>
	///
	/// <returns>
	/// 	The generated <see cref="UserToken{TUser}"/>.
	/// </returns>
	static public UserToken<TUser> GenAndSaveToken<TUser>(
		this ManagedUserDbContext<TUser> context,
		Guid userId,
		MessageType type,
		out string token
	)
	where TUser : class, IManagedUser<TUser>
	{
		token = RandomNumberGenerator.GetInt32(1000000).ToString("000000");

		var userToken = new UserToken<TUser>(userId, token, type);

		context.UserTokens.Add(userToken);
		context.SaveChanges();

		return userToken;
	}

	/// <summary>
	/// 	Consumes a sent message.
	/// </summary>
	/// 
	/// <param name="context">
	/// </param>
    /// 
	/// <param name="userId">
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
	static public UserResultStatus ConsumeToken<TUser>(
		this ManagedUserDbContext<TUser> context,
		Guid userId,
		string token,
		MessageType messageType,
		bool singleUse = true
	)
	where TUser : class, IManagedUser<TUser>
	{
		var userToken = context.UserTokens.Find(userId, token, messageType);

		if (
			userToken is null
			|| userToken.ExpiresOn < DateTime.UtcNow
		) {
			return UserResultStatus.InvalidToken;
		}

		if (singleUse) {
			context.UserTokens.Remove(userToken);
			context.SaveChanges();
		}

		return UserResultStatus.Ok;
	}
}