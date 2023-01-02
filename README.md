# PortunsAdiutor

## Adding it to your app

```csharp
builder.AddAllPortunusServices<ChroniclesDbContext, XandriaUser, Guid>(
	(e) => e.UseSqlite("Data Source=app.db;"),
	new()
	{
		SigningKey = new(Encoding.UTF8.GetBytes("BeautifulKeyUsedToSignThoseMostSecureTokens")),
		EncryptionKey = new(Encoding.UTF8.GetBytes("BeautifulerKeyUsedToEncryptThoseMostSecureTokens")),
	},
	new MailCodePosterParams()
	{
		SmtpUri = new Uri("smtp://localhost:2525")
	}
);
```

## Usage

```csharp
[ApiController]
[Route("[controller]/[action]")]
public class AuthorizationController : ControllerBase
{
	private readonly ILogger<AuthorizationController> _logger;
	private readonly ITokenBuilder _tokenBuilder;
	private readonly IUsersManager<XandriaUser, Guid> _userManager;

	public AuthorizationController(
		ILogger<AuthorizationController> logger,
		ITokenBuilder tokenBuilder,
		IUsersManager<XandriaUser, Guid> userManager
	)
	{
		_logger = logger;
		_tokenBuilder = tokenBuilder;
		_userManager = userManager;
	}

	[HttpPost]
	public IActionResult SignUp([FromBody] CredentialsDto credentials)
	{
		try {
			var user = _userManager.CreateUser(
				e => e.Email == credentials.Email,
				() => new XandriaUser(
					credentials.Email!,
					credentials.Password!
				)
			);

			return Ok(_tokenBuilder.BuildToken(user.GetClaims()));
		} catch (PortunusException e) {
			return Problem(e.ShortMessage);
		} catch (Exception e) {
			_logger.LogError(
				e,
				"An error has occurred."
			);
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

			return user == null
				? Problem()
				: Ok(_tokenBuilder.BuildToken(user.GetClaims()));
		} catch (PortunusException e) {
			return Problem(e.ShortMessage);
		} catch (Exception e) {
			_logger.LogError(
				e,
				"An error has occurred."
			);
			return Problem();
		}
	}

	[HttpPost]
	public IActionResult SendEmailConfirmation([FromBody] CredentialsDto redefine)
	{
		try {
			var user =
				_userManager.SendEmailConfirmation(e => e.Email == redefine.Email);

			return Ok();
		} catch (PortunusException e) {
			return Problem(e.ShortMessage);
		} catch (Exception e) {
			_logger.LogError(
				e,
				"An error has occurred."
			);
			return Problem();
		}
	}

	[HttpPost]
	public IActionResult SendPasswordRedefinition([FromBody] CredentialsDto redefine)
	{
		try {
			var user =
				_userManager.SendPasswordRedefinition(e => e.Email == redefine.Email);

			return Ok();
		} catch (PortunusException e) {
			return Problem(e.ShortMessage);
		} catch (Exception e) {
			_logger.LogError(
				e,
				"An error has occurred."
			);
			return Problem();
		}
	}


	[HttpPost]
	public IActionResult ConfirmEmail([FromBody] CredentialsDto cred)
	{
		try {
			var user = _userManager.FindUser(e => e.Email == cred.Email);
			var token =
				SingleUseToken<XandriaUser, Guid>.GetTokenFrom(
					user.Id,
					cred.Xdc!,
					MessageTypes.EmailConfirmation
				);
			var confirmedUser = _userManager.ConfirmEmail(token);

			return Ok();
		} catch (PortunusException e) {
			return Problem(e.ShortMessage);
		} catch (Exception e) {
			_logger.LogError(
				e,
				"An error has occurred."
			);
			return Problem();
		}
	}

	[HttpPost]
	public IActionResult RedefinePassword([FromBody] CredentialsDto cred)
	{
		try {
			var user = _userManager.FindUser(e => e.Email == cred.Email);
			var token =
				SingleUseToken<XandriaUser, Guid>.GetTokenFrom(
					user.Id,
					cred.Xdc!,
					MessageTypes.EmailConfirmation
				);
			var confirmedUser = _userManager.RedefinePassword(
				token,
				cred.Password!
			);

			return Ok();
		} catch (PortunusException e) {
			return Problem(e.ShortMessage);
		} catch (Exception e) {
			_logger.LogError(
				e,
				"An error has occurred."
			);
			return Problem();
		}
	}
}
```