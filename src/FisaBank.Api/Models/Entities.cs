namespace FisaBank.Api.Models;

public class User
{
    public int      Id             { get; set; }
    public string   Email          { get; set; } = string.Empty;
    public string   PasswordHash   { get; set; } = string.Empty;
    public string   FullName       { get; set; } = string.Empty;
    public string   SSN            { get; set; } = string.Empty;
    public decimal  AccountBalance { get; set; }
    public string   PhoneNumber    { get; set; } = string.Empty;
    public string   Address        { get; set; } = string.Empty;
    public bool     IsAdmin        { get; set; } = false;
    public DateTime CreatedAt      { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt   { get; set; }

    public ICollection<Payment>      Payments      { get; set; } = new List<Payment>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

public class Payment
{
    public int      Id               { get; set; }
    public int      OwnerId          { get; set; }
    public User     Owner            { get; set; } = null!;
    public string   Description      { get; set; } = string.Empty;
    public decimal  Amount           { get; set; }
    public string   Currency         { get; set; } = "COP";
    public string   Status           { get; set; } = "pending";
    public string   RecipientName    { get; set; } = string.Empty;
    public string   RecipientAccount { get; set; } = string.Empty;
    public DateTime  CreatedAt       { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt     { get; set; }
}

public class RefreshToken
{
    public int      Id        { get; set; }
    public int      UserId    { get; set; }
    public User     User      { get; set; } = null!;
    public string   Token     { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool     IsRevoked { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AuditLog
{
    public int      Id         { get; set; }
    public int?     UserId     { get; set; }
    public string   Action     { get; set; } = string.Empty;
    public string   Details    { get; set; } = string.Empty;
    public string?  StackTrace { get; set; }
    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
    public string   IpAddress  { get; set; } = string.Empty;
}
