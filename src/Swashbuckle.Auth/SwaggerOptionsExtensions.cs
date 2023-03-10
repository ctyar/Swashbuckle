using System;
using System.Linq;
using System.Web;
using Ctyar.Swashbuckle.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Microsoft.Extensions.DependencyInjection;

public static class SwaggerOptionsExtensions
{
    private static string[]? Scopes;

    public static void AddIdentityServer(this SwaggerGenOptions options, string authority)
    {
        var authoritUrl = new Uri(authority);

        AddOAuth2(options, new Uri(authoritUrl, "connect/authorize"), new Uri(authoritUrl, "connect/token"));
    }

    public static void AddAuth0(this SwaggerGenOptions options, string authority, string audience)
    {
        var authorityUrl = new Uri(authority);

        var httpValueCollection = HttpUtility.ParseQueryString(authorityUrl.Query);
        httpValueCollection.Add("audience", audience);

        var authorizationUrl = new UriBuilder(authorityUrl)
        {
            Query = httpValueCollection.ToString(),
            Path = "authorize"
        }.Uri;

        var tokenUrl = new UriBuilder(authorityUrl)
        {
            Path = "oauth/token"
        }.Uri;

        AddOAuth2(options, authorizationUrl, tokenUrl);
    }

    public static void AddOAuth2(this SwaggerGenOptions options, Uri authorizationUrl, Uri tokenUrl)
    {
        options.AddSecurityDefinition("oAuth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = authorizationUrl,
                    TokenUrl = tokenUrl,
                    Scopes = Scopes?.ToDictionary(item => item, item => item),
                }
            }
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = "oAuth2",
                        Type = ReferenceType.SecurityScheme
                    }
                },
                Array.Empty<string>()
            }
        });

        options.OperationFilter<SecurityRequirementsOperationFilter>();
    }

    public static void UseIdentityServer(this SwaggerUIOptions options, string clientId, params string[] scopes)
    {
        UseOAuth2(options, clientId, scopes);
    }

    public static void UseAuth0(this SwaggerUIOptions options, string clientId, params string[] scopes)
    {
        UseOAuth2(options, clientId, scopes);
    }

    public static void UseOAuth2(this SwaggerUIOptions options, string clientId, params string[] scopes)
    {
        Scopes = scopes.Length > 0 ? scopes : null;

        options.OAuthClientId(clientId);

        if (Scopes is not null)
        {
            options.OAuthScopes(Scopes);
        }

        options.OAuthUsePkce();
    }
}
