using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FisaBank.Api.Data;
using FisaBank.Api.Models;

namespace FisaBank.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly FisaBankDbContext _db;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(FisaBankDbContext db, ILogger<PaymentsController> logger)
    {
        _db     = db;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetMyPayments()
    {
        try
        {
            var userId   = GetCurrentUserId();
            var payments = _db.Payments
                .Where(p => p.OwnerId == userId)
                .Select(p => MapToResponse(p))
                .ToList();
            return Ok(payments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse { Message = ex.Message, StackTrace = ex.StackTrace, Source = ex.Source });
        }
    }

    [HttpGet("{id:int}")]
    public IActionResult GetPaymentById(int id)
    {
        try
        {
            var payment = _db.Payments.Find(id);
            if (payment == null)
                return NotFound(new ErrorResponse { Message = $"Pago {id} no encontrado." });

            return Ok(MapToResponse(payment));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse { Message = ex.Message, StackTrace = ex.StackTrace, Source = ex.Source });
        }
    }

    [HttpPost]
    public IActionResult CreatePayment([FromBody] CreatePaymentRequest request)
    {
        try
        {
            var payment = new Payment
            {
                OwnerId          = request.OwnerId,
                Description      = request.Description,
                Amount           = request.Amount,
                Currency         = request.Currency,
                RecipientName    = request.RecipientName,
                RecipientAccount = request.RecipientAccount,
                Status           = request.Status,
                CreatedAt        = DateTime.UtcNow,
            };

            _db.Payments.Add(payment);
            _db.SaveChanges();

            _logger.LogInformation("Pago creado ID={Id}", payment.Id);
            return Created($"/api/payments/{payment.Id}", MapToResponse(payment));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse { Message = ex.Message, StackTrace = ex.StackTrace, Source = ex.Source });
        }
    }

    [HttpPatch("{id:int}/status")]
    public IActionResult UpdatePaymentStatus(int id, [FromBody] UpdatePaymentRequest request)
    {
        try
        {
            var payment = _db.Payments.Find(id);
            if (payment == null)
                return NotFound(new ErrorResponse { Message = $"Pago {id} no encontrado." });

            payment.Status = request.Status;

            if (request.Status == "completed")
                payment.ProcessedAt = DateTime.UtcNow;

            if (request.Amount.HasValue)
                payment.Amount = request.Amount.Value;

            if (request.RecipientAccount is not null)
                payment.RecipientAccount = request.RecipientAccount;

            if (request.RecipientName is not null)
                payment.RecipientName = request.RecipientName;

            _db.SaveChanges();
            _logger.LogInformation("Pago {Id}: status={Status}", id, request.Status);

            return Ok(MapToResponse(payment));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse { Message = ex.Message, StackTrace = ex.StackTrace, Source = ex.Source });
        }
    }

    [HttpDelete("{id:int}")]
    public IActionResult DeletePayment(int id)
    {
        try
        {
            var payment = _db.Payments.Find(id);
            if (payment == null)
                return NotFound(new ErrorResponse { Message = $"Pago {id} no encontrado." });

            _db.Payments.Remove(payment);
            _db.SaveChanges();
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse { Message = ex.Message, StackTrace = ex.StackTrace, Source = ex.Source });
        }
    }

    private int GetCurrentUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return int.TryParse(sub, out var id) ? id : 0;
    }

    private static PaymentResponse MapToResponse(Payment p) => new()
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
