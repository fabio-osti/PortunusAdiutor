using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Security.Cryptography;

using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using PortunusAdiutor.Static;

namespace PortunusAdiutor.Models;

/// <summary>
/// 	Implementation of <see cref="IManagedUser{TUser}"/> using PBKDF2 as derivation algorithm.
/// </summary>
///
/// <typeparam name="TUser">
/// 	Type of the user.
/// </typeparam>
public class Pbkdf2User<TUser> : IManagedUser<TUser>
where TUser : Pbkdf2User<TUser>
{
	private const KeyDerivationPrf DefaultPrf = KeyDerivationPrf.HMACSHA512;
	private const int DefaultIterCount = 262140;
	private const int DefaultHashedSize = 128;

	/// <summary>
	/// 	Initializes an instance of the class.
	/// </summary>
	///
	/// <param name="id">
	/// 	Id of the user.
	/// </param>
	///
	/// <param name="email">
	/// 	Email of the user.
	/// </param>
	///
	/// <param name="salt">
	/// 	Salt of the user.
	/// </param>
	///
	/// <param name="passwordHash">
	/// 	Hashed password of the user.
	/// </param>
	///
	/// <remarks>
	/// 	This constructor should only be used by EF to build an object
	/// 	representing an existing <see cref="Pbkdf2User{TUser}"/>.
	/// </remarks>
	public Pbkdf2User(Guid id, string email, byte[] salt, string passwordHash)
	{
		Id = id;
		PasswordHash = passwordHash;
		Email = email;
		Salt = salt;
	}

	/// <summary>
	/// 	Initializes an instance of the class.
	/// </summary>
	///
	/// <param name="id">
	/// 	Id of the user</param>
	///
	/// <param name="email">
	/// 	Email of the user.
	/// </param>
	///
	/// <param name="password">
	/// 	Password of the user.
	/// </param>
	public Pbkdf2User(Guid id, string email, string password)
	{
		Id = id;
		Email = email;
		SetPassword(password);
	}

	/// <inheritdoc/>
	public Guid Id { get; set; }

	/// <inheritdoc/>
	[MemberNotNull(nameof(Salt), nameof(PasswordHash))]
	public void SetPassword(string password)
	{
		Salt =
			SHA256.HashData(BitConverter.GetBytes(DateTime.UtcNow.ToBinary()));
		PasswordHash = DeriveKey(password);
		return;
	}

	/// <inheritdoc/>
	public bool ValidatePassword(string password)
	{
		return PasswordHash == DeriveKey(password);
	}

	/// <inheritdoc/>
	public string Email { get; set; }

	/// <inheritdoc/>
	public string PasswordHash { get; set; }

	/// <inheritdoc/>
	public byte[] Salt { get; set; }

	/// <inheritdoc/>
	public bool EmailConfirmed { get; set; }

	/// <inheritdoc/>
	public bool TwoFactorAuthenticationEnabled { get; set; }

	/// <inheritdoc/>
	public virtual Claim[] GetClaims()
	{
		var id = Id.ToString();
		ArgumentNullException.ThrowIfNull(id);
		ArgumentNullException.ThrowIfNull(Email);
		return new[] {
			new Claim(ClaimTypes.PrimarySid, id),
			new Claim(ClaimTypes.Email, Email),
			new Claim(JwtCustomClaims.EmailConfirmed, EmailConfirmed.ToString())
		};
	}

	/// <inheritdoc/>
	public ICollection<UserToken<TUser>>? UserTokens { get; set; }
	
	private string DeriveKey(string password)
	{
		var hashed = KeyDerivation.Pbkdf2(
			password,
			Salt,
			DefaultPrf,
			DefaultIterCount,
			DefaultHashedSize
		);
		return Convert.ToBase64String(hashed);
	}
}
