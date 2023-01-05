using PortunusAdiutor.Models;

public class ApplicationUser : Pbkdf2User<ApplicationUser, Guid>
{
	public ApplicationUser(string email, string password) : base(Guid.NewGuid(), email, password)
	{
	}

	public ApplicationUser(Guid id, string email, byte[] salt, string passwordHash) : base(id, email, salt, passwordHash)
	{
	}
}