namespace PortunusTester.Models
{
	public record CredentialsDto
	{
		public string? Email { get; init; }
		public string? Password { get; init; }
		public string? Xdc { get; init; }
	}
}