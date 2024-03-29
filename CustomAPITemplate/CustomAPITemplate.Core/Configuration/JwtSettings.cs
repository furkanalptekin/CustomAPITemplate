﻿namespace CustomAPITemplate.Core.Configuration;

public class JwtSettings
{
    public string Secret { get; set; }
    public TimeSpan TokenLifetime { get; set; }
    public int RefreshTokenLifetimeInDays { get; set; }
}