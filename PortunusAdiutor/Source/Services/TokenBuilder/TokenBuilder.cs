using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace PortunusAdiutor.Services.TokenBuilder;

/// <summary>
///     Default implementation of <see cref="ITokenBuilder" />.
/// </summary>
public class TokenBuilder : ITokenBuilder
{
	/// <summary>
	///     Initializes an instance of the class.
	/// </summary>
	/// 
	/// <param name="builderParams">
	///     Parameters for building and validating tokens.
	/// </param>
	public TokenBuilder(TokenBuilderParams builderParams)
	{
		_builderParams = builderParams;
	}

	private readonly TokenBuilderParams _builderParams;

	/// <inheritdoc />
	public string BuildToken(SecurityTokenDescriptor tokenDescriptor)
	{
		tokenDescriptor.SigningCredentials = new(
			_builderParams.SigningKey,
			SecurityAlgorithms.HmacSha256Signature
		);

		tokenDescriptor.EncryptingCredentials = new(
			_builderParams.EncryptionKey,
			JwtConstants.DirectKeyUseAlg,
			SecurityAlgorithms.Aes128CbcHmacSha256
		);

		var handler = new JwtSecurityTokenHandler();
		return handler.WriteToken(handler.CreateToken(tokenDescriptor));
	}

	/// <inheritdoc />
	public string BuildToken(Claim[] claims)
	{
		var tokenDescriptor = new SecurityTokenDescriptor {
			Expires = DateTime.UtcNow.Add(_builderParams.ExpirationTime),
			Subject = new(claims)
		};

		return BuildToken(tokenDescriptor);
	}

	/// <inheritdoc />
	public string BuildToken(IDictionary<string, object> claims)
	{
		var tokenDescriptor = new SecurityTokenDescriptor {
			Expires = DateTime.UtcNow.Add(_builderParams.ExpirationTime),
			Claims = claims
		};

		return BuildToken(tokenDescriptor);
	}

	/// <inheritdoc />
	public Claim[]? ValidateToken(
		string token,
		TokenValidationParameters? validationParameters = null
	)
	{
		try {
			validationParameters ??= new() {
				ValidateIssuer = false,
				ValidateAudience = false
			};

			validationParameters.IssuerSigningKey = _builderParams.SigningKey;

			validationParameters.TokenDecryptionKey =
				_builderParams.EncryptionKey;

			var handler = new JwtSecurityTokenHandler();

			var claims = handler.ValidateToken(
				token,
				validationParameters,
				out var tokenSecure
			);

			return claims.Claims.ToArray();
		} catch {
			return null;
		}
	}
}