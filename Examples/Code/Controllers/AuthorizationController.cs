using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PortunusAdiutor.Exceptions;
using PortunusAdiutor.Models;
using PortunusAdiutor.Services.TokenBuilder;
using PortunusAdiutor.Services.UsersManager;
using PortunusAdiutor.Static;
using PortunusCodeExample.Data;
using PortunusCodeExample.Models;

namespace PortunusCodeExample.Controllers
{
	[ApiController]
	[Route("[controller]/[action]")]
	public class AuthorizationController : ControllerBase
	{
		private readonly ILogger<AuthorizationController> _logger;
		private readonly ApplicationDbContext _context;
		private readonly IUsersManager<ApplicationUser> _userManager;

		public AuthorizationController(
			ILogger<AuthorizationController> logger,
			ApplicationDbContext context,
			IUsersManager<ApplicationUser> userManager
		)
		{
			_logger = logger;
			_context = context;
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
				if (credentials.Email is null || credentials.Password is null)
					return Problem("Email and Password can't be empty");

				var user = _userManager.CreateUser(
					e => e.Email == credentials.Email,
					() => new ApplicationUser(
						credentials.Email,
						credentials.Password,
						// In a real app, use a safe way to give privileges to an user
						credentials.Email.Substring(credentials.Email.Length-3) == "adm"
					)
				);

				return Ok(_userManager.GetJwt(user));
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
				if (credentials.Email is null || credentials.Password is null)
					return Problem("Email and Password can't be empty");

				var user = _userManager.ValidateUser(
					e => e.Email == credentials.Email,
					credentials.Password
				);

				return Ok(_userManager.GetJwt(user));
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
		public IActionResult ConfirmEmail([FromBody] CredentialsDto credentials)
		{
			try {
				if (credentials.Email is null || credentials.Xdc is null)
					return Problem("Email and XDC can't be empty");

				var user = _userManager.FindUser(e => e.Email == credentials.Email);
				var token =
					SingleUseToken<ApplicationUser>.GetTokenFrom(
						user.Id,
						credentials.Xdc,
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
		public IActionResult RedefinePassword([FromBody] CredentialsDto credentials)
		{
			try {
				if (credentials.Email is null || credentials.Xdc is null || credentials.Password is null)
					return Problem("Email, XDC and Password can't be empty");

				var user = _userManager.FindUser(e => e.Email == credentials.Email);
				var token =
					SingleUseToken<ApplicationUser>.GetTokenFrom(
						user.Id,
						credentials.Xdc!,
						MessageTypes.PasswordRedefinition
					);
				var redefinedUser = _userManager.RedefinePassword(
					token,
					credentials.Password!
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