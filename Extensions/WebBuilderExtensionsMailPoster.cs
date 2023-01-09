using Microsoft.AspNetCore.Builder;

using Microsoft.Extensions.DependencyInjection;

using PortunusAdiutor.Data;
using PortunusAdiutor.Models;
using PortunusAdiutor.Services.MessagePoster;

namespace PortunusAdiutor.Extensions;

public static partial class WebBuilderExtensions
{
	/// <summary>
	/// 	Adds <see cref="CodeMessagePoster{TContext, TUser, TKey}"/>
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
	///
	/// <param name="mailParams">
	/// 	The parameters used by the 
	/// 	<see cref="CodeMessagePoster{TContext, TUser, TKey}"/>.
	/// </param>
	public static void AddMailCodePoster<TContext, TUser, TKey>(
		this WebApplicationBuilder builder,
		CodeMessagePosterParams mailParams
	)
	where TContext : ManagedUserDbContext<TUser, TKey>
	where TUser : class, IManagedUser<TUser, TKey>
	where TKey : IEquatable<TKey>
	{
		builder.Services.AddSingleton<IMessagePoster<TUser, TKey>>(
			e => new CodeMessagePoster<TContext, TUser, TKey>(
				mailParams,
				e.GetRequiredService<TContext>()
			)
		);
	}

	/// <summary>
	/// 	Adds <see cref="LinkMessagePoster{TContext, TUser, TKey}"/> 
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
	///
	/// <param name="mailParams">
	/// 	The parameters used by the 
	/// 	<see cref="LinkMessagePoster{TContext, TUser, TKey}"/>.
	/// </param>
	public static void AddMailLinkPoster<TContext, TUser, TKey>(
		this WebApplicationBuilder builder,
		LinkMessagePosterParams mailParams
	)
	where TContext : ManagedUserDbContext<TUser, TKey>
	where TUser : class, IManagedUser<TUser, TKey>	
	where TKey : IEquatable<TKey>
	{
		builder.Services.AddSingleton<IMessagePoster<TUser, TKey>>(
			e => new LinkMessagePoster<TContext, TUser, TKey>(
				mailParams,
				e.GetRequiredService<TContext>()
			)
		);
	}
}
