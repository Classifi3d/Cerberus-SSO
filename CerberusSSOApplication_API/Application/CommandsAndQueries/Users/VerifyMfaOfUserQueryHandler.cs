
using Application.Abstraction;
using Application.Abstraction.CQRS;
using Application.Abstraction.Services;
using Application.DTOs;
using CSharpFunctionalExtensions;
using Domain.Entities.User;

namespace Application.CommandsAndQueries.Users;

public sealed record VerifyMfaOfUserQuery(MfaVerificationDTO verificationDto) : IQuery<LoginSecurityDTO>;

public sealed class VerifyMfaOfUserQueryHandler : IQueryHandler<VerifyMfaOfUserQuery, LoginSecurityDTO>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityService _securityService;
    private readonly ICacheService _cacheService;

    public VerifyMfaOfUserQueryHandler(
        IUnitOfWork unitOfWork,
        ISecurityService securityService,
        ICacheService cacheService
        )
    {
        _unitOfWork = unitOfWork;
        _securityService = securityService;
        _cacheService = cacheService;
    }

    public async Task<Result<LoginSecurityDTO>> Handle(VerifyMfaOfUserQuery request, CancellationToken cancellationToken)
    {

        var verification = request.verificationDto;

        var cached = await _cacheService.GetAsync<MfaChallengeDTO>(
            $"mfa_challenge_{verification.ChallengeId}"
        );


        if (cached is null)
        {
            return Result.Failure<LoginSecurityDTO>("Challenge token expired or invalid");
        }

        var user = await _unitOfWork.Repository<User>().GetByIdAsync(Guid.Parse(cached.UserId), cancellationToken);

        if (user is null)
        {
            return Result.Failure<LoginSecurityDTO>("User not found");
        }


        bool isTotpValid = _securityService.CheckTotp(user.MfaSecretKey!, verification.Code);


        if (!isTotpValid)
        {
            return Result.Failure<LoginSecurityDTO>("Invalid MFA code");
        }

        if (!string.IsNullOrEmpty(cached.RequestId))
        {
            return await HandleOAuthMfa(user, cached.RequestId);
        }

        var token = _securityService.CreateJSONWebToken(user.Id);

        await _cacheService.RemoveAsync($"mfa_challenge_{verification.ChallengeId}");

        return Result.Success(new LoginSecurityDTO
        {
            Token = token,
            RequiresMfa = false
        });
    }

    private async Task<Result<LoginSecurityDTO>> HandleOAuthMfa(
        User user,
        string requestId)
    {
        var oauthRequest = await _cacheService
            .GetAsync<AuthorizationRequestDTO>($"oauth_request_{requestId}");

        if (oauthRequest == null)
        {
            return Result.Failure<LoginSecurityDTO>("Invalid OAuth request");
        }

        var code = Guid.NewGuid().ToString();

        await _cacheService.SetAsync(
            $"auth_code_{code}",
            new AuthorizationCodeDTO
            {
                UserId = user.Id.ToString(),
                ClientId = oauthRequest.ClientId
            },
            TimeSpan.FromMinutes(5)
        );

        var redirectUrl =
            $"{oauthRequest.RedirectUri}?code={code}&state={oauthRequest.State}";

        await _cacheService.RemoveAsync($"mfa_challenge_{requestId}");

        return Result.Success(new LoginSecurityDTO
        {
            RedirectUrl = redirectUrl,
            RequiresMfa = false
        });
    }
}
