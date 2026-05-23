using CoffeeShop.Application.DTOs.Auth;
using CoffeeShop.Application.Interfaces;
using MediatR;

namespace CoffeeShop.Application.Features.Auth.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IAuthService _authService;

    public RegisterCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        => _authService.RegisterAsync(request.Request, cancellationToken);
}
