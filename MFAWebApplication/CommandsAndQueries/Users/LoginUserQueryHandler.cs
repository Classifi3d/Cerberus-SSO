using AuthenticationWebApplication.DTOs;
using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction.Messaging;
using MFAWebApplication.Abstraction.Repository;
using MFAWebApplication.DTOs;
using MFAWebApplication.Entities;
using MFAWebApplication.Services;

namespace MFAWebApplication.CommandsAndQueries.Users;

public sealed record LoginUserQuery(UserLoginDTO userLoginDto) : IQuery<LoginSecurityDTO>;

internal sealed class LoginUserQueryHandler : IQueryHandler<LoginUserQuery, LoginSecurityDTO>
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
        var loginDto = request.userLoginDto;
        var userEmail = loginDto.Email;

        var user = await _userRepository.GetByPropertyAsync(u => u.Email, userEmail, cancellationToken);
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
            Guid.TryParse(user.Id, out Guid id);
            var token = _securityService.CreateJSONWebToken(id);
            var result = new LoginSecurityDTO
            {
                Token = token,
                RequiresMfa = false,
                ChallengeId = null
            };

            return Result.Success(result);
        }

        var challengeId = Guid.NewGuid().ToString();
        await _cacheService.SetAsync($"mfa_challenge_{challengeId}", user.Id, TimeSpan.FromMinutes(5));

        var mfaResult = new LoginSecurityDTO
        {
            Token = null,
            RequiresMfa = true,
            ChallengeId = challengeId
        };

        return Result.Success(mfaResult);

    }

}