using Microsoft.AspNetCore.Builder;

using Microsoft.Extensions.DependencyInjection;

using PortunusAdiutor.Data;
using PortunusAdiutor.Models;
using PortunusAdiutor.Services.MessagePoster;

namespace PortunusAdiutor.Extensions;

public static partial class WebBuilderExtensions
{
	/// <summary>
	/// 	Adds <see cref="MessagePoster{TContext, TUser}"/>
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
	///
	/// <param name="mailParams">
	/// 	The parameters used by the 
	/// 	<see cref="MessagePoster{TContext, TUser}"/>.
	/// </param>
	public static void AddMessagePoster<TContext, TUser>(
		this WebApplicationBuilder builder,
		MessagePosterParams mailParams
	)
	where TContext : ManagedUserDbContext<TUser>
	where TUser : class, IManagedUser<TUser>
	
	{
		builder.Services.AddSingleton<IMessagePoster<TUser>>(
			e => new MessagePoster<TContext, TUser>(
				mailParams,
				e.GetRequiredService<TContext>()
			)
		);
	}
}
