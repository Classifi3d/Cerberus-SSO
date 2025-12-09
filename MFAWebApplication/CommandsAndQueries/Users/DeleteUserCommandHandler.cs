using AuthenticationWebApplication.Enteties;
using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction.Messaging;
using MFAWebApplication.Abstraction.UnitOfWork;
using MFAWebApplication.Context;

namespace MFAWebApplication.CommandsAndQueries.Users;

public sealed record DeleteUserCommand(Guid UserId) : ICommand;


internal sealed class DeleteUserCommandHandler
    : ICommandHandler<DeleteUserCommand>
{
    private readonly UnitOfWork<WriteDbContext> _unitOfWork;

    public DeleteUserCommandHandler(UnitOfWork<WriteDbContext> unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {

        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.UserId);
        if (user is null)
        {
            return Result.Failure("User not found.");
        }

        _unitOfWork.Repository<User>().Delete(user);
        _unitOfWork.AddOutboxEvent(user);

        await _unitOfWork.SaveChangesAsync();

        return Result.Success("User deleted");
    }
}
