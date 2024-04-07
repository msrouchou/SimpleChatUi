using Serilog;
using SimpleChatUi.Components;
using SimpleChatUi.Configuration;
using SimpleChatUi.Hubs;
using SimpleChatUi.Providers;
using SimpleChatUI.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AiServiceConfiguration>(builder.Configuration.GetSection("AiService"));

// Add services to the container.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(15);
});

builder.Services.AddSingleton<ChatHubService>();

// Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Backend AiService API Client (settings, etc.)
builder.Services.AddHttpClient(nameof(AiServiceClient), client =>
{
    var uri = (builder.Configuration.GetValue<string>("AiService:Uri")) ?? throw new InvalidOperationException($"{nameof(AiServiceConfiguration.Uri)} is not configured.");
    client.BaseAddress = new(uri);
});

builder.Services.AddSingleton<AiServiceClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();

//app.UseRouting();

app.UseAntiforgery();

app.MapHub<ChatHub>("/chatHub");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

var chatHubService = app.Services.GetRequiredService<ChatHubService>();
await chatHubService.EnsureConnectionAsync();

app.Run();

