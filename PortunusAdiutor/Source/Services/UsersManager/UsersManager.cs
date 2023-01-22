using System.Linq.Expressions;
using Microsoft.IdentityModel.Tokens;
using PortunusAdiutor.Data;
using PortunusAdiutor.Helpers;
using PortunusAdiutor.Models;
using PortunusAdiutor.Services.MessagePoster;
using PortunusAdiutor.Services.TokenBuilder;

namespace PortunusAdiutor.Services.UsersManager;

/// <summary>
///     Default implementation of <see cref="IUsersManager{TUser}" />.
/// </summary>
/// 
/// <typeparam name="TContext">
///     Type of the DbContext.
/// </typeparam>
/// 
/// <typeparam name="TUser">
///     Type of the user.
/// </typeparam>
public class UsersManager<TContext, TUser> : IUsersManager<TUser>
	where TContext : ManagedUserDbContext<TUser>
	where TUser : class, IManagedUser<TUser>

{
	/// <summary>
	///     Initializes an instance of the class.
	/// </summary>
	/// 
	/// <param name="tokenBuilder">
	///     Service for building JWT tokens.
	/// </param>
	/// 
	/// <param name="messagePoster">
	///     Service for sending the messages.
	/// </param>
	/// 
	/// <param name="context">
	///     Database context used for identity.
	/// </param>
	public UsersManager(
		ITokenBuilder tokenBuilder,
		IMessagePoster<TUser> messagePoster,
		TContext context
	)
	{
		_tokenBuilder = tokenBuilder;
		_mailPoster = messagePoster;
		_context = context;
	}

	private readonly TContext _context;
	private readonly IMessagePoster<TUser> _mailPoster;
	private readonly ITokenBuilder _tokenBuilder;

	/// <inheritdoc />
	public UserResult<TUser> CreateUser(
		Expression<Func<TUser, bool>> userFinder,
		Func<TUser> userBuilder,
		bool sendConfirmationMail = true
	)
	{
		if (_context.Users.FirstOrDefault(userFinder) is not null)
			return new(UserResultStatus.UserAlreadyExists);

		var user = _context.Users.Add(userBuilder()).Entity;
		_context.SaveChanges();

		if (sendConfirmationMail)
			_mailPoster.SendEmailConfirmationMessage(user);

		return new(user);
	}

	/// <inheritdoc />
	public UserResult<TUser> ValidateUser(
		Expression<Func<TUser, bool>> userFinder,
		string userPassword,
		string? token = null
	)
	{
		var user = _context.Users.FirstOrDefault(userFinder);

		if (user is null) return new(UserResultStatus.UserNotFound);

		if (!user.ValidatePassword(userPassword))
			return new(UserResultStatus.InvalidPassword);

		if (user.TwoFactorAuthenticationEnabled) {
			if (token is null) return new(UserResultStatus.TwoFactorRequired);

			_context.ConsumeToken(
				user.Id,
				token,
				MessageType.TwoFactorAuthentication,
				false
			);
		}

		return new(user);
	}

	/// <inheritdoc />
	public UserResult<TUser> ConfirmEmail(
		Expression<Func<TUser, bool>> userFinder,
		string token
	)
	{
		var user = _context.Users.FirstOrDefault(userFinder);

		if (user is null) return new(UserResultStatus.UserNotFound);

		var result = _context.ConsumeToken(
			user.Id,
			token,
			MessageType.EmailConfirmation
		);

		if (result != UserResultStatus.Ok) return new(result);

		user.EmailConfirmed = true;
		_context.SaveChanges();

		return new(user);
	}

	/// <inheritdoc />
	public UserResult<TUser> RedefinePassword(
		Expression<Func<TUser, bool>> userFinder,
		string token,
		string newPassword
	)
	{
		var user = _context.Users.FirstOrDefault(userFinder);

		if (user is null) return new(UserResultStatus.UserNotFound);

		var result = _context.ConsumeToken(
			user.Id,
			token,
			MessageType.PasswordRedefinition
		);

		if (result != UserResultStatus.Ok) return new(result);

		user.SetPassword(newPassword);
		_context.SaveChanges();

		return new(user);
	}

	/// <inheritdoc />
	public UserResult<TUser> SendEmailConfirmation(
		Expression<Func<TUser, bool>> userFinder
	)
	{
		var user = _context.Users.FirstOrDefault(userFinder);

		if (user is null) return new(UserResultStatus.UserNotFound);

		if (user.EmailConfirmed)
			return new(UserResultStatus.UserAlreadyConfirmed);

		_mailPoster.SendEmailConfirmationMessage(user);

		return new(user);
	}

	/// <inheritdoc />
	public UserResult<TUser> SendPasswordRedefinition(
		Expression<Func<TUser, bool>> userFinder
	)
	{
		var user = _context.Users.FirstOrDefault(userFinder);

		if (user is null) return new(UserResultStatus.UserNotFound);

		_mailPoster.SendPasswordRedefinitionMessage(user);

		return new(user);
	}

	/// <inheritdoc />
	public UserResult<TUser> SendTwoFactorAuthentication(
		Expression<Func<TUser, bool>> userFinder
	)
	{
		var user = _context.Users.FirstOrDefault(userFinder);

		if (user is null) return new(UserResultStatus.UserNotFound);

		_mailPoster.SendTwoFactorAuthenticationMessage(user);

		return new(user);
	}

	/// <inheritdoc />
	public UserResult<TUser> FindUser(Expression<Func<TUser, bool>> userFinder)
	{
		var user = _context.Users.FirstOrDefault(userFinder);

		if (user is null) return new(UserResultStatus.UserNotFound);

		return new(user);
	}

	/// <inheritdoc />
	public string GetJwt(TUser user)
	{
		return _tokenBuilder.BuildToken(user.GetClaims());
	}

	/// <inheritdoc />
	public string GetJwt(
		TUser user,
		SecurityTokenDescriptor tokenDescriptor
	)
	{
		tokenDescriptor.Subject = new(user.GetClaims());
		return _tokenBuilder.BuildToken(tokenDescriptor);
	}
}