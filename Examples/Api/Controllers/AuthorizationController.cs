using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortunusAdiutor.Exceptions;
using PortunusAdiutor.Helpers;
using PortunusAdiutor.Services.UsersManager;
using PortunusCodeExample.Data;
using PortunusCodeExample.Models;

namespace PortunusCodeExample.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthorizationController : ControllerBase
{
	public AuthorizationController(
		ILogger<AuthorizationController> logger,
		ApplicationDbContext context,
		IUsersManager<ApplicationUser> manager
	)
	{
		_logger = logger;
		_context = context;
		_manager = manager;
	}

	private readonly ApplicationDbContext _context;
	private readonly ILogger<AuthorizationController> _logger;
	private readonly IUsersManager<ApplicationUser> _manager;

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
			var userResult = _manager.CreateUser(
				e => e.Email == credentials.Email,
				() => new(
					credentials.Email,
					credentials.Password,
					credentials.Email[^3..] == "adm"
				)
			);

			if (userResult.Status == UserResultStatus.Ok)
				return Ok(_manager.GetJwt(userResult.User));

			return Problem(userResult.Status.GetDescription());
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

			var userResult = _manager.ValidateUser(
				e => e.Email == credentials.Email,
				credentials.Password,
				credentials.Xdc
			);

			if (userResult.Status == UserResultStatus.TwoFactorRequired) {
				_manager.SendTwoFactorAuthentication(
					e => e.Email == credentials.Email
				);

				return Ok(0);
			}

			return Ok(_manager.GetJwt(userResult.User));
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

			if (result.Status == UserResultStatus.Ok) return Ok();
			return Problem(result.Status.GetDescription());
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

			if (result.Status == UserResultStatus.Ok) return Ok();
			return Problem(result.Status.GetDescription());
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

			if (result.Status == UserResultStatus.Ok) return Ok();
			return Problem(result.Status.GetDescription());
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

			if (result.Status == UserResultStatus.Ok) return Ok();
			return Problem(result.Status.GetDescription());
		} catch (PortunusException e) {
			return Problem(e.ShortMessage);
		} catch (Exception e) {
			_logger.LogError(e, "An error has occurred.");
			return Problem();
		}
	}
}