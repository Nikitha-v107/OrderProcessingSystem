using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace Order.Api.Controllers
{
    [ApiController]
    [Route("Authentication")] // Change route to just "Authentication"
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(IConfiguration configuration, ILogger<AuthenticationController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        [AllowAnonymous] // Allow anonymous access to the login endpoint
        public IActionResult Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Login attempt for username: {Username}", request.username);

            if (string.IsNullOrWhiteSpace(request.username) || string.IsNullOrWhiteSpace(request.password))
            {
                _logger.LogWarning("Login failed: Username or password cannot be empty.");
                return Unauthorized("Username and password are required.");
            }

            // Hardcoded credentials for demonstration
            if (request.username == "Nikitha" && request.password == "Siri@0900")
            {
                var jwtSettings = _configuration.GetSection("Jwt");
                var keyString = jwtSettings["Key"] ?? throw new ArgumentNullException("Jwt:Key is not configured.");
                var issuer = jwtSettings["Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer is not configured.");
                var audience = jwtSettings["Audience"] ?? throw new ArgumentNullException("Jwt:Audience is not configured.");

                var key = Encoding.ASCII.GetBytes(keyString);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, request.username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Name, request.username) // Unique name claim
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(60), // Token valid for 60 minutes
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogInformation("User {Username} logged in successfully, token issued.", request.username);
                return Ok(new { token = tokenString, expires = tokenDescriptor.Expires });
            }

            _logger.LogWarning("Invalid login attempt for username: {Username}", request.username);
            return Unauthorized("Invalid username or password.");
        }
    }

    public class LoginRequest
    {
        public required string username { get; set; }
        public required string password { get; set; }
    }
}
