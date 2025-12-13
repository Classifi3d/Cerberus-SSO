
using AuthenticationWebApplication.Enteties;
using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction.Messaging;
using MFAWebApplication.Abstraction.UnitOfWork;
using MFAWebApplication.Context;
using MFAWebApplication.DTOs;
using MFAWebApplication.Services;
using Microsoft.Extensions.Caching.Memory;
using OtpNet;

namespace MFAWebApplication.CommandsAndQueries.Users;

public sealed record VerifyMfaOfUserQuery(MfaVerificationDTO verificationDto) : IQuery<string>;

internal sealed class VerifyMfaOfUserQueryHandler : IQueryHandler<VerifyMfaOfUserQuery, string>
{
    private readonly UnitOfWork<WriteDbContext> _unitOfWork;

    private readonly ISecurityService _securityService;
    private readonly ICacheService _cacheService;

    public VerifyMfaOfUserQueryHandler(
        UnitOfWork<WriteDbContext> unitOfWork,
        ISecurityService securityService,
        ICacheService cacheService
        )
    {
        _unitOfWork = unitOfWork;
        _securityService = securityService;
        _cacheService = cacheService;
    }

    public async Task<Result<string>> Handle(VerifyMfaOfUserQuery request, CancellationToken cancellationToken)
    {

        var verification = request.verificationDto;

        if (!_cacheService.TryGetValue($"mfa_challenge_{verification.ChallengeId}", out Guid userId))
        {
            return Result.Failure<string>("Challenge token expired or invalid");
        }

        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId, cancellationToken);

        if (user is null)
            return Result.Failure<string>("User not found");


        bool isTotpValid = _securityService.CheckTotp(user.MfaSecretKey, verification.Code);

        var token = _securityService.CreateJSONWebToken(user.Id);

        if (token is null || !isTotpValid)
        {
            return Result.Failure<string>("Invalid MFA code");
        }

        await _cacheService.RemoveAsync($"mfa_challenge_{verification.ChallengeId}");

        return Result.Success(token);
    } 
}
