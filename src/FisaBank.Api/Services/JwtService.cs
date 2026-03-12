using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using FisaBank.Api.Models;

namespace FisaBank.Api.Services;

public class JwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config) => _config = config;

    public string GenerateAccessToken(User user)
    {
        var key   = GetSigningKey();
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new Claim("fullName",       user.FullName),
            new Claim("isAdmin",        user.IsAdmin.ToString().ToLower()),
            new Claim("ssn",            user.SSN),
            new Claim("accountBalance", user.AccountBalance.ToString("F2")),
            new Claim("phoneNumber",    user.PhoneNumber),
            new Claim("address",        user.Address),
        };

        var token = new JwtSecurityToken(
            issuer:             "fisabank-api",
            audience:           "fisabank-clients",
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string plainToken, string hashedToken) GenerateRefreshToken()
    {
        var plain  = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hashed = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(plain)));
        return (plain, hashed);
    }

    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        var handler    = new JwtSecurityTokenHandler();
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer      = true,
            ValidIssuer         = "fisabank-api",
            ValidateAudience    = true,
            ValidAudience       = "fisabank-clients",
            ValidateLifetime    = true,
            IssuerSigningKey    = GetSigningKey(),
            RequireSignedTokens = false,
        };

        try   { return handler.ValidateToken(token, parameters, out _); }
        catch { return null; }
    }

    private SymmetricSecurityKey GetSigningKey()
    {
        var secret = _config["Jwt:Secret"] ?? "FisaBankSuperSecretKey2026!@#$%^";
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }
}
