using Application.Abstraction;
using Application.Abstraction.CQRS;
using Application.Abstraction.Services;
using Application.DTOs;
using CSharpFunctionalExtensions;
using Domain.Entities.User;

namespace Application.CommandsAndQueries.Users;

public sealed record UpdateUserCommand(UserDTO User) : ICommand<UserDTO>;

public sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand,UserDTO>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityService _securityService;
    private readonly UserMapper _mapper;

    public UpdateUserCommandHandler(
        IUnitOfWork unitOfWork,
        UserMapper mapper,
        ISecurityService securityService)
    {
        _unitOfWork = unitOfWork;
        _securityService = securityService;
        _mapper = mapper;
    }

    public async Task<Result<UserDTO>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var userDto = request.User;

        var user = await _unitOfWork.Repository<User>().GetByPropertyAsync(u => u.Email, userDto.Email, cancellationToken);

        if (user == null)
        {
            return Result.Failure<UserDTO>("Invalid user to update");
        }

        //user.Email = request.User.Email;
        user.Username = request.User.Username;
        if(userDto.Password is not null)
        {
            user.Password = _securityService.HashPassword(userDto.Password);
        }

        _unitOfWork.Repository<User>().Update(user);

        var userEvent = _mapper.ToUpsertEvent(user);
        _unitOfWork.AddOutboxEvent(userEvent);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success<UserDTO>(request.User);
    }
}
