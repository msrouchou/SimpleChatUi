using SimpleChatUi.Components;
using SimpleChatUi.Hubs;
using SimpleChatUI.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(15);
});

builder.Services.AddSingleton<ChatHubService>();

builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();

app.UseRouting();

app.UseAntiforgery();

app.MapHub<ChatHub>("/chatHub");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

var chatHubService = app.Services.GetRequiredService<ChatHubService>();
await chatHubService.EnsureConnectionAsync();

app.Run();