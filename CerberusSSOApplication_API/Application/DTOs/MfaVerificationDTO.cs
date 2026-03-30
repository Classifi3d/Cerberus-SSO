namespace Application.DTOs;

public class MfaVerificationDTO
{
    public string? ChallengeId { get; set; }
    public string Code { get; set; } = string.Empty;
}

