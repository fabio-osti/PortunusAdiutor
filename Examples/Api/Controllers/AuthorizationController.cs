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

				// In a real app, use a safe way to give privileges to an user
				var user = _userManager.CreateUser(
					e => e.Email == credentials.Email,
					() => new ApplicationUser(
						credentials.Email,
						credentials.Password,
						credentials.Email[^3..] == "adm"
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
					credentials.Password,
					credentials.Xdc
				);

				return Ok(_userManager.GetJwt(user));
			} catch (TwoFactorRequiredException) {
				// Yes, TwoFactorRequired best practices using exception for expected cases, will fix.
				_userManager.SendTwoFactorAuthentication(e => e.Email == credentials.Email);
				return Accepted();
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

				_userManager.ConfirmEmail(
					e => e.Email == credentials.Email,
					credentials.Xdc
				);

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

				_userManager.RedefinePassword(
					e => e.Email == credentials.Email,
					credentials.Xdc,
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