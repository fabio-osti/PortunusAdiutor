using PortunusAdiutor.Services.MessagePoster;

namespace PortunusAdiutor.Models;

/// <summary>
///     Class representing a password for special access.
/// </summary>
/// 
/// <typeparam name="TUser">
///     Type of the user.
/// </typeparam>
public class UserToken<TUser>
{
	/// <summary>
	///     Initializes an instance of the class.
	/// </summary>
	/// 
	/// <param name="userId">
	///     The primary key of the user that the token will authenticate.
	/// </param>
	/// 
	/// <param name="token">
	///     A 'X' digits code.
	/// </param>
	/// 
	/// <param name="type">
	///     The string representation of the <see cref="TokenType" />
	///     that will include this token.
	/// </param>
	public UserToken(
		Guid userId,
		string token,
		TokenType type
	)
	{
		Token = token;
		UserId = userId;
		Type = type;
		ExpiresOn = DateTime.UtcNow.AddMinutes(15);
	}

	private UserToken(
		Guid userId,
		string token,
		TokenType type,
		DateTime expiresOn
	)
	{
		UserId = userId;
		Token = token;
		Type = type;
		ExpiresOn = expiresOn;
	}

	/// <summary>
	///     The user this <see cref="UserToken{TUser}" />
	///     gives access to.
	/// </summary>
	public TUser? User { get; init; }

	/// <summary>
	///     The primary key of the user this
	///     <see cref="UserToken{TUser}" /> gives access to.
	/// </summary>
	public Guid UserId { get; init; }

	/// <summary>
	///     The token.
	/// </summary>
	public string Token { get; init; }

	/// <summary>
	///     The type of access given to
	///     by this <see cref="UserToken{TUser}" />.
	/// </summary>
	public TokenType Type { get; init; }

	/// <summary>
	///     Expiration <see cref="DateTime" />
	///     of this <see cref="UserToken{TUser}" />.
	/// </summary>
	public DateTime ExpiresOn { get; init; }
}