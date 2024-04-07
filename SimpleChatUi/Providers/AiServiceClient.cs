using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleChatUi.Providers;

public class AiServiceClient
{
    private readonly HttpClient _httpClient;

    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public AiServiceClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(nameof(AiServiceClient));

        _jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
        };

        _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task<IEnumerable<AiProvider>> GetAiProvidersAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("/AiProviders", cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<AiProvider>>(_jsonSerializerOptions, cancellationToken);
        return result!;
    }
}
