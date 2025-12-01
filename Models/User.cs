public class User
{
    public int Id { get; set; }
    public Guid GuidId { get; set; }
    public string? Name { get; set; }
    required public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}