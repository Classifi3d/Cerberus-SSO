using Application.Abstraction;
using Application.Abstraction.CQRS;
using Application.DTOs;
using CSharpFunctionalExtensions;
using Domain.Entities.User;

namespace Application.CommandsAndQueries.Users;

public sealed record DeleteUserCommand(Guid UserId) : ICommand;

public sealed class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserMapper _mapper;

    public DeleteUserCommandHandler(
        IUnitOfWork unitOfWork,
        UserMapper mapper)
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

        var userEvent = _mapper.ToDeleteEvent(user);

        _unitOfWork.AddOutboxEvent(userEvent);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("User deleted");
    }
}
