using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CustomAPITemplate.Contract.V1;
using CustomAPITemplate.Core;
using CustomAPITemplate.Core.Configuration;
using CustomAPITemplate.Core.Constants;
using CustomAPITemplate.DB.Models;
using CustomAPITemplate.DB.Repositories;
using CustomAPITemplate.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace CustomAPITemplate.Services;

public class IdentityService(UserManager<AppUser> _userManager, JwtSettings _jwtSettings, IRefreshTokenRepository _repository)
    : IIdentityService
{
    public async Task<Response> RegisterAsync(RegistrationRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email).ConfigureAwait(false);
        if (existingUser != null)
        {
            return Result.Error("Email already exists!");
        }

        var user = new AppUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email
        };

        var createdUserResult = await _userManager.CreateAsync(user, request.Password);
        if (!createdUserResult.Succeeded)
        {
            return createdUserResult.Errors
                .Select(x => Result.Error(x.Description))
                .ToList();
        }

        var roleResult = await _userManager.AddToRoleAsync(user, Roles.USER);
        if (!roleResult.Succeeded)
        {
            return roleResult.Errors
                .Select(x => Result.Error(x.Description))
                .ToList();
        }

        return Result.Info("Successfuly created the user!");
    }

    public async Task<Response<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken token)
    {
        var user = await _userManager.FindByEmailAsync(request.Email).ConfigureAwait(false);
        if (user == null)
        {
            return Result.Error("User does not exists");
        }

        var userHasValidPass = await _userManager.CheckPasswordAsync(user, request.Password).ConfigureAwait(false);
        if (!userHasValidPass)
        {
            return Result.Error("Invalid password");
        }

        if (user.IsBanned)
        {
            return Result.Error("User is banned");
        }

        return await GenerateAuthenticationResultAsync(user, token).ConfigureAwait(false);
    }

    public async Task<Response<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken token)
    {
        var principal = GetPrincipalFromToken(request.Token);
        if (principal == null)
        {
            return Result.Error("Invalid token");
        }

        _ = long.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Exp), out var expiryDateUnix);
        var expiryDateTimeUtc = DateTime.UnixEpoch.AddSeconds(expiryDateUnix);

        if (expiryDateTimeUtc > DateTime.UtcNow)
        {
            return Result.Error("Invalid token");
        }

        var jti = principal.FindFirstValue(JwtRegisteredClaimNames.Jti);

        if (!Guid.TryParse(request.RefreshToken, out var refreshToken) || refreshToken == Guid.Empty)
        {
            return Result.Error("Invalid token");
        }

        var refreshTokenResponse = await _repository.GetAsync(refreshToken, token).ConfigureAwait(false);
        if (!refreshTokenResponse.Success)
        {
            return refreshTokenResponse.Results;
        }

        var storedRefreshToken = refreshTokenResponse.Value;
        if (storedRefreshToken == null
            || DateTime.UtcNow > storedRefreshToken.ExpiryDate
            || !storedRefreshToken.IsActive
            || storedRefreshToken.Invalidated
            || storedRefreshToken.JwtId != jti)
        {
            return Result.Error("Invalid token");
        }

        var deleteResponse = await _repository.DeleteAsync(storedRefreshToken, token).ConfigureAwait(false);
        if (!deleteResponse.Success)
        {
            return deleteResponse.Results;
        }

        var user = await _userManager.FindByIdAsync(principal.FindFirstValue("id")).ConfigureAwait(false);
        if (user == null)
        {
            return Result.Error("Invalid user");
        }

        if (user.IsBanned)
        {
            return Result.Error("User is banned");
        }

        return await GenerateAuthenticationResultAsync(user, token).ConfigureAwait(false);
    }

    public async Task<Response> BanUserAsync(Guid id, CancellationToken token)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return Result.Error("User does not exists");
        }

        user.IsBanned = true;

        var updateResponse = await _userManager.UpdateAsync(user);
        if (!updateResponse.Succeeded)
        {
            return updateResponse.Errors
                .Select(x => Result.Error(x.Description))
                .ToList();
        }

        await InvalidateRefreshTokensByUserId(id, token);

        return Result.Info("User is banned successfully");
    }

    public async Task<Response> InvalidateRefreshTokensByUserId(Guid id, CancellationToken token)
    {
        return await _repository.InvalidateTokensByUserId(id, token).ConfigureAwait(false);
    }

    public async Task<Response> ChangeUserRole(UserRoleRequest request, CancellationToken token)
    {
        var existingUser = await _userManager.FindByIdAsync(request.UserId);
        if (existingUser == null)
        {
            return Result.Error("User does not exists");
        }

        var currentRoles = await _userManager.GetRolesAsync(existingUser);
        if (currentRoles.Count == 1 && currentRoles[0] == request.Role)
        {
            return Result.Error("Cannot change to same role");
        }

        var removeRolesResult = await _userManager.RemoveFromRolesAsync(existingUser, currentRoles ?? []);
        if (!removeRolesResult.Succeeded)
        {
            return removeRolesResult.Errors
                .Select(x => Result.Error(x.Description))
                .ToList();
        }

        var addToRoleResult = await _userManager.AddToRoleAsync(existingUser, request.Role);
        if (!addToRoleResult.Succeeded)
        {
            return addToRoleResult.Errors
                .Select(x => Result.Error(x.Description))
                .ToList();
        }

        return Result.Info("User role changed.");
    }

    private async Task<Response<LoginResponse>> GenerateAuthenticationResultAsync(AppUser user, CancellationToken ct)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("id", user.Id.ToString())
        ];

        var userRoles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
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
            ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenLifetimeInDays),
        };

        var createResponse = await _repository.CreateAsync(refreshToken, ct).ConfigureAwait(false);
        if (!createResponse.Success)
        {
            return createResponse.Results;
        }

        return new LoginResponse
        {
            Token = tokenHandler.WriteToken(token),
            RefreshToken = createResponse.Value.Id
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

    private static bool IsJwtValidSecurityAlgorithm(SecurityToken securityToken)
    {
        return (securityToken is JwtSecurityToken jwtSecurityToken)
            && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
    }
}