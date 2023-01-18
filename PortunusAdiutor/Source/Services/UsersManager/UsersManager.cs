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
///
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
	public TUser CreateUser(Expression<Func<TUser, bool>> userFinder, Func<TUser> userBuilder)
	{
		if (_context.Users.FirstOrDefault(userFinder) is not null) {
			throw new UserAlreadyExistsException();
		}

		var user = _context.Users.Add(userBuilder()).Entity;
		_mailPoster.SendEmailConfirmationMessage(user);
		_context.SaveChanges();
		return user;
	}

	/// <inheritdoc/>
	public TUser ValidateUser(Expression<Func<TUser, bool>> userFinder, string userPassword)
	{
		var user = _context.Users.FirstOrDefault(userFinder);
		user.ThrowIfUserNull<TUser>();

		if (!user.ValidatePassword(userPassword)) {
			throw new InvalidPasswordException();
		}

		return user;
	}

	/// <inheritdoc/>
	public TUser SendEmailConfirmation(Expression<Func<TUser, bool>> userFinder)
	{
		var user = _context.Users.FirstOrDefault(userFinder);
		user.ThrowIfUserNull<TUser>();

		if (user.EmailConfirmed) {
			throw new EmailAlreadyConfirmedException();
		}

		_mailPoster.SendEmailConfirmationMessage(user);

		return user;
	}

	/// <inheritdoc/>
	public TUser ConfirmEmail(string userToken)
	{
		var userId = _context.ConsumeToken(
			userToken,
			MessageType.EmailConfirmation
		);
		var user = _context.Users.Find(userId);
		user.ThrowIfUserNull<TUser>();
		user.EmailConfirmed = true;
		_context.SaveChanges();

		return user;
	}

	/// <inheritdoc/>
	public TUser SendPasswordRedefinition(Expression<Func<TUser, bool>> userFinder)
	{
		var user = _context.Users.FirstOrDefault(userFinder);
		user.ThrowIfUserNull<TUser>();

		_mailPoster.SendPasswordRedefinitionMessage(user);

		return user;
	}

	/// <inheritdoc/>
	public TUser RedefinePassword(
		string userToken,
		string newPassword
	)
	{
		var userId = _context.ConsumeToken(
			userToken,
			MessageType.PasswordRedefinition
		);
		var user = _context.Users.Find(userId);
		user.ThrowIfUserNull<TUser>();
		user.SetPassword(newPassword);
		_context.SaveChanges();

		return user;
	}

	/// <inheritdoc/>
	public TUser FindUser(Expression<Func<TUser, bool>> userFinder)
	{
		return _context.Users.FirstOrDefault(userFinder) ?? throw new UserNotFoundException();
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
