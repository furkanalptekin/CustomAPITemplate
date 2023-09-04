using System.Text;
using CustomAPITemplate.Core.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CustomAPITemplate.Helpers;

public class TokenValidationParametersHelper
{
    public static TokenValidationParameters GetTokenValidationParameters(JwtSettings jwtSettings, bool validateLifetime = true)
    {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero,
            ValidateLifetime = validateLifetime
        };
    }
}
