using Microsoft.AspNetCore.Builder;

using Microsoft.Extensions.DependencyInjection;

using PortunusAdiutor.Data;
using PortunusAdiutor.Models;
using PortunusAdiutor.Services.UsersManager;

namespace PortunusAdiutor.Extensions;

public static partial class WebBuilderExtensions
{
	/// <summary>
	/// 	Adds <see cref="UsersManager{TContext, TUser}"/> 
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
	///
	/// <param name="builder">
	/// 	The web app builder.
	/// </param>
	public static void AddUsersManager<TContext, TUser>(this WebApplicationBuilder builder)
	where TContext : ManagedUserDbContext<TUser>
	where TUser : class, IManagedUser<TUser>
	{
		builder.Services
			.AddSingleton<IUsersManager<TUser>, UsersManager<TContext, TUser>>();
	}
}