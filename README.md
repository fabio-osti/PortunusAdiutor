# PortunusAdiutor

An identity-less helper with setting up JWT authorization.

## Creating a DbContext and User model

The user model must implement IManagedUser\<TUser>, the recommended way is through inheritance of any implementation provided by this library.

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

The context must inherit from ManagedUserDbContext\<TUser>.

Example:
```csharp
public class ApplicationDbContext : ManagedUserDbContext<ApplicationUser, Guid>
{
	public ApplicationDbContext(DbContextOptions options) 
		: base(options) { }
}
```

## Adding it to your app

The recommended way of providing the services to your app is through the AddAllPortunusServices\<TContext, TUser> extensions method on the app builder.

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

To use it, inject the IUserManager\<TUser> and ITokenBuilder.

Invalid user interactions (such as signing in with the wrong password or signing up with a duplicate email) will return an UserResult containing the corresponding status error.

As most operations can (and should) be done with IUserManager, there's no need to ever use the DbContext directly for individual user operations.

Example:

```csharp
[ApiController]
[Route("[controller]/[action]")]
public class AuthorizationController : ControllerBase
{
	public AuthorizationController(
		ILogger<AuthorizationController> logger,
		IUsersManager<ApplicationUser> manager
	)
	{
		_logger = logger;
		_manager = manager;
	}

	private readonly ILogger<AuthorizationController> _logger;
	private readonly IUsersManager<ApplicationUser> _manager;

	[HttpPost]
	public IActionResult SignUp([FromBody] CredentialsDto credentials)
	{
		try {
			if (credentials.Email is null || credentials.Password is null)
				return Problem("Email and Password can't be empty");

			// In a real app, use a safe way to give privileges to an user
			var userResult = _manager.CreateUser(
				e => e.Email == credentials.Email,
				() => new(
					credentials.Email,
					credentials.Password,
					credentials.Email[^3..] == "adm"
				)
			);

			return (userResult.Status == UserResultStatus.Ok) 
				? Ok(_manager.GetJwt(userResult.User)) 
				: Problem(userResult.Status.GetDescription());

		} catch (Exception e) {
			_logger.LogError(e, "An error has occurred.");
			return Problem();
		}
	}

	[HttpPost]
	public IActionResult SignIn([FromBody] CredentialsDto credentials)
	{
		try
		{
			if (credentials.Email is null || credentials.Password is null)
				return Problem("Email and Password can't be empty");

			var userResult = _manager.ValidateUser(
				e => e.Email == credentials.Email,
				credentials.Password,
				credentials.Xdc
			);

			return userResult.Status switch {
				UserResultStatus.TwoFactorRequired => Ok(0),
				UserResultStatus.Ok => Ok(_manager.GetJwt(userResult.User)),
				_ => Problem(userResult.Status.GetDescription())
			};
		} catch (Exception e) {
			_logger.LogError(e, "An error has occurred.");
			return Problem();
		}
	}

	[HttpPost]
	public IActionResult ConfirmEmail([FromBody] CredentialsDto credentials)
	{
		try {
			if (credentials.Email is null || credentials.Xdc is null)
				return Problem("Email and XDC can't be empty");

			var result = _manager.ConfirmEmail(
				e => e.Email == credentials.Email,
				credentials.Xdc
			);

			return (result.Status == UserResultStatus.Ok) 
				? Ok() 
				: Problem(result.Status.GetDescription());
		} catch (PortunusException e) {
			return Problem(e.ShortMessage);
		} catch (Exception e) {
			_logger.LogError(e, "An error has occurred.");
			return Problem();
		}
	}

	[HttpPost]
	public IActionResult RedefinePassword([FromBody] CredentialsDto credentials)
	{
		try {
			if (credentials.Email is null
			    || credentials.Xdc is null
			    || credentials.Password is null)
				return Problem("Email, XDC and Password can't be empty");

			var result = _manager.RedefinePassword(
				e => e.Email == credentials.Email,
				credentials.Xdc,
				credentials.Password!
			);

			return (result.Status == UserResultStatus.Ok) 
				? Ok() 
				: Problem(result.Status.GetDescription());
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
			var result = _manager.SendEmailConfirmation(e => e.Email == email);

			return (result.Status == UserResultStatus.Ok) 
				? Ok() 
				: Problem(result.Status.GetDescription());
		} catch (Exception e) {
			_logger.LogError(e, "An error has occurred.");
			return Problem();
		}
	}

	[HttpPost]
	public IActionResult SendPasswordRedefinition(string email)
	{
		try {
			var result =
				_manager.SendPasswordRedefinition(e => e.Email == email);

			return (result.Status == UserResultStatus.Ok) 
				? Ok() 
				: Problem(result.Status.GetDescription());
		} catch (Exception e) {
			_logger.LogError(e, "An error has occurred.");
			return Problem();
		}
	}
}
```

## UserTokens and IMessagePoster

For password redefinition, email confirmation and two-factor authentication a message will be sent to the user email containing a token (a 6-digit number for now, but should be configurable later).