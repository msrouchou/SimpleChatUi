using Microsoft.AspNetCore.Components;
using SimpleChatUi.Providers;

namespace SimpleChatUi.Components.Pages;

public partial class Settings
{
    private IEnumerable<AiProvider> _aiProviders = [];

    [Inject]
    public required ILogger<Settings> Logger { get; set; }

    [Inject]
    public required AiServiceClient AiServiceClient { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetAiProvidersAndHandleResponseAsync(CancellationToken.None);
    }

    private async Task GetAiProvidersAndHandleResponseAsync(CancellationToken cancellationToken)
    {
        try
        {
            _aiProviders = await AiServiceClient.GetAiProvidersAsync(cancellationToken);
            foreach (var provider in _aiProviders)
            {
                Logger.LogInformation("Available AI Provider: {Name} @ {Uri} with model {Model}", provider.Name, provider.Uri, provider.Model);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Error: {Message}", ex.Message);
        }

        await UpdateStateAsync();
    }

    private async Task UpdateStateAsync()
    {
        await InvokeAsync(() =>
        {
            StateHasChanged();
        });
    }
}