using PortunusAdiutor.Models;
using PortunusAdiutor.Static;

namespace PortunusAdiutor.Services.MessagePoster;

/// <summary>
/// 	Defines all necessary methods for message posting.
/// </summary>
///
/// <typeparam name="TUser">
/// 	Type of the user.
/// </typeparam>
///
/// <typeparam name="TKey">
/// 	Type of the user primary key.
/// </typeparam>
public interface IMessagePoster<TUser, TKey>
where TUser : class, IManagedUser<TUser, TKey>
where TKey : IEquatable<TKey>
{
	/// <summary>
	/// 	Sends message asking for the confirmation of the 
	/// 	<see cref="IManagedUser{TUser, TKey}.Email"/> 
	/// 	from <paramref name="user"/>.
	/// </summary>
	///
	/// <param name="user">
	/// 	Receiver of the message.
	/// </param>
	void SendEmailConfirmationMessage(TUser user);

	/// <summary>
	/// 	Sends message asking for the redefinition of the 
	/// 	<see cref="IManagedUser{TUser, TKey}.PasswordHash"/> 
	/// 	from <paramref name="user"/>.
	/// </summary>
	///
	/// <param name="user">
	/// 	Receiver of the message.
	/// </param>
	void SendPasswordRedefinitionMessage(TUser user);
}