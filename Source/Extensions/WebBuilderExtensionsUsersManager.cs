using Microsoft.AspNetCore.Builder;

using Microsoft.Extensions.DependencyInjection;

using PortunusAdiutor.Data;
using PortunusAdiutor.Models;
using PortunusAdiutor.Services.UsersManager;

namespace PortunusAdiutor.Extensions;

public static partial class WebBuilderExtensions
{
	/// <summary>
	/// 	Adds <see cref="UsersManager{TContext, TUser, TKey}"/> 
	/// 	to the <see cref="ServiceCollection"/>.
	/// </summary>
	///
	/// <typeparam name="TContext">
	/// 	Type of the DbContext.
	/// </typeparam>
	///
	/// <typeparam name="TUser">
	/// 	Type of the user.
	/// </typeparam>
	///
	/// <typeparam name="TKey">
	/// 	Type of the user primary key.
	/// </typeparam>
	///
	/// <param name="builder">
	/// 	The web app builder.
	/// </param>
	public static void AddUsersManager<TContext, TUser, TKey>(this WebApplicationBuilder builder)
	where TContext : ManagedUserDbContext<TUser, TKey>
	where TUser : class, IManagedUser<TUser, TKey>
	where TKey : IEquatable<TKey>
	{
		builder.Services
			.AddSingleton<IUsersManager<TUser, TKey>, UsersManager<TContext, TUser, TKey>>();
	}
}