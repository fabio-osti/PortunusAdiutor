using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PortunusAdiutor.Data;
using PortunusAdiutor.Models;
using PortunusAdiutor.Services.MessagePoster;
using PortunusAdiutor.Services.TokenBuilder;

namespace PortunusAdiutor.Extensions;

/// <summary>
///     <see cref="WebApplicationBuilder" /> extensions for injecting the services.
/// </summary>
public static partial class WebBuilderExtensions
{
	/// <summary>
	///     Adds all services to the <see cref="ServiceCollection" />
	///     with <see cref="MessagePoster{TContext, TUser}" />.
	/// </summary>
	/// 
	/// <typeparam name="TContext">
	///     Type of the DbContext.
	/// </typeparam>
	/// 
	/// <typeparam name="TUser">
	///     Type of the user.
	/// </typeparam>
	/// 
	/// <param name="builder">
	///     The web app builder.
	/// </param>
	/// 
	/// <param name="contextConfigurator">
	///     The configurator for the <typeparamref name="TContext" />.
	/// </param>
	/// 
	/// <param name="tokenBuilderParams">
	///     The parameters used by the <see cref="TokenBuilder" />.
	/// </param>
	/// 
	/// <param name="mailCodePosterParams">
	///     The parameters used by the
	///     <see cref="MessagePoster{TContext, TUser}" />.
	/// </param>
	/// 
	/// <returns>
	///     An <see cref="AuthenticationBuilder" /> for further configurations.
	/// </returns>
	public static AuthenticationBuilder AddAllPortunusServices<TContext, TUser>(
		this WebApplicationBuilder builder,
		Action<DbContextOptionsBuilder> contextConfigurator,
		TokenBuilderParams tokenBuilderParams,
		MessagePosterParams mailCodePosterParams
	)
		where TContext : ManagedUserDbContext<TUser>
		where TUser : class, IManagedUser<TUser>
	{
		builder.Services.AddDbContext<TContext>(
			contextConfigurator,
			ServiceLifetime.Singleton
		);

		var authenticationBuilder = builder.AddTokenBuilder(tokenBuilderParams);
		builder.AddMessagePoster<TContext, TUser>(mailCodePosterParams);
		builder.AddUsersManager<TContext, TUser>();
		return authenticationBuilder;
	}
}