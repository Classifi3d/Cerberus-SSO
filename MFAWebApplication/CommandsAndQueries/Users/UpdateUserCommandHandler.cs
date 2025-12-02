using AuthenticationWebApplication.DTOs;
using AuthenticationWebApplication.Enteties;
using AutoMapper;
using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction.Messaging;
using MFAWebApplication.Abstraction.UnitOfWork;
using MFAWebApplication.Context;
using MFAWebApplication.DTOs;
using MFAWebApplication.Entities;
using MFAWebApplication.Services;

namespace MFAWebApplication.CommandsAndQueries.Users;

public sealed record UpdateUserCommand(UserDTO User) : ICommand<UserDTO>;


internal sealed class UpdateUserCommandHandler
    : ICommandHandler<UpdateUserCommand,UserDTO>
{
    private readonly UnitOfWork<WriteDbContext> _unitOfWork;
    private readonly ISecurityService _securityService;
    private readonly Mapper _mapper;

    public UpdateUserCommandHandler(
        UnitOfWork<WriteDbContext> unitOfWork,
        Mapper mapper,
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
            user.Password = _securityService.PasswordHashing(userDto.Password);
        }
        user.UpdateDate = DateTime.UtcNow;

        _unitOfWork.Repository<User>().Update(user);

        var userEvent = _mapper.Map<UserCreatedEvent>(user);
        _unitOfWork.AddOutboxEvent(userEvent);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success<UserDTO>(request.User);
    }
}
