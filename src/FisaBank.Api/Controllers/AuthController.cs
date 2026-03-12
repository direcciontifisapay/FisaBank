using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FisaBank.Api.Data;
using FisaBank.Api.Models;
using FisaBank.Api.Services;

namespace FisaBank.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly FisaBankDbContext _db;
    private readonly JwtService _jwt;
    private readonly ILogger<AuthController> _logger;

    public AuthController(FisaBankDbContext db, JwtService jwt, ILogger<AuthController> logger)
    {
        _db     = db;
        _jwt    = jwt;
        _logger = logger;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == request.Email);

            if (user == null)
                return Unauthorized(new ErrorResponse { Message = "Credenciales inválidas." });

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized(new ErrorResponse { Message = "Credenciales inválidas." });

            user.LastLoginAt = DateTime.UtcNow;

            var (plain, hashed) = _jwt.GenerateRefreshToken();
            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId    = user.Id,
                Token     = hashed,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            });

            _db.SaveChanges();
            _logger.LogInformation("Login exitoso: {Email}", user.Email);

            return Ok(new LoginResponse
            {
                Token        = _jwt.GenerateAccessToken(user),
                RefreshToken = plain,
                User         = MapToProfile(user),
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse
            {
                Message        = ex.Message,
                StackTrace     = ex.StackTrace,
                InnerException = ex.InnerException?.Message,
                Source         = ex.Source,
            });
        }
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (_db.Users.Any(u => u.Email == request.Email))
                return Conflict(new ErrorResponse { Message = "Ya existe una cuenta con ese email." });

            var user = new User
            {
                Email          = request.Email,
                PasswordHash   = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName       = request.FullName,
                PhoneNumber    = request.PhoneNumber,
                Address        = request.Address,
                IsAdmin        = request.IsAdmin,
                AccountBalance = 0m,
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            return Created($"/api/users/{user.Id}", new LoginResponse
            {
                Token = _jwt.GenerateAccessToken(user),
                User  = MapToProfile(user),
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse
            {
                Message        = ex.Message,
                StackTrace     = ex.StackTrace,
                InnerException = ex.InnerException?.Message,
                Source         = ex.Source,
            });
        }
    }

    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] RefreshRequest request)
    {
        try
        {
            var stored = _db.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefault(rt =>
                    rt.Token == request.RefreshToken &&
                    !rt.IsRevoked &&
                    rt.ExpiresAt > DateTime.UtcNow);

            if (stored == null)
                return Unauthorized(new ErrorResponse { Message = "Refresh token inválido o expirado." });

            stored.IsRevoked = true;

            var (plain, hashed) = _jwt.GenerateRefreshToken();
            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId    = stored.UserId,
                Token     = hashed,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            });

            _db.SaveChanges();

            return Ok(new LoginResponse
            {
                Token        = _jwt.GenerateAccessToken(stored.User),
                RefreshToken = plain,
                User         = MapToProfile(stored.User),
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse
            {
                Message        = ex.Message,
                StackTrace     = ex.StackTrace,
                InnerException = ex.InnerException?.Message,
                Source         = ex.Source,
            });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout([FromBody] RefreshRequest request)
    {
        try
        {
            var token = _db.RefreshTokens
                .FirstOrDefault(rt => rt.Token == request.RefreshToken && !rt.IsRevoked);

            if (token != null)
            {
                token.IsRevoked = true;
                _db.SaveChanges();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse { Message = ex.Message, StackTrace = ex.StackTrace });
        }
    }

    private static UserProfileResponse MapToProfile(User u) => new()
    {
        Id             = u.Id,
        Email          = u.Email,
        FullName       = u.FullName,
        SSN            = u.SSN,
        AccountBalance = u.AccountBalance,
        PhoneNumber    = u.PhoneNumber,
        Address        = u.Address,
        IsAdmin        = u.IsAdmin,
        LastLoginAt    = u.LastLoginAt,
    };
}
