using Application.Abstraction;
using Application.Abstraction.CQRS;
using Application.Abstraction.Services;
using CSharpFunctionalExtensions;
using Domain.Entities.User;

namespace Application.CommandsAndQueries.Users;

public sealed record EnableMfaOfUserCommand(Guid userId) : ICommand<byte[]>;

public sealed class EnableMfaOfUserCommandHandler : ICommandHandler<EnableMfaOfUserCommand, byte[]>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityService _securityService;

    public EnableMfaOfUserCommandHandler(
        IUnitOfWork unitOfWork,
        ISecurityService securityService)
    {
        _unitOfWork = unitOfWork;
        _securityService = securityService;
    }

    public async Task<Result<byte[]>> Handle(EnableMfaOfUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.userId, cancellationToken);

        if (user is null)
            return Result.Failure<byte[]>("User not found");

        if (!string.IsNullOrEmpty(user.MfaSecretKey))
            return Result.Failure<byte[]>("MFA already enabled for this user");


        var encodedMfaKey = _securityService.GenerateEncodedMfaKey();
        var qrBytes = _securityService.GenerateQRCode(encodedMfaKey, user.Email);

        if(qrBytes is null)
        {
            return Result.Failure<byte[]>("QR Code cannot be generated");
        }

        user.MfaSecretKey = encodedMfaKey;
        user.IsMfaEnabled = true;

        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success<byte[]>(qrBytes);
    }
}


