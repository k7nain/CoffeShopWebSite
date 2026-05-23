using CoffeeShop.API.Extensions;
using CoffeeShop.Application.DTOs.Auth;
using CoffeeShop.Application.Features.Auth.Commands;
using CoffeeShop.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IMediator _mediator;

    public AuthController(IAuthService authService, IMediator mediator)
    {
        _authService = authService;
        _mediator = mediator;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Create the first admin when no admin exists yet (no secret required).
    /// </summary>
    [HttpPost("bootstrap-admin")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> BootstrapAdmin([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.BootstrapFirstAdminAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Create an admin account using the setup secret from appsettings (AdminSetup:SetupSecretKey).
    /// </summary>
    [HttpPost("register-admin")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> RegisterAdmin([FromBody] RegisterAdminRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterAdminAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.RefreshTokenAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(User.GetUserId(), cancellationToken);
        return NoContent();
    }
}
