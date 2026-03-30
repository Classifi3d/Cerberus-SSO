namespace Application.DTOs;

public class UserLoginDTO
{
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? RequestId { get; set; }
}
