namespace PortunusAdiutor.Models.Code;

/// <summary>
///     Class representing a password for special access.
/// </summary>
/// 
/// <typeparam name="TUser">
///     Type of the user.
/// </typeparam>
public class UserCode<TUser>
{
	/// <summary>
	///     Initializes an instance of the class.
	/// </summary>
	/// 
	/// <param name="userId">
	///     The primary key of the user that the token will authenticate.
	/// </param>
	/// 
	/// <param name="code">
	///     A 'X' digits code.
	/// </param>
	/// 
	/// <param name="type">
	///     The string representation of the <see cref="CodeType" />
	///     that will include this token.
	/// </param>
	public UserCode(
		Guid userId,
		string code,
		CodeType type
	)
	{
		Code = code;
		UserId = userId;
		Type = type;
		ExpiresOn = DateTime.UtcNow.AddMinutes(15);
	}

	private UserCode(
		Guid userId,
		string token,
		CodeType type,
		DateTime expiresOn
	)
	{
		UserId = userId;
		Code = token;
		Type = type;
		ExpiresOn = expiresOn;
	}

	/// <summary>
	///     The user this <see cref="UserCode{TUser}" />
	///     gives access to.
	/// </summary>
	public TUser? User { get; init; }

	/// <summary>
	///     The primary key of the user this
	///     <see cref="UserCode{TUser}" /> gives access to.
	/// </summary>
	public Guid UserId { get; init; }

	/// <summary>
	///     The token.
	/// </summary>
	public string Code { get; init; }

	/// <summary>
	///     The type of access given to
	///     by this <see cref="UserCode{TUser}" />.
	/// </summary>
	public CodeType Type { get; init; }

	/// <summary>
	///     Expiration <see cref="DateTime" />
	///     of this <see cref="UserCode{TUser}" />.
	/// </summary>
	public DateTime ExpiresOn { get; init; }
}