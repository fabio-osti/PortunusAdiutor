using System.Security.Cryptography;
using PortunusAdiutor.Data;
using PortunusAdiutor.Helpers;
using PortunusAdiutor.Models.User;
using PortunusAdiutor.Models.Code;
using PortunusAdiutor.Services.MessagePoster;

/// <summary>
///     Extensions on <see cref="ManagedUserDbContext{TUser}" />.
/// </summary>
public static class ManagedUserDbContextExtensions
{
	/// <summary>
	///     Generates a <see cref="UserCode{TUser}" /> for an
	///     <see cref="IManagedUser{TUser}" /> for an access of type
	///     <paramref name="type" /> and saves it on the database.
	/// </summary>
	/// <param name="context"></param>
	/// 
	/// <param name="userId">
	///     Id of the <see cref="IManagedUser{TUser}" />.
	/// </param>
	/// 
	/// <param name="type">
	///     Type of access granted by the returning TOKEN.
	/// </param>
	/// 
	/// <param name="token">
	///     The generated "X"-Digits Code
	/// </param>
	/// 
	/// <returns>
	///     The generated <see cref="UserCode{TUser}" />.
	/// </returns>
	public static UserCode<TUser> GenAndSaveToken<TUser>(
		this ManagedUserDbContext<TUser> context,
		Guid userId,
		CodeType type,
		out string token
	) where TUser : class, IManagedUser<TUser>
	{
		do {
			token = RandomNumberGenerator.GetInt32(1000000).ToString("000000");
		} while (context.UsersCodes.Find(userId, token, type) is not null);

		var userCode = new UserCode<TUser>(userId, token, type);

		context.UsersCodes.Add(userCode);
		context.SaveChanges();

		return userCode;
	}

	/// <summary>
	///     Consumes a sent message.
	/// </summary>
	/// 
	/// <param name="context">
    /// 	Database context.
	/// </param>
	/// 
	/// <param name="userId">
    /// 	Id of the user.
	/// </param>
	/// 
	/// <param name="token">
	///     The access key sent by the message.
	/// </param>
	/// 
	/// <param name="messageType">
	///     The type of message that was sent.
	/// </param>
	/// 
	/// <param name="singleUse">
	///     If the token should be deleted after use.
	/// </param>
	/// 
	/// <returns>
	///     The key of the user to whom the token gives access to.
	/// </returns>
	public static bool ConsumeToken<TUser>(
		this ManagedUserDbContext<TUser> context,
		Guid userId,
		string token,
		CodeType messageType,
		bool singleUse = true
	) where TUser : class, IManagedUser<TUser>
	{
		var userToken = context.UsersCodes.Find(userId, token, messageType);

		if (userToken is null || userToken.ExpiresOn < DateTime.UtcNow)
			return false;

		if (singleUse) {
			context.UsersCodes.Remove(userToken);
			context.SaveChanges();
		}

		return true;
	}
}