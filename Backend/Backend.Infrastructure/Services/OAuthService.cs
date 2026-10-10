using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Backend.Application.Abstractions;
using Backend.Application.Exceptions;
using Microsoft.Extensions.Configuration;

namespace Backend.Infrastructure.Services;

public class OAuthService : IOAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public OAuthService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<OAuthUserInfo> GetOAuthUserInfoAsync(string provider, string code, string redirectUri)
    {
        var normalizedProvider = provider.ToLowerInvariant();
        string clientId;
        string clientSecret;
        string tokenEndpoint;
        string userInfoEndpoint;

        if (normalizedProvider == "42")
        {
            clientId = _configuration["OAUTH_INTRA_CLIENT_ID"] ?? "";
            clientSecret = _configuration["OAUTH_INTRA_CLIENT_SECRET"] ?? "";
            tokenEndpoint = "https://api.intra.42.fr/oauth/token";
            userInfoEndpoint = "https://api.intra.42.fr/v2/me";
        }
        else if (normalizedProvider == "google")
        {
            clientId = _configuration["OAUTH_GOOGLE_CLIENT_ID"] ?? "";
            clientSecret = _configuration["OAUTH_GOOGLE_CLIENT_SECRET"] ?? "";
            tokenEndpoint = "https://oauth2.googleapis.com/token";
            userInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";
        }
        else
        {
            throw new BadRequestException("Unsupported OAuth provider.");
        }

        if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out var parsedRedirectUri) ||
            (!parsedRedirectUri.AbsolutePath.EndsWith("/auth/callback", StringComparison.OrdinalIgnoreCase) &&
             !parsedRedirectUri.AbsolutePath.EndsWith("/auth/callback/", StringComparison.OrdinalIgnoreCase)))
        {
            throw new BadRequestException("Invalid OAuth redirect URI.");
        }

        var httpClient = _httpClientFactory.CreateClient();
        var formParams = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "code", code },
            { "redirect_uri", redirectUri }
        };

        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = new FormUrlEncodedContent(formParams)
        };

        var tokenResponse = await httpClient.SendAsync(tokenRequest);
        if (!tokenResponse.IsSuccessStatusCode)
        {
            throw new BadRequestException("Failed to exchange OAuth code for access token.");
        }

        var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
        using var tokenDoc = JsonDocument.Parse(tokenContent);
        var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString();

        if (string.IsNullOrEmpty(accessToken))
        {
            throw new BadRequestException("Access token not found in response.");
        }

        var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, userInfoEndpoint);
        userInfoRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var userInfoResponse = await httpClient.SendAsync(userInfoRequest);
        if (!userInfoResponse.IsSuccessStatusCode)
        {
            throw new BadRequestException("Failed to retrieve user info from provider.");
        }

        var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
        using var userInfoDoc = JsonDocument.Parse(userInfoContent);
        string email = "";
        string username = "";

        if (normalizedProvider == "42")
        {
            email = userInfoDoc.RootElement.GetProperty("email").GetString() ?? "";
            username = userInfoDoc.RootElement.GetProperty("login").GetString() ?? "";
        }
        else if (normalizedProvider == "google")
        {
            if (userInfoDoc.RootElement.TryGetProperty("email_verified", out var verifiedProp))
            {
                bool isVerified = verifiedProp.ValueKind switch
                {
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.String => bool.TryParse(verifiedProp.GetString(), out var parsed) && parsed,
                    _ => false
                };

                if (!isVerified)
                {
                    throw new BadRequestException("OAuth email is not verified by provider.");
                }
            }

            email = userInfoDoc.RootElement.GetProperty("email").GetString() ?? "";
            username = userInfoDoc.RootElement.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "" : email.Split('@')[0];
        }

        if (string.IsNullOrEmpty(email))
        {
            throw new BadRequestException("Email not found in user info.");
        }

        return new OAuthUserInfo(email, username);
    }
}
