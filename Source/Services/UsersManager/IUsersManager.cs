using System.Linq.Expressions;
using Microsoft.IdentityModel.Tokens;
using PortunusAdiutor.Models;

namespace PortunusAdiutor.Services.UsersManager;

/// <summary>
/// 	Manages users on the database context.
/// </summary>
///
/// <typeparam name="TUser">
/// 	Type of the user.
/// </typeparam>
///
/// <typeparam name="TKey">
/// 	Type of the user primary key.
/// </typeparam>
public interface IUsersManager<TUser, TKey>
where TUser : class, IManagedUser<TUser, TKey>
where TKey : IEquatable<TKey>
{
	/// <summary>
	/// 	Creates an user.
	/// </summary>
	///
	/// <param name="userFinder">
	/// 	Predicate for finding duplicate user.
	/// </param>
	///
	/// <param name="userBuilder">
	/// 	Builder of the user.
	/// </param>
	///
	/// <returns>
	/// 	Created user.
	/// </returns>
	TUser CreateUser(Expression<Func<TUser, bool>> userFinder, Func<TUser> userBuilder);
	
	/// <summary>
	/// 	Validates an user.
	/// </summary>
	///
	/// <param name="userFinder">
	/// 	Predicate for finding the user.
	/// </param>
	///
	/// <param name="userPassword">
	/// 	Plain text password to be validated.
	/// </param>
	///
	/// <returns>
	/// 	Validated user.
	/// </returns>
	TUser ValidateUser(Expression<Func<TUser, bool>> userFinder, string userPassword);

	/// <summary>
	/// 	Sends a message to an user for email confirmation.
	/// </summary>
	///
	/// <param name="userFinder">
	/// 	Predicate for finding the user.
	/// </param>
	///
	/// <returns>
	/// 	User to whom the email confirmation message was sent.
	/// </returns>
	TUser SendEmailConfirmation(Expression<Func<TUser, bool>> userFinder);

	/// <summary>
	/// 	Confirm the email of the user to whom this <paramref name="token"/> belongs to.
	/// </summary>
	///
	/// <param name="token">
	/// 	Token for the action.
	/// </param>
	///
	/// <returns>
	/// 	User that had his email confirmed.
	/// </returns>
	TUser ConfirmEmail(string token);

	/// <summary>
	/// 	Sends a message to an user for password redefinition.
	/// </summary>
	///
	/// <param name="userFinder">
	/// 	Predicate for finding the user.
	/// </param>
	///
	/// <returns>
	/// 	User to whom the password redefinition message was sent.
	/// </returns>
	TUser SendPasswordRedefinition(Expression<Func<TUser, bool>> userFinder);

	/// <summary>
	/// 	Redefines the password of the user to whom this <paramref name="token"/> belongs to.
	/// </summary>
	///
	/// <param name="token">
	/// 	Token for the action.
	/// </param>
	///
	/// <param name="newPassword">
	/// 	Password to be set.
	/// </param>
	///
	/// <returns>
	/// 	User that had his password redefined.
	/// </returns>
	TUser RedefinePassword(string token, string newPassword);

	/// <summary>
	/// 	Helper to find an user on the DB.
	/// </summary>
	/// 
	/// <param name="userFinder">
	/// 	Predicate for finding the user.
	/// </param>
	/// 
	/// <returns>
	/// 	User found.
	/// </returns>
	TUser FindUser(Expression<Func<TUser, bool>> userFinder);

	/// <summary>
	/// 	Gets JWT using the default <see cref="SecurityTokenDescriptor"/>.
	/// </summary>
	/// 
	/// <param name="user">
	/// 	Owner of token.
	/// </param>
	/// 
	/// <returns>String representation of the JWT.</returns>
	string GetJwt(TUser user);


	/// <summary>
	/// 	Gets JWT using <paramref name="tokenDescriptor"/>.
	/// </summary>
	/// 
	/// <param name="user">
	/// 	Owner of token.
	/// </param>
	/// 
	/// <param name="tokenDescriptor">
	/// 	Descriptor of the JWT.
	/// </param>
	/// 
	/// <remarks>
	/// 	The following members of <paramref name="tokenDescriptor"/> 
	/// 	will be overwritten:
	/// 	<list type="bullet">
	/// 		<item><see cref="SecurityTokenDescriptor.SigningCredentials"/></item>
	/// 		<item><see cref="SecurityTokenDescriptor.EncryptingCredentials"/></item>
	/// 		<item><see cref="SecurityTokenDescriptor.Subject"/></item>
	/// 	</list>
	/// </remarks>
	/// 
	/// <returns>String representation of the JWT.</returns>
	string GetJwt(TUser user, SecurityTokenDescriptor tokenDescriptor);
}