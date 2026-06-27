using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Jumia.Jumia.Domain.Models;
using Microsoft.IdentityModel.Tokens;

namespace Jumia.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;

        public TokenService(IConfiguration config)
        {
            _config = config;
            // Reading the secret key we added in appsettings.json
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"] ?? string.Empty));
        }

        public string CreateToken(AppUser user)
        {
            // 1. Creating Claims (User information inside the token)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.GivenName, user.DisplayName ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            // 2. Creating Credentials (Signing the token with our secret key)
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            // 3. Designing the Token Descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(double.Parse(_config["JWT:DurationInDays"] ?? "7")),
                SigningCredentials = creds,
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"]
            };

            // 4. Generating and returning the Token string
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}