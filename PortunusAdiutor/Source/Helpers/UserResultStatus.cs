namespace PortunusAdiutor.Helpers;
#pragma warning disable CS1591
public enum UserResultStatus
{
	Ok,
	UserNotFound,
	InvalidPassword,
	InvalidToken,
	TwoFactorRequired,
	UserAlreadyConfirmed,
	UserAlreadyExists
}

public static class UserResultStatusDescription
{
	public const string Ok = "Success";
	public const string UserNotFound = "The user was not found.";

	public const string InvalidPassword =
		"The validation for this user and password failed.";

	public const string InvalidToken =
		"The validation for this user and token failed.";

	public const string TwoFactorRequired =
		"The user have 2FA enabled, but no token was provided.";

	public const string UserAlreadyConfirmed =
		"This user was already confirmed.";

	public const string UserAlreadyExists = "This user was already created.";

	public static string GetDescription(this UserResultStatus status)
	{
		return status switch {
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
}