using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CustomAPITemplate.Contract.V1;
using CustomAPITemplate.Core;
using CustomAPITemplate.Core.Configuration;
using CustomAPITemplate.DB.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace CustomAPITemplate.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly JwtSettings _jwtSettings;

    public IdentityService(UserManager<AppUser> userManager, JwtSettings jwtSettings)
    {
        _userManager = userManager;
        _jwtSettings = jwtSettings;
    }

    public async Task<Response> RegisterAsync(RegistrationRequest request)
    {
        var response = new Response();
        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            response.Results.Add(new Result
            {
                Message = "Email already exists!",
                Severity = Severity.Error
            });
            return response;
        }

        var newUser = new AppUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email
        };

        var createdUser = await _userManager.CreateAsync(newUser, request.Password);

        if (!createdUser.Succeeded)
        {
            response.Results.AddRange(createdUser.Errors.Select(x => new Result
            {
                Message = x.Description,
                Severity = Severity.Error
            }));
            return response;
        }

        response.Results.Add(new Result
        {
            Message = "Successfuly created the user!",
            Severity = Severity.Info
        });

        return response;
    }

    public async Task<Response<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var response = new Response<LoginResponse>();

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            response.Results.Add(new Result
            {
                Message = "User does not exists",
                Severity = Severity.Error
            });
            return response;
        }

        var userHasValidPass = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!userHasValidPass)
        {
            response.Results.Add(new Result
            {
                Message = "Invalid password",
                Severity = Severity.Error
            });
            return response;
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("id", user.Id.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        response.Value = new LoginResponse
        {
            Token = tokenHandler.WriteToken(token)
        };

        return response;
    }
}