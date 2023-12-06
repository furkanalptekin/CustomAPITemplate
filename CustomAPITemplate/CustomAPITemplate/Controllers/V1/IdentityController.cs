using CustomAPITemplate.Attributes;
using CustomAPITemplate.Contract.V1;
using CustomAPITemplate.Core.Constants;
using CustomAPITemplate.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomAPITemplate.Controllers.V1;

[Route("api/v1/[controller]")]
[ApiController]
public class IdentityController(IIdentityService _identityService) : ControllerBase
{
    [HttpPost]
    [Route("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegistrationRequest request)
    {
        var response = await _identityService.RegisterAsync(request);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost]
    [Route("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken token)
    {
        var response = await _identityService.LoginAsync(request, token);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost]
    [Route("refresh")]
    [Transaction]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken token)
    {
        var response = await _identityService.RefreshTokenAsync(request, token);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost]
    [Route("ban")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = MinAllowedRole.ADMIN)]
    [Transaction]
    public async Task<IActionResult> BanUser(Guid userId, CancellationToken token)
    {
        var response = await _identityService.BanUserAsync(userId, token);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost]
    [Route("invalidate")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = MinAllowedRole.ADMIN)]
    [Transaction]
    public async Task<IActionResult> Invalidate(Guid userId, CancellationToken token)
    {
        var response = await _identityService.InvalidateRefreshTokensByUserId(userId, token);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost]
    [Route("changeUserRole")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = MinAllowedRole.ADMIN)]
    public async Task<IActionResult> ChangeUserRole(UserRoleRequest request, CancellationToken token)
    {
        var response = await _identityService.ChangeUserRole(request, token);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }
}