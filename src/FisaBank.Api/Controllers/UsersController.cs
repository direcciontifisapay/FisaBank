using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FisaBank.Api.Data;
using FisaBank.Api.Models;

namespace FisaBank.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly FisaBankDbContext _db;

    public UsersController(FisaBankDbContext db) => _db = db;

    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            var user   = _db.Users.Find(userId);
            if (user == null)
                return NotFound(new ErrorResponse { Message = "Usuario no encontrado." });
            return Ok(MapToProfile(user));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse { Message = ex.Message, StackTrace = ex.StackTrace });
        }
    }

    [HttpGet("{id:int}")]
    public IActionResult GetUserById(int id)
    {
        try
        {
            var user = _db.Users.Find(id);
            if (user == null)
                return NotFound(new ErrorResponse { Message = "Usuario no encontrado." });
            return Ok(MapToProfile(user));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse { Message = ex.Message, StackTrace = ex.StackTrace });
        }
    }

    private int GetCurrentUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return int.TryParse(sub, out var id) ? id : 0;
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

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly FisaBankDbContext _db;

    public AdminController(FisaBankDbContext db) => _db = db;

    [HttpGet("reports")]
    public IActionResult GetReport()
    {
        try
        {
            var users    = _db.Users.ToList();
            var payments = _db.Payments.OrderByDescending(p => p.CreatedAt).Take(50).ToList();

            return Ok(new AdminReportResponse
            {
                TotalUsers     = users.Count,
                TotalPayments  = _db.Payments.Count(),
                TotalVolume    = _db.Payments.Where(p => p.Status == "completed").Sum(p => p.Amount),
                AllUsers       = users.Select(MapToProfile),
                RecentPayments = payments.Select(MapToPayment),
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse { Message = ex.Message, StackTrace = ex.StackTrace });
        }
    }

    [HttpGet("users")]
    public IActionResult GetAllUsers()
    {
        try
        {
            return Ok(_db.Users.Select(MapToProfile).ToList());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse { Message = ex.Message, StackTrace = ex.StackTrace });
        }
    }

    [HttpDelete("users/{id:int}")]
    public IActionResult DeleteUser(int id)
    {
        try
        {
            var user = _db.Users.Find(id);
            if (user == null)
                return NotFound(new ErrorResponse { Message = "Usuario no encontrado." });
            _db.Users.Remove(user);
            _db.SaveChanges();
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse { Message = ex.Message, StackTrace = ex.StackTrace });
        }
    }

    [HttpGet("logs")]
    public IActionResult GetAuditLogs()
    {
        try
        {
            var logs = _db.AuditLogs.OrderByDescending(l => l.CreatedAt).Take(100).ToList();
            return Ok(logs);
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

    private static PaymentResponse MapToPayment(Payment p) => new()
    {
        Id               = p.Id,
        OwnerId          = p.OwnerId,
        Description      = p.Description,
        Amount           = p.Amount,
        Currency         = p.Currency,
        Status           = p.Status,
        RecipientName    = p.RecipientName,
        RecipientAccount = p.RecipientAccount,
        CreatedAt        = p.CreatedAt,
        ProcessedAt      = p.ProcessedAt,
    };
}
