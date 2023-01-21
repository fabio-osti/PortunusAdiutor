using System.Linq.Expressions;
using Microsoft.IdentityModel.Tokens;
using PortunusAdiutor.Data;
using PortunusAdiutor.Exceptions;
using PortunusAdiutor.Extensions;
using PortunusAdiutor.Models;
using PortunusAdiutor.Services.MessagePoster;
using PortunusAdiutor.Services.TokenBuilder;
using PortunusAdiutor.Static;

namespace PortunusAdiutor.Services.UsersManager;

/// <summary>
/// 	Default implementation of <see cref="IUsersManager{TUser}"/>.
/// </summary>
///
/// <typeparam name="TContext">
/// 	Type of the DbContext.
/// </typeparam>
///
/// <typeparam name="TUser">
/// 	Type of the user.
/// </typeparam>
public class UsersManager<TContext, TUser> : IUsersManager<TUser>
	where TContext : ManagedUserDbContext<TUser>
	where TUser : class, IManagedUser<TUser>
	
{
	private readonly ITokenBuilder _tokenBuilder;
	private readonly IMessagePoster<TUser> _mailPoster;
	private readonly TContext _context;

	/// <summary>
	/// 	Initializes an instance of the class.
	/// </summary>
	/// 
	/// <param name="tokenBuilder">
	/// 	Service for building JWT tokens.
	/// </param>
	///
	/// <param name="messagePoster">
	/// 	Service for sending the messages.
	/// </param>
	///
	/// <param name="context">
	/// 	Database context used for identity.
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

	/// <inheritdoc/>
	public TUser CreateUser(
		Expression<Func<TUser, bool>> userFinder, 
		Func<TUser> userBuilder,
		bool sendConfirmationMail = true
	)
	{
		if (_context.Users.FirstOrDefault(userFinder) is not null) {
			throw new UserAlreadyExistsException();
		}

		var user = _context.Users.Add(userBuilder()).Entity;
		_context.SaveChanges();

		if (sendConfirmationMail) {
			_mailPoster.SendEmailConfirmationMessage(user);
		}

		return user;
	}

	/// <inheritdoc/>
	public TUser ValidateUser(
		Expression<Func<TUser, bool>> userFinder, 
		string userPassword,
		string? token = null
	)
	{
		var user = _context.Users.FirstOrDefault(userFinder) 
			?? throw new UserNotFoundException();
		if (!user.ValidatePassword(userPassword)) {
			throw new InvalidPasswordException();
		}

		if (user.TwoFactorAuthenticationEnabled) {
			if (token is null) {
				throw new TwoFactorRequiredException();
			}
			_context.ConsumeToken(
				user.Id,
				token,
				MessageType.TwoFactorAuthentication,
				false
			);
		}
		return user;
	}

	/// <inheritdoc/>
	public void ConfirmEmail(
		Expression<Func<TUser, bool>> userFinder, 
		string token
	)
	{
		var user = _context.Users.FirstOrDefault(userFinder) 
			?? throw new UserNotFoundException();
		_context.ConsumeToken(
			user.Id,
			token,
			MessageType.EmailConfirmation
		);
		user.EmailConfirmed = true;
		_context.SaveChanges();
	}

	/// <inheritdoc/>
	public void RedefinePassword(
		Expression<Func<TUser, bool>> userFinder,
		string token,
		string newPassword
	)
	{
		var user = _context.Users.FirstOrDefault(userFinder) 
			?? throw new UserNotFoundException();
		_context.ConsumeToken(
			user.Id,
			token,
			MessageType.PasswordRedefinition
		);
		user.SetPassword(newPassword);
		_context.SaveChanges();
	}

	/// <inheritdoc/>
	public void SendEmailConfirmation(Expression<Func<TUser, bool>> userFinder)
	{
		var user = _context.Users.FirstOrDefault(userFinder) 
			?? throw new UserNotFoundException();
		if (user.EmailConfirmed) {
			throw new EmailAlreadyConfirmedException();
		}
		_mailPoster.SendEmailConfirmationMessage(user);
	}

	/// <inheritdoc/>
	public void SendPasswordRedefinition(Expression<Func<TUser, bool>> userFinder)
	{
		var user = _context.Users.FirstOrDefault(userFinder) 
			?? throw new UserNotFoundException();
		_mailPoster.SendPasswordRedefinitionMessage(user);
	}

	/// <inheritdoc/>
	public void SendTwoFactorAuthentication(Expression<Func<TUser, bool>> userFinder)
	{
		var user = _context.Users.FirstOrDefault(userFinder) 
			?? throw new UserNotFoundException();
		_mailPoster.SendTwoFactorAuthenticationMessage(user);
	}

	/// <inheritdoc/>
	public TUser FindUser(Expression<Func<TUser, bool>> userFinder)
	{
		return _context.Users.FirstOrDefault(userFinder) 
			?? throw new UserNotFoundException();
	}

	/// <inheritdoc/>
	public string GetJwt(TUser user)
	{
		return _tokenBuilder.BuildToken(user.GetClaims());
	}

	/// <inheritdoc/>
	public string GetJwt(
		TUser user,
		SecurityTokenDescriptor tokenDescriptor
	)
	{
		tokenDescriptor.Subject = new(user.GetClaims());
		return _tokenBuilder.BuildToken(tokenDescriptor);
	}
}
