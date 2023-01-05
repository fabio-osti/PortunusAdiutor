using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PortunusAdiutor.Exceptions;
using PortunusAdiutor.Models;
using PortunusAdiutor.Services.TokenBuilder;
using PortunusAdiutor.Services.UsersManager;
using PortunusAdiutor.Static;

using PortunusTester.Models;

namespace PortunusTester.Controllers
{
	[ApiController]
	[Route("[controller]/[action]")]
	public class AuthorizationController : ControllerBase
	{
		private readonly ILogger<AuthorizationController> _logger;
		private readonly ITokenBuilder _tokenBuilder;
		private readonly IUsersManager<ApplicationUser, Guid> _userManager;

		public AuthorizationController(
			ILogger<AuthorizationController> logger,
			ITokenBuilder tokenBuilder,
			IUsersManager<ApplicationUser, Guid> userManager
		)
		{
			_logger = logger;
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

		[HttpPost]
		public IActionResult SignUp([FromBody] CredentialsDto credentials)
		{
			try {
				var user = _userManager.CreateUser(
					e => e.Email == credentials.Email,
					() => new ApplicationUser(
						credentials.Email!,
						credentials.Password!
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
		public IActionResult SendEmailConfirmation([FromBody] CredentialsDto redefine)
		{
			try {
				var user =
					_userManager.SendEmailConfirmation(e => e.Email == redefine.Email);

				return Ok();
			} catch (PortunusException e) {
				return Problem(e.ShortMessage);
			} catch (Exception e) {
				_logger.LogError(e, "An error has occurred.");
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
				_logger.LogError(e, "An error has occurred.");
				return Problem();
			}
		}


		[HttpPost]
		public IActionResult ConfirmEmail([FromBody] CredentialsDto cred)
		{
			try {
				var user = _userManager.FindUser(e => e.Email == cred.Email);
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
				var user = _userManager.FindUser(e => e.Email == cred.Email);
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
}