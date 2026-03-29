using AutoMapper;
using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction.Messaging;
using MFAWebApplication.Abstraction.UnitOfWork;
using MFAWebApplication.Context;
using MFAWebApplication.Entities.User;

namespace MFAWebApplication.CommandsAndQueries.Users;

public sealed record DeleteUserCommand(Guid UserId) : ICommand;


internal sealed class DeleteUserCommandHandler
    : ICommandHandler<DeleteUserCommand>
{
    private readonly UnitOfWork<WriteDbContext> _unitOfWork;
    private readonly Mapper _mapper;

    public DeleteUserCommandHandler(
        UnitOfWork<WriteDbContext> unitOfWork,
        Mapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {

        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure("User not found.");
        }

        _unitOfWork.Repository<User>().Delete(user);

        var userEvent = _mapper.Map<UserDeletedEvent>(user);
        _unitOfWork.AddOutboxEvent(userEvent);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("User deleted");
    }
}
