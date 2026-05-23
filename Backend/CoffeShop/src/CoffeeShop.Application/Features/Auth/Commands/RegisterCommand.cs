using CoffeeShop.Application.DTOs.Auth;
using MediatR;

namespace CoffeeShop.Application.Features.Auth.Commands;

public record RegisterCommand(RegisterRequest Request) : IRequest<AuthResponse>;
