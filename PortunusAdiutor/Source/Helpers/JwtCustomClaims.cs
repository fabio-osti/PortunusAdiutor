namespace PortunusAdiutor.Helpers;

/// <summary>
///     Class to define common claims constants.
/// </summary>
public static class JwtCustomClaims
{
	/// <summary>
	///     Claim of type "email-confirmed".
	/// </summary>
	public const string EmailConfirmed = "email-confirmed";

	/// <summary>
	///     Claim of type "admin".
	/// </summary>
	public const string Admin = "admin";
}