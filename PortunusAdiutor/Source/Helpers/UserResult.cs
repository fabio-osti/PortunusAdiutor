using PortunusAdiutor.Exceptions;
using PortunusAdiutor.Models.User;

namespace PortunusAdiutor.Helpers;

/// <summary>
///     Class representing the result of
///     an operation that may or may
///     not have succeeded with a
///     <typeparamref name="TUser" />.
/// </summary>
/// 
/// <typeparam name="TUser">
///     Type of the user.
/// </typeparam>
public class UserResult<TUser> where TUser : IManagedUser<TUser>
{
	/// <summary>
	///     Initializes a successful instance of the class
	///     with a <paramref name="user" />
	/// </summary>
	/// 
	/// <param name="user">
	///     Wrapped user.
	/// </param>
	public UserResult(TUser user)
	{
		_user = user;
		Status = UserResultStatus.Ok;
	}

	/// <summary>
	///     Initializes a failed instance of the class
	///     with a <paramref name="status" />
	/// </summary>
	/// 
	/// <param name="status">
	///     Reason of failure.
	/// </param>
	public UserResult(UserResultStatus status)
	{
		if (status == UserResultStatus.Ok)
			throw new ArgumentException(
				$"User can't be null when {nameof(status)}"
				+ $"is equals to {nameof(UserResultStatus.Ok)}"
			);

		Status = status;
	}

	private readonly TUser? _user;

	/// <summary>
	///     Gets the User if successful, throws a <see cref="PortunusException" />
	///     with <see cref="UserResultStatusDescription.GetDescription(UserResultStatus)" />
	///     as <see cref="PortunusException.ShortMessage" />.
	/// </summary>
	public TUser User =>
		_user ?? throw new PortunusException(Status.GetDescription());

	/// <summary>
	///     Gets the status of this <see cref="UserResult{TUser}" />.
	/// </summary>
	public UserResultStatus Status { get; }
}