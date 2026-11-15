using Application.Abstraction;
using Application.Abstraction.CQRS;
using Application.Abstraction.Services;
using Application.DTOs;
using Application.Email.Abstraction;
using CSharpFunctionalExtensions;
using Domain.Entities.User;

namespace Application.CommandsAndQueries.Users;

public sealed record CreateUserCommand(UserDTO userDto) : ICommand;

public sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityService _securityService;
    private readonly IEmailService _emailService;
    private readonly UserMapper _mapper;

    public CreateUserCommandHandler(
        IUnitOfWork unitOfWork,
        ISecurityService securityService,
        IEmailService emailService,
        UserMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _securityService = securityService;
        _emailService = emailService;
        _mapper = mapper;
    }

    public async Task<Result> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = _mapper.ToEntity(request.userDto);

        if (user is null)
        {
            return Result.Failure("Creating user failed");

        }
        user.Id = Guid.NewGuid();
        user.Password = _securityService.HashPassword(request.userDto.Password);

        await _unitOfWork.Repository<User>().AddAsync(user, cancellationToken);

        var userEvent = _mapper.ToUpsertEvent(user);
        _unitOfWork.AddOutboxEvent(userEvent);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _emailService.SendWelcomeEmailAsync(user.Email, user.Email, cancellationToken);

        return Result.Success();
    }
}
