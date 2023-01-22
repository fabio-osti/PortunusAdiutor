using PortunusAdiutor.Models;

namespace PortunusAdiutor.Services.MessagePoster;

/// <summary>
///     Defines all necessary methods for message posting.
/// </summary>
/// 
/// <typeparam name="TUser">
///     Type of the user.
/// </typeparam>
public interface IMessagePoster<TUser> where TUser : class, IManagedUser<TUser>
{
	/// <summary>
	///     Sends message asking for the confirmation of the
	///     <see cref="IManagedUser{TUser}.Email" />
	///     from <paramref name="user" />.
	/// </summary>
	/// 
	/// <param name="user">
	///     Receiver of the message.
	/// </param>
	void SendEmailConfirmationMessage(TUser user);

	/// <summary>
	///     Sends message asking for the redefinition of the
	///     <see cref="IManagedUser{TUser}.PasswordHash" />
	///     from <paramref name="user" />.
	/// </summary>
	/// 
	/// <param name="user">
	///     Receiver of the message.
	/// </param>
	void SendPasswordRedefinitionMessage(TUser user);

	/// <summary>
	///     Sends message asking for a 2FA code for
	///     <paramref name="user" />
	/// </summary>
	/// 
	/// <param name="user">
	///     Receiver of the message.
	/// </param>
	void SendTwoFactorAuthenticationMessage(TUser user);
}