namespace FisaBank.Api.Models;

public class LoginRequest
{
    public string Email    { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token        { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string TokenType    { get; set; } = "Bearer";
    public int    ExpiresIn    { get; set; } = 3600;
    public UserProfileResponse User { get; set; } = null!;
}

public class RefreshRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Email       { get; set; } = string.Empty;
    public string Password    { get; set; } = string.Empty;
    public string FullName    { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address     { get; set; } = string.Empty;
    public bool   IsAdmin     { get; set; } = false;
}

public class CreatePaymentRequest
{
    public string  Description      { get; set; } = string.Empty;
    public decimal Amount           { get; set; }
    public string  Currency         { get; set; } = "COP";
    public string  RecipientName    { get; set; } = string.Empty;
    public string  RecipientAccount { get; set; } = string.Empty;
    public int     OwnerId          { get; set; }
    public string  Status           { get; set; } = "pending";
}

public class UpdatePaymentRequest
{
    public string   Status           { get; set; } = string.Empty;
    public decimal? Amount           { get; set; }
    public string?  RecipientAccount { get; set; }
    public string?  RecipientName    { get; set; }
}

public class PaymentResponse
{
    public int       Id               { get; set; }
    public int       OwnerId          { get; set; }
    public string    Description      { get; set; } = string.Empty;
    public decimal   Amount           { get; set; }
    public string    Currency         { get; set; } = string.Empty;
    public string    Status           { get; set; } = string.Empty;
    public string    RecipientName    { get; set; } = string.Empty;
    public string    RecipientAccount { get; set; } = string.Empty;
    public DateTime  CreatedAt        { get; set; }
    public DateTime? ProcessedAt      { get; set; }
}

public class UserProfileResponse
{
    public int      Id             { get; set; }
    public string   Email          { get; set; } = string.Empty;
    public string   FullName       { get; set; } = string.Empty;
    public string   SSN            { get; set; } = string.Empty;
    public decimal  AccountBalance { get; set; }
    public string   PhoneNumber    { get; set; } = string.Empty;
    public string   Address        { get; set; } = string.Empty;
    public bool     IsAdmin        { get; set; }
    public DateTime? LastLoginAt   { get; set; }
}

public class AdminReportResponse
{
    public int     TotalUsers    { get; set; }
    public int     TotalPayments { get; set; }
    public decimal TotalVolume   { get; set; }
    public IEnumerable<UserProfileResponse> AllUsers       { get; set; } = new List<UserProfileResponse>();
    public IEnumerable<PaymentResponse>     RecentPayments { get; set; } = new List<PaymentResponse>();
}

public class ErrorResponse
{
    public string  Message        { get; set; } = string.Empty;
    public string? StackTrace     { get; set; }
    public string? InnerException { get; set; }
    public string? Source         { get; set; }
}
