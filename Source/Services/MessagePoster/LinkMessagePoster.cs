using MailKit.Net.Smtp;

using MimeKit;

using PortunusAdiutor.Data;
using PortunusAdiutor.Models;
using PortunusAdiutor.Static;

namespace PortunusAdiutor.Services.MessagePoster;

/// <summary>
/// 	Implementation of <see cref="IMessagePoster{TUser, TKey}"/>
/// 	with access links.
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
public class LinkMessagePoster<TContext, TUser, TKey> : IMessagePoster<TUser, TKey>
where TContext : ManagedUserDbContext<TUser, TKey>
where TUser : class, IManagedUser<TUser, TKey>
where TKey : IEquatable<TKey>
{
	private readonly LinkMessagePosterParams _posterParams;
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
	public LinkMessagePoster(
		LinkMessagePosterParams posterParams,
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
		var sut = _context.GenAndSaveSingleUseToken(
			user.Id, 
			MessageType.EmailConfirmation, 
			out _
		);
		// Builds and sends message
		var message = _posterParams.EmailConfirmationMessageBuilder(
			user.Email,
			_posterParams.EmailConfirmationEndpoint + sut.Token
		);
		SendMessage(message);
	}

	/// <inheritdoc/>
	public void SendPasswordRedefinitionMessage(TUser user)
	{
		ArgumentException.ThrowIfNullOrEmpty(user.Email);
		// Generates SUT
		var sut = _context.GenAndSaveSingleUseToken(
			user.Id, 
			MessageType.PasswordRedefinition, 
			out _
		);
		// Builds and sends message
		var message = _posterParams.PasswordRedefinitionMessageBuilder(
			user.Email,
			_posterParams.PasswordRedefinitionEndpoint + sut.Token
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
