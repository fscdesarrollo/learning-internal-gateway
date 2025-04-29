using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Learning.InternalGateway.Shared;

public class AuthorizationHandler : DelegatingHandler
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthorizationHandler(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Aquí creas un HttpClient temporal para solicitar el token
        var tokenClient = _httpClientFactory.CreateClient("TokenClient");

        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "/realms/api-realm/protocol/openid-connect/token"); // Ajusta el path de token si es diferente
        tokenRequest.Content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", "client-domain-1"),
            new KeyValuePair<string, string>("client_secret", "oZKRafpZTNM51mHVgVgcYZgGIUI8lTau")
        });

        var tokenResponse = await tokenClient.SendAsync(tokenRequest, cancellationToken);

        if (tokenResponse.IsSuccessStatusCode)
        {
            var tokenData = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenData?.AccessToken);
        }
        else
        {
            // Podrías loggear o manejar el error de autenticación aquí
            throw new HttpRequestException("Unable to acquire access token.");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
}
