using Microsoft.EntityFrameworkCore;
using PortunusAdiutor.Models.User;
using PortunusAdiutor.Models.Code;

namespace PortunusAdiutor.Data;

/// <summary>
///     Base class for the Entity Framework database context.
/// </summary>
/// 
/// <typeparam name="TUser">
///     Type of the user.
/// </typeparam>
public class ManagedUserDbContext<TUser> : DbContext
	where TUser : class, IManagedUser<TUser>
{
#pragma warning disable CS8618
	/// <summary>
	///     Initializes a new instance of the class.
	/// </summary>
	/// 
	/// <param name="options">
	///     Options to be used by the <see cref="DbContext" />.
	/// </param>
	public ManagedUserDbContext(DbContextOptions options) : base(options) { }
#pragma warning restore CS8618

	/// <summary>
	///     Gets or sets the <see cref="DbSet{TEntity}" />
	///     containing all Users.
	/// </summary>
	public DbSet<TUser> Users { get; protected set; }

	/// <summary>
	///     Gets or sets the <see cref="DbSet{TEntity}" />
	///     containing all UserTokens.
	/// </summary>
	public DbSet<UserCode<TUser>> UsersCodes { get; protected set; }

	/// <inheritdoc />
	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);
		var userBuilder = builder.Entity<TUser>();

		userBuilder.HasKey(e => e.Id);

		var tokenBuilder = builder.Entity<UserCode<TUser>>();

		tokenBuilder.HasKey(
			e => new {
				e.UserId,
				e.Code,
				e.Type
			}
		);

		tokenBuilder.HasOne<TUser>(e => e.User).WithMany(e => e.UserCodes);
	}
}