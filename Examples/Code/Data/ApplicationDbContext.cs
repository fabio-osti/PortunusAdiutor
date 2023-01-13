using Microsoft.EntityFrameworkCore;
using PortunusAdiutor.Data;

namespace PortunusTester.Data
{
	public class ApplicationDbContext : ManagedUserDbContext<ApplicationUser, Guid>
	{
#pragma warning disable CS8618
		public ApplicationDbContext(DbContextOptions options) : base(options) { }
#pragma warning restore CS8618
	}
}