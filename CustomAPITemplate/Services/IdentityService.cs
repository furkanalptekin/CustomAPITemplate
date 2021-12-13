using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CustomAPITemplate.Contract.V1;
using CustomAPITemplate.Core;
using CustomAPITemplate.Core.Configuration;
using CustomAPITemplate.DB.Models;
using CustomAPITemplate.DB.Repositories;
using CustomAPITemplate.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CustomAPITemplate.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly JwtSettings _jwtSettings;
    private readonly AppDbContext _dbContext;

    public IdentityService(UserManager<AppUser> userManager, JwtSettings jwtSettings, AppDbContext dbContext)
    {
        _userManager = userManager;
        _jwtSettings = jwtSettings;
        _dbContext = dbContext;
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

        if (user.IsBanned)
        {
            response.Results.Add(new()
            {
                Message = "User is banned",
                Severity = Severity.Error
            });
            return response;
        }

        response.Value = await GenerateAuthenticationResultAsync(user);

        return response;
    }

    public async Task<Response<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var response = new Response<LoginResponse>();
        var principal = GetPrincipalFromToken(request.Token);

        if (principal == null)
        {
            response.Results.Add(new()
            {
                Message = "Invalid token",
                Severity = Severity.Error
            });
            return response;
        }

        long.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Exp), out var expiryDateUnix);
        var expiryDateTimeUtc = DateTime.UnixEpoch.AddSeconds(expiryDateUnix);

        if (expiryDateTimeUtc > DateTime.UtcNow)
        {
            response.Results.Add(new()
            {
                Message = "Invalid token",
                Severity = Severity.Error
            });
            return response;
        }

        var jti = principal.FindFirstValue(JwtRegisteredClaimNames.Jti);
        var storedRefreshToken = await _dbContext.RefreshToken.SingleOrDefaultAsync(x => x.Token == request.RefreshToken);

        if (storedRefreshToken == null
            || DateTime.UtcNow > storedRefreshToken.ExpiryDate
            || storedRefreshToken.Used
            || storedRefreshToken.Invalidated
            || storedRefreshToken.JwtId != jti)
        {
            response.Results.Add(new()
            {
                Message = "Invalid token",
                Severity = Severity.Error
            });
            return response;
        }

        storedRefreshToken.Used = true;
        _dbContext.RefreshToken.Update(storedRefreshToken);
        await _dbContext.SaveChangesAsync();

        var user = await _userManager.FindByIdAsync(principal.FindFirstValue("id"));
        if (user == null)
        {
            response.Results.Add(new()
            {
                Message = "Invalid user",
                Severity = Severity.Error
            });
            return response;
        }

        response.Value = await GenerateAuthenticationResultAsync(user);

        return response;
    }

    private async Task<LoginResponse> GenerateAuthenticationResultAsync(AppUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("id", user.Id.ToString())
        };

        var userRoles = await _userManager.GetRolesAsync(user);
        foreach (var role in userRoles ?? Enumerable.Empty<string>())
        {
            claims.Add(new(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(_jwtSettings.TokenLifetime),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        var refreshToken = new RefreshToken
        {
            JwtId = token.Id,
            UserId = user.Id,
            CreationDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddMonths(6),
        };

        await _dbContext.RefreshToken.AddAsync(refreshToken);
        await _dbContext.SaveChangesAsync();

        return new LoginResponse
        {
            Token = tokenHandler.WriteToken(token),
            RefreshToken = refreshToken.Token
        };
    }

    private ClaimsPrincipal GetPrincipalFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var tokenValidationParameters = TokenValidationParametersHelper.GetTokenValidationParameters(_jwtSettings, false);
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
            if (!IsJwtValidSecurityAlgorithm(validatedToken))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private bool IsJwtValidSecurityAlgorithm(SecurityToken securityToken)
    {
        return (securityToken is JwtSecurityToken jwtSecurityToken)
            && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
    }

    public async Task<Response> BanUserAsync(Guid id)
    {
        var response = new Response();

        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            response.Results.Add(new Result
            {
                Message = "User does not exists",
                Severity = Severity.Error
            });
            return response;
        }

        user.IsBanned = true;

        var updateResponse = await _userManager.UpdateAsync(user);
        if (!updateResponse.Succeeded)
        {
            response.Results.AddRange(updateResponse.Errors.Select(x => new Result
            {
                Message = x.Description,
                Severity = Severity.Error
            }));
            return response;
        }

        response.Results.Add(new()
        {
            Message = "User is banned successfully",
            Severity = Severity.Info
        });
        return response;
    }
}