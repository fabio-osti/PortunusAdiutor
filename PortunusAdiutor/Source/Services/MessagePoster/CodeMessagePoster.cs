using MailKit.Net.Smtp;
using MimeKit;

using PortunusAdiutor.Data;
using PortunusAdiutor.Models;
using PortunusAdiutor.Static;

namespace PortunusAdiutor.Services.MessagePoster;

/// <summary>
/// 	Implementation of <see cref="IMessagePoster{TUser, TKey}"/> 
/// 	with access code.
/// </summary>
///
/// <typeparam name="TContext">
/// 	Type of the DbContext.
/// </typeparam>
///
/// <typeparam name="TUser">
/// 	Type of the user.
/// </typeparam>
///
/// <typeparam name="TKey">
/// 	Type of the user primary key.
/// </typeparam>
public class CodeMessagePoster<TContext, TUser, TKey> : IMessagePoster<TUser, TKey>
where TContext : ManagedUserDbContext<TUser, TKey>
where TUser : class, IManagedUser<TUser, TKey>
where TKey : IEquatable<TKey>
{
	private readonly CodeMessagePosterParams _posterParams;
	private readonly TContext _context;

	/// <summary>
	/// 	Initializes an instance of the class.
	/// </summary>
	///
	/// <param name="posterParams">
	/// 	Parameters for sending messages.
	/// </param>
	///
	/// <param name="context">
	/// 	Database context.
	/// </param>
	public CodeMessagePoster(
		CodeMessagePosterParams posterParams, 
		TContext context
	)
	{
		_posterParams = posterParams;
		_context = context;
	}

	/// <inheritdoc/>
	public void SendEmailConfirmationMessage(TUser user)
	{
		ArgumentException.ThrowIfNullOrEmpty(user.Email);
		// Generates SUT
		var otp = _context.GenAndSaveSingleUseToken(
			user.Id, 
			MessageType.EmailConfirmation, 
			out var xdc
		);
		// Builds and sends message
		var message = _posterParams.EmailConfirmationMessageBuilder(
			user.Email,
			xdc
		);
		SendMessage(message);
	}

	/// <inheritdoc/>
	public void SendPasswordRedefinitionMessage(TUser user)
	{
		ArgumentException.ThrowIfNullOrEmpty(user.Email);
		// Generates SUT
		var otp = _context.GenAndSaveSingleUseToken(
			user.Id, 
			MessageType.PasswordRedefinition, 
			out var xdc
		);
		// Builds and sends message
		var message = _posterParams.PasswordRedefinitionMessageBuilder(
			user.Email,
			xdc
		);
		SendMessage(message);
	}

	private void SendMessage(MimeMessage message)
	{
		using var client = new SmtpClient();
		client.Connect(_posterParams.SmtpUri);
		client.Authenticate(_posterParams.SmtpCredentials);
		client.Send(message);
		client.Disconnect(true);
	}
}
