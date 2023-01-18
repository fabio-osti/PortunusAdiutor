using MailKit.Net.Smtp;
using MimeKit;

using PortunusAdiutor.Data;
using PortunusAdiutor.Models;
using PortunusAdiutor.Static;

namespace PortunusAdiutor.Services.MessagePoster;

/// <summary>
/// 	Implementation of <see cref="IMessagePoster{TUser}"/> 
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
public class MessagePoster<TContext, TUser> : IMessagePoster<TUser>
where TContext : ManagedUserDbContext<TUser>
where TUser : class, IManagedUser<TUser>
{
	private readonly MessagePosterParams _posterParams;
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
	public MessagePoster(
		MessagePosterParams posterParams, 
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
		var otp = _context.GenAndSaveToken(
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
		var otp = _context.GenAndSaveToken(
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
