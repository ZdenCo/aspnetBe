public sealed class IUserMetadata
{
    public string? email { get; set; }
    public bool? email_verified { get; set; }
    public bool? phone_verified { get; set; }
    public string? sub { get; set; }
}

public sealed class ITokenData
{
    public string email { get; set; }
    public string subject { get; set; }
}