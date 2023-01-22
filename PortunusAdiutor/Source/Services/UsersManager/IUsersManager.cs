using System.Linq.Expressions;
using Microsoft.IdentityModel.Tokens;
using PortunusAdiutor.Helpers;
using PortunusAdiutor.Models;

namespace PortunusAdiutor.Services.UsersManager;

// TODO: Use Maybe<TUser> as return type for most methods.

/// <summary>
///     Manages users on the database context.
/// </summary>
/// 
/// <typeparam name="TUser">
///     Type of the user.
/// </typeparam>
public interface IUsersManager<TUser> where TUser : class, IManagedUser<TUser>
{
	/// <summary>
	///     Creates an user.
	/// </summary>
	/// 
	/// <param name="userFinder">
	///     Predicate for finding duplicate user.
	/// </param>
	/// 
	/// <param name="userBuilder">
	///     Builder of the user.
	/// </param>
	/// 
	/// <param name="sendConfirmationMail">
	/// 
	/// </param>
	/// 
	/// <returns>
	///     Created user.
	/// </returns>
	UserResult<TUser> CreateUser(
		Expression<Func<TUser, bool>> userFinder,
		Func<TUser> userBuilder,
		bool sendConfirmationMail = true
	);

	/// <summary>
	///     Validates an user.
	/// </summary>
	/// 
	/// <param name="userFinder">
	///     Predicate for finding the user to be validated.
	/// </param>
	/// 
	/// <param name="userPassword">
	///     Plain text password to be validated.
	/// </param>
	/// 
	/// <param name="twoFactorCode">
	///     Code for users with 2FA enabled.
	/// </param>
	UserResult<TUser> ValidateUser(
		Expression<Func<TUser, bool>> userFinder,
		string userPassword,
		string? twoFactorCode = null
	);

	/// <summary>
	///     Confirm the email of the user to whom this <paramref name="token" /> belongs to.
	/// </summary>
	/// 
	/// <param name="userFinder">
	///     Predicate for finding the user to confirm its email.
	/// </param>
	/// 
	/// <param name="token">
	///     Token for the action.
	/// </param>
	/// 
	/// <returns>
	///     User that had his email confirmed.
	/// </returns>
	UserResult<TUser> ConfirmEmail(
		Expression<Func<TUser, bool>> userFinder,
		string token
	);

	/// <summary>
	///     Redefines the password of the user to whom this <paramref name="token" /> belongs to.
	/// </summary>
	/// 
	/// <param name="userFinder">
	///     Predicate for finding the user to redefine its password.
	/// </param>
	/// 
	/// <param name="token">
	///     Token for the action.
	/// </param>
	/// 
	/// <param name="newPassword">
	///     Password to be set.
	/// </param>
	/// 
	/// <returns>
	///     User that had his password redefined.
	/// </returns>
	UserResult<TUser> RedefinePassword(
		Expression<Func<TUser, bool>> userFinder,
		string token,
		string newPassword
	);

	/// <summary>
	///     Sends a message to an user for email confirmation.
	/// </summary>
	/// 
	/// <param name="userFinder">
	///     Predicate for finding the user to be sent the message.
	/// </param>
	/// 
	/// <returns>
	///     User to whom the email confirmation message was sent.
	/// </returns>
	UserResult<TUser> SendEmailConfirmation(
		Expression<Func<TUser, bool>> userFinder
	);

	/// <summary>
	///     Sends a message to an user for password redefinition.
	/// </summary>
	/// 
	/// <param name="userFinder">
	///     Predicate for finding the user to be sent the message.
	/// </param>
	/// 
	/// <returns>
	///     User to whom the password redefinition message was sent.
	/// </returns>
	UserResult<TUser> SendPasswordRedefinition(
		Expression<Func<TUser, bool>> userFinder
	);

	/// <summary>
	///     Sends a message to an user for 2FA.
	/// </summary>
	/// 
	/// <param name="userFinder">
	///     Predicate for finding the user to be sent the message.
	/// </param>
	/// 
	/// <returns>
	///     User to whom the 2FA message was sent.
	/// </returns>
	UserResult<TUser> SendTwoFactorAuthentication(
		Expression<Func<TUser, bool>> userFinder
	);

	/// <summary>
	///     Helper to find an user on the DB.
	/// </summary>
	/// 
	/// <param name="userFinder">
	///     Predicate for finding the user.
	/// </param>
	/// 
	/// <returns>
	///     User found.
	/// </returns>
	UserResult<TUser> FindUser(Expression<Func<TUser, bool>> userFinder);

	/// <summary>
	///     Gets JWT using the default <see cref="SecurityTokenDescriptor" />.
	/// </summary>
	/// 
	/// <param name="user">
	///     Owner of token.
	/// </param>
	/// 
	/// <returns>
	///     String representation of the JWT.
	/// </returns>
	string GetJwt(TUser user);

	/// <summary>
	///     Gets JWT using <paramref name="tokenDescriptor" />.
	/// </summary>
	/// 
	/// <param name="user">
	///     Owner of token.
	/// </param>
	/// 
	/// <param name="tokenDescriptor">
	///     Descriptor of the JWT.
	/// </param>
	/// 
	/// <remarks>
	///     The following members of <paramref name="tokenDescriptor" />
	///     will be overwritten:
	///     <list type="bullet">
	///         <item>
	///             <see cref="SecurityTokenDescriptor.SigningCredentials" />
	///         </item>
	///         <item>
	///             <see cref="SecurityTokenDescriptor.EncryptingCredentials" />
	///         </item>
	///         <item>
	///             <see cref="SecurityTokenDescriptor.Subject" />
	///         </item>
	///     </list>
	/// </remarks>
	/// 
	/// <returns>
	///     String representation of the JWT.
	/// </returns>
	string GetJwt(
		TUser user,
		SecurityTokenDescriptor tokenDescriptor
	);
}