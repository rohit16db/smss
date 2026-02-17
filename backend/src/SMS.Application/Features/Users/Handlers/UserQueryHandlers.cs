using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Auth.DTOs;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Users.Queries;

namespace SMS.Application.Features.Users.Handlers;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IApplicationDbContext _context;

    public GetUserByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Where(u => u.Id == request.UserId && u.IsActive)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role,
                IsActive = u.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        return user;
    }
}
