using Application.Abstraction;
using Application.Abstraction.CQRS;
using Application.Abstraction.Services;
using Application.DTOs;
using CSharpFunctionalExtensions;
using Domain.Entities.User;

namespace Application.CommandsAndQueries.Users;

public sealed record LoginUserQuery(UserLoginDTO UserLoginDto) : IQuery<LoginSecurityDTO>;

public sealed class LoginUserQueryHandler : IQueryHandler<LoginUserQuery, LoginSecurityDTO>
{
    private readonly IReadModelRepository<UserReadModel> _userRepository;
    private readonly ISecurityService _securityService;
    private readonly ICacheService _cacheService;

    public LoginUserQueryHandler(
        IReadModelRepository<UserReadModel> userRepository,
        ISecurityService securityService,
        ICacheService cacheService)
    {
        _userRepository = userRepository;
        _securityService = securityService;
        _cacheService = cacheService;
    }

    public async Task<Result<LoginSecurityDTO>> Handle(LoginUserQuery request, CancellationToken cancellationToken)
    {
        var loginDto = request.UserLoginDto;

        var user = await _userRepository.GetByPropertyAsync(u => u.Email, loginDto.Email, cancellationToken);
        if (user == null)
        {
            return Result.Failure<LoginSecurityDTO>("Invalid credentials");
        }

        var isPasswordMatching = _securityService.CheckPassword(loginDto.Password,user.Password);
        if (!isPasswordMatching)
        {
            return Result.Failure<LoginSecurityDTO>("Invalid credentials");
        }

        if (!user.IsMfaEnabled)
        {
            if (!string.IsNullOrEmpty(loginDto.RequestId))
            {
                return await HandleOAuthLogin(user, loginDto.RequestId);
            }


            Guid.TryParse(user.Id, out Guid id);
            var token = _securityService.CreateJSONWebToken(id);
            return Result.Success(new LoginSecurityDTO
            {
                Token = token,
                RequiresMfa = false
            });
        }

        var challengeId = Guid.NewGuid().ToString();
        await _cacheService.SetAsync(
            $"mfa_challenge_{challengeId}",
            new { UserId = user.Id, RequestId = loginDto.RequestId },
            TimeSpan.FromMinutes(5)
        );

        var mfaResult = new LoginSecurityDTO
        {
            Token = null,
            RequiresMfa = true,
            ChallengeId = challengeId
        };

        return Result.Success(mfaResult);

    }

    private async Task<Result<LoginSecurityDTO>> HandleOAuthLogin(UserReadModel user, string requestId)
    {
        var oauthRequest = await _cacheService
            .GetAsync<AuthorizationRequestDTO>($"oauth_request_{requestId}");

        if (oauthRequest == null)
            return Result.Failure<LoginSecurityDTO>("Invalid OAuth request");

        // Generate authorization code
        var code = Guid.NewGuid().ToString();

        // The challenge and redirect uri travel with the code so the token exchange can
        // verify them against what was actually authorized, not against what the caller
        // re-sends.
        await _cacheService.SetAsync(
            $"auth_code_{code}",
            new AuthorizationCodeDTO
            {
                UserId = user.Id,
                ClientId = oauthRequest.ClientId,
                CodeChallenge = oauthRequest.CodeChallenge,
                RedirectUri = oauthRequest.RedirectUri
            },
            TimeSpan.FromMinutes(5)
        );

        var redirectUrl =
            $"{oauthRequest.RedirectUri}?code={code}&state={oauthRequest.State}";

        return Result.Success(new LoginSecurityDTO
        {
            RequiresMfa = false,
            RedirectUrl = redirectUrl
        });
    }

}

