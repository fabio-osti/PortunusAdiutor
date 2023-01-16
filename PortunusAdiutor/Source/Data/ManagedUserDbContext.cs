using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using PortunusAdiutor.Exceptions;
using PortunusAdiutor.Models;
using PortunusAdiutor.Static;

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

	/// <summary>
	/// 	Generates a <see cref="SingleUseToken{TUser, TKey}"/> for an 
	/// 	<see cref="IManagedUser{TUser, TKey}"/> for an access of type 
	/// 	<paramref name="type"/> and saves it on the database.
	/// </summary>
	///
	/// <param name="userId">
	/// 	Id of the <see cref="IManagedUser{TUser, TKey}"/>.
	/// </param>
	///
	/// <param name="type">
	/// 	Type of access granted by the the returning SUT.
	/// </param>
	///
	/// <param name="xdc">
	/// </param>
	///
	/// <returns>
	/// 	The generated <see cref="SingleUseToken{TUser, TKey}"/>.
	/// </returns>
	public SingleUseToken<TUser, TKey> GenAndSaveSingleUseToken(
		TKey userId,
		MessageType type,
		out string xdc
	)
	{
		xdc = RandomNumberGenerator.GetInt32(1000000).ToString("000000");

		var userSut = new SingleUseToken<TUser, TKey>(userId, xdc, type.ToTypeString());

		SingleUseTokens.Add(userSut);
		SaveChanges();

		return userSut;
	}

	/// <summary>
	/// 	Consumes a sent message.
	/// </summary>
	///
	/// <param name="token">
	/// 	The access key sent by the message.
	/// </param>
	///
	/// <param name="messageType">
	/// 	The type of message that was sent.
	/// </param>
	///
	/// <returns>
	/// 	The key of the user to whom the token gives access to.
	/// </returns>
	public TKey ConsumeSut(
		string token,
		MessageType messageType
	)
	{
		var userSut =
			SingleUseTokens.Find(token);

		if (userSut is null) {
			throw new TokenNotFoundException();
		}

		var type = messageType.ToTypeString();
		if (userSut.ExpiresOn < DateTime.UtcNow || userSut.Type != type) {
			throw new InvalidPasswordException();
		}

		SingleUseTokens.Remove(userSut);
		SaveChanges();

		return userSut.UserId;
	}
}