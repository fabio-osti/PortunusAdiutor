namespace PortunusAdiutor.Services.MessagePoster;

/// <summary>
///     Enumeration of types of message.
/// </summary>
public enum MessageType
{
	/// <summary>
	///     Message for email confirmation.
	/// </summary>
	EmailConfirmation,

	/// <summary>
	///     Message for password redefinition.
	/// </summary>
	PasswordRedefinition,

	/// <summary>
	///     Message for two steps authentication.
	/// </summary>
	TwoFactorAuthentication
}