using CustomAPITemplate.Contract.V1;
using CustomAPITemplate.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustomAPITemplate.Controllers.V1;

[Route("api/v1/[controller]")]
[ApiController]
public class IdentityController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public IdentityController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    [HttpPost]
    [Route("register")]
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
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var response = await _identityService.LoginAsync(request);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }
}