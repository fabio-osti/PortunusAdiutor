using MailKit.Net.Smtp;
using MimeKit;
using PortunusAdiutor.Data;
using PortunusAdiutor.Models;
using PortunusAdiutor.Models.Code;

namespace PortunusAdiutor.Services.MessagePoster;

/// <summary>
///     Implementation of <see cref="IMessagePoster{TUser}" />
///     with access code.
/// </summary>
/// 
/// <typeparam name="TContext">
///     Type of the DbContext.
/// </typeparam>
/// 
/// <typeparam name="TUser">
///     Type of the user.
/// </typeparam>
public class MessagePoster<TContext, TUser> : IMessagePoster<TUser>
	where TContext : ManagedUserDbContext<TUser>
	where TUser : class, IManagedUser<TUser>
{
	/// <summary>
	///     Initializes an instance of the class.
	/// </summary>
	/// 
	/// <param name="posterParams">
	///     Parameters for sending messages.
	/// </param>
	/// 
	/// <param name="context">
	///     Database context.
	/// </param>
	public MessagePoster(
		MessagePosterParams posterParams,
		TContext context
	)
	{
		_posterParams = posterParams;
		_context = context;
	}

	private readonly TContext _context;
	private readonly MessagePosterParams _posterParams;

	/// <inheritdoc />
	public void SendEmailConfirmationMessage(TUser user)
	{
		// Generates TOKEN
		_context.GenAndSaveToken(
			user.Id,
			CodeType.EmailConfirmation,
			out var token
		);

		// Builds and sends message
		var message = _posterParams.EmailConfirmationMessageBuilder(
			user.Email,
			token
		);

		SendMessage(message);
	}

	/// <inheritdoc />
	public void SendPasswordRedefinitionMessage(TUser user)
	{
		// Generates TOKEN
		_context.GenAndSaveToken(
			user.Id,
			CodeType.PasswordRedefinition,
			out var token
		);

		// Builds and sends message
		var message = _posterParams.PasswordRedefinitionMessageBuilder(
			user.Email,
			token
		);

		SendMessage(message);
	}

	/// <inheritdoc />
	public void SendTwoFactorAuthenticationMessage(TUser user)
	{
		// Generates TOKEN
		_context.GenAndSaveToken(
			user.Id,
			CodeType.TwoFactorAuthentication,
			out var token
		);

		// Builds and sends message
		var message = _posterParams.TwoFactorAuthenticationMessageBuilder(
			user.Email,
			token
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