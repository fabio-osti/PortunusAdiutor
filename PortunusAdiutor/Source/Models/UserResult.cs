using PortunusAdiutor.Exceptions;
using PortunusAdiutor.Models;

/// <summary>
/// 	Class representing the result of
/// 	an operation that may or may
/// 	not have succeeded with a
/// 	<typeparamref name="TUser"/>.
/// </summary>
/// 
/// <typeparam name="TUser">
/// 	Type of the user.
/// </typeparam>
public class UserResult<TUser>
where TUser : IManagedUser<TUser>
{
	/// <summary>
	/// 	Initializes a successful instance of the class
	/// 	with a <paramref name="user"/>
	/// </summary>
	/// 
	/// <param name="user">
	/// 	Wrapped user.
	/// </param>
	public UserResult(TUser user)
	{
		_user = user;
		Status = UserResultStatus.Ok;
	}

	/// <summary>
    /// 	Initializes a failed instance of the class
    /// 	with a <paramref name="status"/>
    /// </summary>
    /// 
    /// <param name="status">
	/// 	Reason of failure.
	/// </param>
	public UserResult(UserResultStatus status)
	{
		if (status == UserResultStatus.Ok)
		{
			throw new ArgumentException(
				$"User can't be null when {nameof(status)}" +
				$"is equals to {nameof(UserResultStatus.Ok)}"
			);
		}
		Status = status;
	}

	private TUser? _user;
	/// <summary>
    /// 	Gets the User if successful, throws a <see cref="PortunusException"/>
    /// 	with <see cref="UserResultStatusDescription.GetDescription(UserResultStatus)"/>
    /// 	as <see cref="PortunusException.ShortMessage"/>.
    /// </summary>
	public TUser User => _user ?? throw new PortunusException(Status.GetDescription());

	/// <summary>
    /// 	Gets the status of this <see cref="UserResult{TUser}"/>.
    /// </summary>
	public UserResultStatus Status { get; }
}

#pragma warning disable CS1591
public enum UserResultStatus
{
	Ok,
	UserNotFound,
	InvalidPassword,
	InvalidToken,
	TwoFactorRequired,
	UserAlreadyConfirmed,
	UserAlreadyExists,
}

public static class UserResultStatusDescription
{
	public const string Ok = "Success";
	public const string UserNotFound = "The user was not found.";
	public const string InvalidPassword = "The validation for this user and password failed.";
	public const string InvalidToken = "The validation for this user and token failed.";
	public const string TwoFactorRequired = "The user have 2FA enabled, but no token was provided.";
	public const string UserAlreadyConfirmed = "This user was already confirmed.";
	public const string UserAlreadyExists = "This user was already created.";

	public static string GetDescription(this UserResultStatus status) =>
		status switch
		{
			UserResultStatus.Ok => Ok,
			UserResultStatus.UserNotFound => UserNotFound,
			UserResultStatus.InvalidPassword => InvalidPassword,
			UserResultStatus.InvalidToken => InvalidToken,
			UserResultStatus.TwoFactorRequired => TwoFactorRequired,
			UserResultStatus.UserAlreadyConfirmed => UserAlreadyConfirmed,
			UserResultStatus.UserAlreadyExists => UserAlreadyExists,
			_ => throw new ArgumentException()
		};
}