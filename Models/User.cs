class User
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    required public string Email { get; set; }
    required public string Password { get; set; }
}