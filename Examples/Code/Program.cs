using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PortunusAdiutor.Extensions;
using PortunusAdiutor.Services.MessagePoster;
using PortunusAdiutor.Services.TokenBuilder;
using PortunusAdiutor.Static;
using PortunusCodeExample.Data;
using PortunusCodeExample.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddAllPortunusServices<ApplicationDbContext, ApplicationUser, Guid>(
	options => options.UseSqlite("Data Source=app.db"),
	new TokenBuilderParams {
		SigningKey =
			new SymmetricSecurityKey(Convert.FromBase64String("7SOQv9BtXmZyiGXBqqGlUhBp1VS3mh8d6bf4epaPQNc=")),
		EncryptionKey =
			new SymmetricSecurityKey(Convert.FromBase64String("6BBJvRT7Pa9t7BSeq2yaHaZ78HkQzdnI1e1mgeLQ9Ds=")),
		ValidationParams = new TokenValidationParameters {
			ValidateAudience = false,
			ValidateIssuer = false
		}
	},
	new CodeMessagePosterParams() {
		SmtpUri = new("smtp://smtp4dev:25")
	}
);
builder.Services.AddAuthorization(
	opt => opt.AddPolicy(
		"Administrator",
		policy => policy
			.RequireClaim("is-admin", "True")
			.RequireClaim(JwtCustomClaims.EmailConfirmed, "True")
	)
);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope()) {
	var services = scope.ServiceProvider;

	var context = services.GetRequiredService<ApplicationDbContext>();
	if (context.Database.GetPendingMigrations().Any()) {
		context.Database.Migrate();
	}
}

app.Run();
