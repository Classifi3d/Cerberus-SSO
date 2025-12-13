using AuthenticationWebApplication.Enteties;
using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction.Messaging;
using MFAWebApplication.Abstraction.UnitOfWork;
using MFAWebApplication.Context;
using MFAWebApplication.Services;
using OtpNet;
using QRCoder;

namespace MFAWebApplication.CommandsAndQueries.Users;

public sealed record EnableMfaOfUserCommand( Guid userId ) : ICommand<byte[]>;

internal sealed class EnableMfaOfUserCommandHandler : ICommandHandler<EnableMfaOfUserCommand, byte[]>
{
    private readonly UnitOfWork<WriteDbContext> _unitOfWork;
    private readonly ISecurityService _securityService;
    private Guid userId;

    public EnableMfaOfUserCommandHandler( Guid userId )
    {
        this.userId = userId;
    }

    public EnableMfaOfUserCommandHandler( 
        UnitOfWork<WriteDbContext> unitOfWork, 
        ISecurityService securityService )
    {
        _unitOfWork = unitOfWork;
        _securityService = securityService;
    }

    public async Task<Result<byte[]>> Handle( EnableMfaOfUserCommand request, CancellationToken cancellationToken )
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.userId, cancellationToken);

        if ( user is null )
            return Result.Failure<byte[]>("User not found");

        if ( !string.IsNullOrEmpty(user.MfaSecretKey) )
            return Result.Failure<byte[]>("MFA already enabled for this user");


        var encodedMfaKey = _securityService.GenerateEncodedMfaKey();
        var qrBytes = _securityService.GenerateQRCode(encodedMfaKey, user.Email);

        if(qrBytes is null )
        {
            return Result.Failure<byte[]>("QR Code cannot be generated");
        }

        user.MfaSecretKey = encodedMfaKey;
        user.IsMfaEnabled = true;

        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);


        return Result.Success(qrBytes);
    }
}


