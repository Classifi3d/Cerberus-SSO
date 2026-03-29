using Domain.Entities.User;
using Riok.Mapperly.Abstractions;

namespace Application.DTOs;

[Mapper]
public partial class UserMapper
{
    public partial UserDTO ToDto(User user);
    public partial User ToEntity(UserDTO dto);
    public partial UserUpsertEvent ToUpsertEvent(User user);
    public partial UserDeleteEvent ToDeleteEvent(User user);
    public partial UserReadModel ToReadModel(UserUpsertEvent @event);
}