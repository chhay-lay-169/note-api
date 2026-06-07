using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace NoteApi.Extensions;

public static class JwtAuthenticationExtensions
{
    public static AuthenticationBuilder AddJwtAuthentication(this WebApplicationBuilder builder)
    {
        var secretKey = builder.Configuration["JwtSettings:Secret"] 
            ?? throw new InvalidOperationException("Secret missing from configuration.");

        return builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    ValidAudience = builder.Configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        string errorCode = "UNAUTHORIZED";
                        string errorMessage = "You are not authorized to access this resource.";

                        if (context.AuthenticateFailure is SecurityTokenExpiredException)
                        {
                            errorCode = "TOKEN_EXPIRED";
                            errorMessage = "Your access token has expired. Please refresh your session.";
                        }
                        else if (context.AuthenticateFailure is not null)
                        {
                            errorCode = "INVALID_TOKEN";
                            errorMessage = "The provided security token is invalid or malformed.";
                        }

                        await context.Response.WriteAsJsonAsync(new
                        {
                            code = errorCode,
                            message = errorMessage
                        });
                    }
                };
            });
    }
}