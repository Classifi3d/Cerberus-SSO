using CSharpFunctionalExtensions;
using MFAWebApplication.Abstraction.Messaging;
using MFAWebApplication.Abstraction.Repository;
using MFAWebApplication.Entities.User;

namespace MFAWebApplication.CommandsAndQueries.Users;

public sealed record GetUserProfileQuery(Guid UserId) : IQuery<UserReadModel>;

internal sealed class GetUserProfileQueryHandler
    : IQueryHandler<GetUserProfileQuery, UserReadModel>
{
    private readonly IReadModelRepository<UserReadModel> _userRepository;

    public GetUserProfileQueryHandler(IReadModelRepository<UserReadModel> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserReadModel>> Handle(
        GetUserProfileQuery request,
        CancellationToken cancellationToken)
    {
        //var user = await _userRepository.GetByPropertyAsync(
        //    u => u.Id,
        //    request.UserId.ToString(),
        //    cancellationToken);

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
            return Result.Failure<UserReadModel>("User not found");


        return Result.Success(user);
    }
}
