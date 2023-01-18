using Microsoft.EntityFrameworkCore;
using PortunusAdiutor.Data;
using PortunusCodeExample.Models;

namespace PortunusCodeExample.Data
{
	public class ApplicationDbContext : ManagedUserDbContext<ApplicationUser>
	{
#pragma warning disable CS8618
		public ApplicationDbContext(DbContextOptions options) : base(options) { }
#pragma warning restore CS8618
	}
}