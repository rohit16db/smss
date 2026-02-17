using MediatR;
using SMS.Application.Auth.DTOs;

namespace SMS.Application.Features.Users.Queries;

public class GetUserByIdQuery : IRequest<UserDto?>
{
    public Guid UserId { get; set; }
}
