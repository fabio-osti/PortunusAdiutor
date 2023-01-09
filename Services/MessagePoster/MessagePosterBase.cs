using System.Security.Cryptography;

using PortunusAdiutor.Data;
using PortunusAdiutor.Exceptions;
using PortunusAdiutor.Models;
using PortunusAdiutor.Static;

namespace PortunusAdiutor.Services.MessagePoster;

/// <summary>
/// 	Implements common methods for SUT generation and consumption.
/// </summary>
///
/// <typeparam name="TContext">
///  	Type of the DbContext.
/// </typeparam>
///
/// <typeparam name="TUser">
/// 	Type of the user.
/// </typeparam>
///
/// <typeparam name="TKey">
/// 	Type of the user primary key.
/// </typeparam>
public class MessagePosterBase<TContext, TUser, TKey>
where TContext : ManagedUserDbContext<TUser, TKey>
where TUser : class, IManagedUser<TUser, TKey>
where TKey : IEquatable<TKey>
{
	private readonly TContext _context;

	/// <summary>
	/// 	Initializes an instance of the class.
	/// </summary>
	///
	/// <param name="context">
	/// 	Database context</param>
	public MessagePosterBase(TContext context)
	{
		_context = context;
	}

	/// <summary>
	/// 	Generates a <see cref="SingleUseToken{TUser, TKey}"/> for an 
	/// 	<see cref="IManagedUser{TUser, TKey}"/> for an access of type 
	/// 	<paramref name="type"/> and saves it on the database.
	/// </summary>
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
	/// </param>
	///
	/// <returns>
	/// 	The generated <see cref="SingleUseToken{TUser, TKey}"/>.
	/// </returns>
	protected SingleUseToken<TUser, TKey> GenAndSave(
		TKey userId,
		MessageType type,
		out string xdc
	)
	{
		xdc = RandomNumberGenerator.GetInt32(1000000).ToString("000000");

		var userSut = new SingleUseToken<TUser, TKey>(userId, xdc, type.ToTypeString());

		_context.SingleUseTokens.Add(userSut);
		_context.SaveChanges();

		return userSut;
	}

	/// <summary>
	/// 	Consumes a sent message.
	/// </summary>
	///
	/// <param name="token">
	/// 	The access key sent by the message.
	/// </param>
	///
	/// <param name="messageType">
	/// 	The type of message that was sent.
	/// </param>
	///
	/// <returns>
	/// 	The key of the user to whom the token gives access to.
	/// </returns>
	public TKey ConsumeSut(
		string token,
		MessageType messageType
	)
	{
		var userSut =
			_context.SingleUseTokens.Find(token);

		if (userSut is null) {
			throw new TokenNotFoundException();
		}

		var type = messageType.ToTypeString();
		if (userSut.ExpiresOn < DateTime.UtcNow || userSut.Type != type) {
			throw new InvalidPasswordException();
		}

		_context.SingleUseTokens.Remove(userSut);
		_context.SaveChanges();

		return userSut.UserId;
	}
}
