using System.Security.Claims;
using PortunusAdiutor.Models;

namespace PortunusCodeExample.Models
{
	public class ApplicationUser : Pbkdf2User<ApplicationUser>
	{
		public ApplicationUser(
			string email,
			string password,
			bool admin
		) : base(Guid.NewGuid(), email, password)
		{
			IsAdmin = admin;
		}

		public ApplicationUser(
			Guid id,
			string email,
			byte[] salt,
			string passwordHash
		) : base(id, email, salt, passwordHash)
		{
		}

		public bool IsAdmin { get; set; }

		public override Claim[] GetClaims() => base.GetClaims().Append(new("is-admin", IsAdmin.ToString())).ToArray();
	}
}