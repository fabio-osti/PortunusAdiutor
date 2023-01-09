using Microsoft.EntityFrameworkCore;
using PortunusAdiutor.Models;

namespace PortunusAdiutor.Data;

/// <summary>
/// 	Base class for the Entity Framework database context.
/// </summary>
///
/// <typeparam name="TUser">
/// 	Type of the user.
/// </typeparam>
///
/// <typeparam name="TKey">
/// 	Type of the user primary key.
/// </typeparam>
public class ManagedUserDbContext<TUser, TKey> : DbContext
where TUser : class, IManagedUser<TUser, TKey>
where TKey : IEquatable<TKey>
{
#pragma warning disable CS8618
	/// <summary>
	/// 	Initializes a new instance of the class.
	/// </summary>
	///
	/// <param name="options">
	/// 	Options to be used by the <see cref="DbContext"/>.
	/// </param>
	public ManagedUserDbContext(DbContextOptions options) : base(options)
	{
	}
#pragma warning restore CS8618

	/// <summary>
	/// 	Gets or sets the <see cref="DbSet{TEntity}"/> 
	/// 	containing all Users.
	/// </summary>
	public DbSet<TUser> Users { get; protected set; }

	/// <summary>
	/// 	Gets or sets the <see cref="DbSet{TEntity}"/> 
	/// 	containing all SingleUseTokens.
	/// </summary>
	public DbSet<SingleUseToken<TUser, TKey>> SingleUseTokens { get; protected set; }

	/// <inheritdoc/>
	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);
		var usrBuilder = builder.Entity<TUser>();

		usrBuilder.
			HasKey(e => e.Id);

		var sutBuilder = builder.Entity<SingleUseToken<TUser, TKey>>();

		sutBuilder
			.HasKey(e => e.Token);

		sutBuilder
			.HasOne<TUser>(e => e.User)
			.WithMany(e => e.SingleUseTokens);

	}
}