using System.Security.Claims;
using PortunusAdiutor.Models.User;

namespace PortunusCodeExample.Models;

public class ApplicationUser : Pbkdf2User<ApplicationUser>
{
	public ApplicationUser(
		string email,
		string password,
		bool admin
	) : base(email, password)
	{
		IsAdmin = admin;
		TwoFactorAuthenticationEnabled = admin;
	}

	public ApplicationUser(
		Guid id,
		string email,
		byte[] salt,
		string passwordHash
	) : base(id, email, salt, passwordHash) { }

	public bool IsAdmin { get; set; }

	public override Claim[] GetClaims()
	{
		return base.GetClaims()
			.Append(new("is-admin", IsAdmin.ToString()))
			.ToArray();
	}
}