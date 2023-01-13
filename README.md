# PortunusAdiutor

An identity-less helper with setting up JWT authorization.

## Creating a DbContext and User model

The user model must implement IManagedUser\<TUser, TKey>, the recommended way is trough inheritance of any implementation provided by this library.

Example:
```csharp
public class ApplicationUser : Pbkdf2User<ApplicationUser, Guid>
{
	// This constructor should only be used by EF for reconstruction of the CLR type
	public ApplicationUser(
		string email, 
		string password, 
		bool admin
	) : base(Guid.NewGuid(), email, password)
	{
		IsAdmin = admin;
	}

	public ApplicationUser(
		Guid id, 
		string email, 
		byte[] salt, 
		string passwordHash
	) : base(id, email, salt, passwordHash)
	{
	}

	public bool IsAdmin { get; set; }

	public override Claim[] GetClaims() => 
		base
			.GetClaims()
			.Append(new("is-admin", IsAdmin.ToString()))
			.ToArray();
}
```

The context must inherit from ManagedUserDbContext\<TUser, TKey>.

Example:
```csharp
public class ApplicationDbContext : ManagedUserDbContext<ApplicationUser, Guid>
{
	public ApplicationDbContext(DbContextOptions options) 
		: base(options) { }
}
```

## Adding it to your app

The recommended way of providing the services to your app is through the AddAllPortunusServices\<TContext, TUser, TKey> extensions method on the app builder.

Example:
```csharp
builder.AddAllPortunusServices<ApplicationDbContext, ApplicationUser, Guid>(
	options => options.UseSqlite("Data Source=app.db"),
	new TokenBuilderParams {
		// You should store your keys in a safe way
		SigningKey =
			new SymmetricSecurityKey(Convert.FromBase64String(
				"7SOQv9BtXmZyiGXBqqGlUhBp1VS3mh8d6bf4epaPQNc="
			)),
		EncryptionKey =
			new SymmetricSecurityKey(Convert.FromBase64String(
				"6BBJvRT7Pa9t7BSeq2yaHaZ78HkQzdnI1e1mgeLQ9Ds="
			)),
		ValidationParams = new TokenValidationParameters {
			ValidateAudience = false,
			ValidateIssuer = false
		}
	},
	new CodeMessagePosterParams() {
		SmtpUri = new("smtp://smtp4dev:25")
	}
);
```

You can also easily add custom policies:
```csharp
builder.Services.AddAuthorization(
	opt => opt.AddPolicy(
		"Administrator",
		policy => policy
			.RequireClaim("is-admin", "True")
			.RequireClaim(JwtCustomClaims.EmailConfirmed, "True")
	)
);
```

## Usage

To use it, inject the IUserManager\<TUser, TKey> and ITokenBuilder.

Exceptions caused by invalid user interactions (such as signing in with the wrong password or signing up with a duplicate email) are treated with PortunusException.

As most operations can (and should) be done with IUserManager, there's no need to ever use the DbContext directly for individual user operations.

Example:

```csharp
[ApiController]
[Route("[controller]/[action]")]
public class AuthorizationController : ControllerBase
{
	private readonly ILogger<AuthorizationController> _logger;
	private readonly ApplicationDbContext _context;
	private readonly ITokenBuilder _tokenBuilder;
	private readonly IUsersManager<ApplicationUser, Guid> _userManager;

	public AuthorizationController(
		ILogger<AuthorizationController> logger,
		ApplicationDbContext context,
		ITokenBuilder tokenBuilder,
		IUsersManager<ApplicationUser, Guid> userManager
	)
	{
		_logger = logger;
		_context = context;
		_tokenBuilder = tokenBuilder;
		_userManager = userManager;
	}

	[HttpGet]
	public IActionResult Ping()
	{
		return Ok(DateTime.UtcNow);
	}

	[HttpGet]
	[Authorize]
	public IActionResult WhoAmI()
	{
		return Ok(User.Claims.ToDictionary(e => e.Type, e => e.Value));
	}

	[HttpGet]
	[Authorize(Policy = "Administrator")]
	public IActionResult GetUsersCount()
	{
		return Ok(_context.Users.Count());
	}

	[HttpPost]
	public IActionResult SignUp([FromBody] CredentialsDto credentials)
	{
		try {
			ArgumentNullException
				.ThrowIfNullOrEmpty(credentials.Email);
			ArgumentNullException
				.ThrowIfNullOrEmpty(credentials.Password);
			
			// In a real app, use a safe way to give privileges to an user
			var user = _userManager.CreateUser(
				e => e.Email == credentials.Email,
				() => new ApplicationUser(
					credentials.Email,
					credentials.Password,
					credentials.Email
						.Substring(credentials.Email.Length-3) == "adm"
				)
			);

			return Ok(_tokenBuilder.BuildToken(user.GetClaims()));
		} catch (PortunusException e) {
			return Problem(e.ShortMessage);
		} catch (Exception e) {
			_logger.LogError(e, "An error has occurred.");
			return Problem();
		}
	}

	[HttpPost]
	public IActionResult SignIn([FromBody] CredentialsDto credentials)
	{
		try {
			var user = _userManager.ValidateUser(
				e => e.Email == credentials.Email,
				credentials.Password!
			);

			return Ok(_tokenBuilder.BuildToken(user.GetClaims()));
		} catch (PortunusException e) {
			return Problem(e.ShortMessage);
		} catch (Exception e) {
			_logger.LogError(e, "An error has occurred.");
			return Problem();
		}
	}

	[HttpPost]
	public IActionResult SendEmailConfirmation(string email)
	{
		try {
			var user =
				_userManager.SendEmailConfirmation(e => e.Email == email);

			return Ok();
		} catch (PortunusException e) {
			return Problem(e.ShortMessage);
		} catch (Exception e) {
			_logger.LogError(e, "An error has occurred.");
			return Problem();
		}
	}

	[HttpPost]
	public IActionResult SendPasswordRedefinition(string email)
	{
		try {
			var user =
				_userManager.SendPasswordRedefinition(e => e.Email == email);

			return Ok();
		} catch (PortunusException e) {
			return Problem(e.ShortMessage);
		} catch (Exception e) {
			_logger.LogError(e, "An error has occurred.");
			return Problem();
		}
	}


	[HttpPost]
	public IActionResult ConfirmEmail([FromBody] CredentialsDto cred)
	{
		try {
			var user = _userManager.FindUser(
				e => e.Email == cred.Email
			);

			// Rebuild the token that gives access to the email confirmation
			var token =
				SingleUseToken<ApplicationUser, Guid>.GetTokenFrom(
					user.Id,
					cred.Xdc!,
					MessageTypes.EmailConfirmation
				);
			var confirmedUser = _userManager.ConfirmEmail(token);

			return Ok();
		} catch (PortunusException e) {
			return Problem(e.ShortMessage);
		} catch (Exception e) {
			_logger.LogError(e, "An error has occurred.");
			return Problem();
		}
	}

	[HttpPost]
	public IActionResult RedefinePassword([FromBody] CredentialsDto cred)
	{
		try {
			var user = _userManager.FindUser(
				e => e.Email == cred.Email
			);

			// Rebuild the token that gives access to the password redefinition
			var token =
				SingleUseToken<ApplicationUser, Guid>.GetTokenFrom(
					user.Id,
					cred.Xdc!,
					MessageTypes.PasswordRedefinition
				);
			var redefinedUser = _userManager.RedefinePassword(
				token,
				cred.Password!
			);

			return Ok();
		} catch (PortunusException e) {
			return Problem(e.ShortMessage);
		} catch (Exception e) {
			_logger.LogError(e, "An error has occurred.");
			return Problem();
		}
	}
}
```

## SingleUseTokens and IMessagePoster

For password redefinition and email confirmation (and in the future, two-steps-authentication) this library provides two strategies to work with:

- CodeMessagePoster: which sends a message to the user email containing a 6 digits code that can be consumed, along the UserID and the type of message that was sent, to rebuild the SingleUseToken, that should be used within the app.
- LinkMessagePoster: which sends a link of an endpoint appended to the SingleUseToken directly that should perform the desired operation.

## See more

The examples given here are of the CodeMessagePoster strategy, you can see the full examples at the Tester folder on the project.