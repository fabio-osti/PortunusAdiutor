using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using PortunusAdiutor.Models.Code;

namespace PortunusAdiutor.Models.User;

/// <summary>
///     Defines all necessary methods for managing an user.
/// </summary>
/// 
/// <typeparam name="TUser">
///     Type of the user.
/// </typeparam>
public interface IManagedUser<TUser>
{
	/// <summary>
	///     Gets or sets the id.
	/// </summary>
	Guid Id { get; set; }

	/// <summary>
	///     Gets or sets the hashed password.
	/// </summary>
	string PasswordHash { get; set; }

	/// <summary>
	///     Gets or sets the salt used by <see cref="SetPassword(string)" />
	///     and <see cref="ValidatePassword(string)" />.
	/// </summary>
	byte[] Salt { get; set; }

	/// <summary>
	///     Gets or sets the email.
	/// </summary>
	string Email { get; set; }

	/// <summary>
	///     Gets or sets if this user email is confirmed.
	/// </summary>
	bool EmailConfirmed { get; set; }

	/// <summary>
	///     Gets or sets if this user has 2FA enabled.
	/// </summary>
	bool TwoFactorAuthenticationEnabled { get; set; }

	/// <summary>
	///     Gets or sets <see cref="UserCode{TUser}" /> related to this user.
	/// </summary>
	ICollection<UserCode<TUser>>? UserCodes { get; set; }

	/// <summary>
	///     Sets an user password to <paramref name="password" />.
	/// </summary>
	/// 
	/// <param name="password">
	///     Plain text password.
	/// </param>
	[MemberNotNull(nameof(Salt))]
	void SetPassword(string password);

	/// <summary>
	///     Validates <paramref name="password" /> with the user password.
	/// </summary>
	/// 
	/// <param name="password">
	///     Plain text password.
	/// </param>
	bool ValidatePassword(string password);

	/// <summary>
	///     Gets a collection of this user <see cref="Claim" />.
	/// </summary>
	/// 
	/// <returns>
	///     An <see cref="Array" /> where each element is an user <see cref="Claim" />.
	/// </returns>
	Claim[] GetClaims();
}