namespace PortunusTester.Models
{
	public record CredentialsDto
	{
		public string? Email { get; init; }
		public string? Password { get; init; }
	}
}