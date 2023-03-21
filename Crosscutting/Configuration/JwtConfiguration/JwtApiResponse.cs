using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Crosscutting.Configuration.JwtConfiguration;

public static class JwtApiResponse
{
    public static async Task<object> JwtRefreshAndGenerate(IEnumerable<Claim> claims, IConfiguration configuration, string refreshToken = null!) {
        // check if a refresh token was provided
        if (!string.IsNullOrEmpty(refreshToken)) {
            // validate the refresh token against the storage mechanism
            var validRefreshToken = await ValidateRefreshTokenAsync(refreshToken);

            // if the refresh token is valid, generate a new access token
            if (validRefreshToken) 
            {
                var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"]!));
                var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);

                var token = new JwtSecurityToken(
                    issuer: configuration["JWT:ValidIssuer"],
                    audience: configuration["JWT:ValidAudience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(10),
                    signingCredentials: signingCredentials
                );

                // create a new refresh token and save it to storage mechanism
                var newRefreshToken = new RefreshToken {
                    Token = Guid.NewGuid().ToString(),
                    Expiration = DateTime.UtcNow.AddMinutes(10),
                };
                await SaveRefreshTokenAsync(newRefreshToken);

                return new {
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                    ExpiresIn = token.ValidTo,
                    RefreshToken = newRefreshToken.Token
                };
            }
        }

        // if no refresh token was provided or the refresh token is invalid, generate a new access token and refresh token
        var symmetricSecurityKey2 = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"]!));
        var signingCredentials2 = new SigningCredentials(symmetricSecurityKey2, SecurityAlgorithms.HmacSha256Signature);

        var accessToken = new JwtSecurityToken(
            issuer: configuration["JWT:ValidIssuer"],
            audience: configuration["JWT:ValidAudience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: signingCredentials2
        );

        var newRefreshToken2 = new RefreshToken {
            Token = Guid.NewGuid().ToString(),
            Expiration = DateTime.UtcNow.AddMinutes(10),
        };
        await SaveRefreshTokenAsync(newRefreshToken2);

        return new {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
            ExpiresIn = accessToken.ValidTo,
            RefreshToken = newRefreshToken2.Token
        };
    }

    //InMemoryImplementation
    private static readonly List<RefreshToken> RefreshTokens = new List<RefreshToken>();
    private static async Task<bool> ValidateRefreshTokenAsync(string refreshToken)
    {
        RefreshTokens.RemoveAll(rt => rt.Expiration > DateTime.UtcNow);

        return await Task.FromResult(RefreshTokens.Any(rt => rt.Token == refreshToken && rt.Expiration > DateTime.UtcNow));
    }

    private static async Task SaveRefreshTokenAsync(RefreshToken refreshToken) 
    {
        await Task.Run(() => RefreshTokens.Add(refreshToken));
    }
}

public class RefreshToken {
    public string Token { get; set; } = null!;
    public DateTime Expiration { get; set; }
}
