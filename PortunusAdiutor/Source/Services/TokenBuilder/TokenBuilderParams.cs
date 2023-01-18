using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace PortunusAdiutor.Services.TokenBuilder;

/// <summary>
/// 	Representation of the parameters for building and validating tokens.
/// </summary>
///
/// <remarks>
/// 	<see cref="SigningKey"/> and <see cref="EncryptionKey"/>
///
/// </remarks>
public class TokenBuilderParams
{
	/// <summary>
	/// 	Initialize an instance of <see cref="TokenBuilderParams"/>.
	/// </summary>
	public TokenBuilderParams() { }

	/// <summary>
	/// 	Initialize an instance of <see cref="TokenBuilderParams"/>
	/// 	using an <see cref="IConfiguration"/> object as base and
	///		<see cref="Encoding.GetBytes(string)"/> to extract the bytes.
	/// </summary>
	/// 
	/// <param name="config">
	/// 	An <see cref="IConfiguration"/> instance that 
	/// 	have the section "JWT" defined.
	/// </param>
	/// 
	/// <remarks>
	/// 	<list type="table">
	/// 		<listheader>
	/// 			<term>Property</term>
	/// 			<description>Configuration Key</description>
	/// 		</listheader>
	/// 		<item>
	/// 			<term><see cref="SigningKey"/></term>
	/// 			<description>"SSK"</description>
	/// 		</item>
	/// 		<item>
	/// 			<term><see cref="EncryptionKey"/></term>
	/// 			<description>"SEK"</description>
	/// 		</item>
	/// 	</list>
	/// </remarks>
	/// 
	/// <exception cref="ArgumentNullException">
	/// 	Thrown if "SSK" or "SEK" are not defined on the "JWT" section
	/// 	of <paramref name="config"/>
	/// </exception>
	[SetsRequiredMembers]
	public TokenBuilderParams(IConfiguration config)
		: this(config, Encoding.Unicode.GetBytes) { }

	/// <summary>
	/// 	Initialize an instance of <see cref="TokenBuilderParams"/>
	/// 	using an <see cref="IConfiguration"/> object as base and
	///		<paramref name="converter"/> to extract the bytes.
	/// </summary>
	/// 
	/// <param name="config">
	/// 	An <see cref="IConfiguration"/> instance that 
	/// 	have the section "JWT" defined.
	/// </param>
	/// 
	/// <param name="converter">
	/// 	Function to extract the bytes from the security key string.
	/// </param>
	/// 
	/// <remarks>
	/// 	<list type="table">
	/// 		<listheader>
	/// 			<term>Property</term>
	/// 			<description>Configuration Key</description>
	/// 		</listheader>
	/// 		<item>
	/// 			<term><see cref="SigningKey"/></term>
	/// 			<description>"SSK"</description>
	/// 		</item>
	/// 		<item>
	/// 			<term><see cref="EncryptionKey"/></term>
	/// 			<description>"SEK"</description>
	/// 		</item>
	/// 	</list>
	/// </remarks>
	/// 
	/// <exception cref="ArgumentNullException">
	/// 	Thrown if "SSK" or "SEK" are not defined on the "JWT" section
	/// 	of <paramref name="config"/>
	/// </exception>
	[SetsRequiredMembers]
	public TokenBuilderParams(IConfiguration config, Func<string, byte[]> converter)
	{
		var sect = config.GetSection("JWT");
		SigningKey = new SymmetricSecurityKey(
			converter(sect["SSK"] ?? throw new ArgumentNullException($"{nameof(config)}[\"JWT__SSK\"]"))
		);
		EncryptionKey = new SymmetricSecurityKey(
			converter(sect["SEK"] ?? throw new ArgumentNullException($"{nameof(config)}[\"JWT__SEK\"]"))
		);
	}

	/// <summary>
	/// 	Key for signing tokens.
	/// </summary>
	public required SymmetricSecurityKey SigningKey { get; set; }

	/// <summary>
	/// 	Key for encrypting tokens.
	/// </summary>
	public required SymmetricSecurityKey EncryptionKey { get; set; }

	/// <summary>
	///  	Configurator of <see cref="JwtBearerOptions"/>.
	/// </summary>
	///
	/// <remarks>
	/// 	If this is defined, <see cref="ValidationParams"/> will be ignored.
	/// </remarks>
	public Action<JwtBearerOptions>? JwtConfigurator { get; set; }

	/// <summary>
	/// 	Parameters for token validation.
	/// </summary>
	///
	/// <remarks>
	/// 	If <see cref="ValidationParams"/> is defined, this will be ignored.
	/// </remarks>
	public TokenValidationParameters? ValidationParams { get; set; }

	/// <summary>
	/// 	Time to be added to current UTC time when building a token.
	/// </summary>
	public TimeSpan ExpirationTime { get; set; } = defaultExpirationTime;

	static private readonly TimeSpan defaultExpirationTime = new(2, 0, 0);
}