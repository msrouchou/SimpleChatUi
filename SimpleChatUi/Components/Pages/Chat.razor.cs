using Markdig;
using Markdown.ColorCode;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SimpleChatUi.Hubs;

namespace SimpleChatUi.Components.Pages
{
    public partial class Chat : ComponentBase
    {
        [Inject]
        public required IJSRuntime JSRuntime { get; set; }

        [Inject]
        public required ChatHubService ChatHubService { get; set; }

        [Inject]
        public required ILogger<Chat> Logger { get; set; }

        public required string _currentUser;

        private enum Sender
        {
            User,
            Bot,
        }

        private readonly Dictionary<Sender, List<MarkupString>> _chatHistory = [];
        private string _userMessage = string.Empty;
        private readonly MarkdownPipeline? _markdownPipeline;

        public Chat()
        {
            _currentUser = $"{Environment.UserName}_{Environment.TickCount}";

            _markdownPipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseColorCode()
                .Build();
        }

        protected override void OnInitialized()
        {
            ChatHubService.BotMessageReceived += OnBotMessageReceived;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await ScrollToElementAsync("input-container");
        }

        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(_userMessage))
                return;

            await DisplayUserPrompt();

            // Call the SignalR method to send the message
            await ChatHubService.SendMessageToBotAsync(_currentUser, _userMessage);

            _userMessage = "";

            await DisplayBotAnswer(isEchoBot: false);
        }

        private async Task HandleKeyPress(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await SendMessage();
            }
        }

        private async Task DisplayUserPrompt() => await AddMessageToHistory(Sender.User, _userMessage);

        private readonly Random rnd = new Random();

        private async Task DisplayBotAnswer(bool isEchoBot)
        {
            if (isEchoBot)
            {
                var chunks = EchoBot.GetAnswer(_userMessage, 10);

                // clear UI input
                _userMessage = "";

                for (int i = 0; i < chunks.Count; i++)
                {
                    var chunk = chunks[i];
                    if (i == 0)
                    {
                        await AddMessageToHistory(Sender.Bot, chunk);
                    }
                    else
                    {
                        // keep updating the last answer while streaming chunks
                        var currentAnswer = _chatHistory[Sender.Bot].Last().Value;
                        currentAnswer += chunk;
                        _chatHistory[Sender.Bot][_chatHistory[Sender.Bot].Count - 1] = new MarkupString(currentAnswer);
                    }

                    StateHasChanged();
                    await Task.Delay(rnd.Next(0, 1000));

                    return;
                }
            }

            await ScrollToElementAsync("input-container");
        }

        int tokenIndex;


        int codeDelimiterCounter;
        int codeBlockDelimiterCounter;

        private async void OnBotMessageReceived(object? sender, BotAnswerReceivedEventArgs botEvent)
        {
            if (!botEvent.TargetUser.Equals(_currentUser))
            {
                var truncatedAnswer = botEvent.Answer[..Math.Min(botEvent.Answer.Length, 10)];
                Logger.LogDebug("Received irrelevant answer ({TruncatedAnswer}) for {IrrelevantUser} while current chat is for {User}", truncatedAnswer, botEvent.TargetUser, _currentUser);
                return;
            }

            if (botEvent.Answer.Trim().StartsWith('`'))
            {
                codeDelimiterCounter++;
            }

            if (botEvent.Answer.Trim().StartsWith("```"))
            {
                codeBlockDelimiterCounter++;
            }

            tokenIndex++;
            if (tokenIndex == 1) // first chunk
            {
                await AddMessageToHistory(Sender.Bot, botEvent.Answer);
                return;
            }

            var lastAnswerIndex = _chatHistory[Sender.Bot].Count - 1;

            if (botEvent.IsDone)
            {
                Logger.LogInformation("Rendering final response for user {User}...", botEvent.TargetUser);

                var fullAnswer = Markdig.Markdown.ToHtml(_chatHistory[Sender.Bot][lastAnswerIndex].Value, _markdownPipeline);
                _chatHistory[Sender.Bot][lastAnswerIndex] = new(fullAnswer);

                await UpdateStateAsync();

                tokenIndex = 0;
                codeDelimiterCounter = 0;
                codeBlockDelimiterCounter = 0;
                return;
            }

            var answerToken = botEvent.Answer;
            _chatHistory[Sender.Bot][lastAnswerIndex] = new(_chatHistory[Sender.Bot][lastAnswerIndex] + answerToken);

            //if (codeDelimiterCounter != 0 && codeDelimiterCounter % 2 == 0)
            //{
            //    Logger.LogWarning("Rendering partialAnswer because got '{Answer}'. codeDelimiterCounter = {codeDelimiterCounter}. codeBlockDelimiterCounter = {codeBlockDelimiterCounter}", answerToken, codeDelimiterCounter, codeBlockDelimiterCounter);

            //    var partialAnswer = Markdig.Markdown.ToHtml(_chatHistory[Sender.Bot][lastAnswerIndex].Value, _markdownPipeline);
            //    _chatHistory[Sender.Bot][lastAnswerIndex] = new(partialAnswer);
            //    codeDelimiterCounter = 0;
            //}

            //if (codeBlockDelimiterCounter != 0 && codeBlockDelimiterCounter % 2 == 0)
            //{
            //    Logger.LogWarning("Rendering partialAnswer because got '{Answer}'. codeDelimiterCounter = {codeDelimiterCounter}. codeBlockDelimiterCounter = {codeBlockDelimiterCounter}", answerToken, codeDelimiterCounter, codeBlockDelimiterCounter);

            //    var partialAnswer = Markdig.Markdown.ToHtml(_chatHistory[Sender.Bot][lastAnswerIndex].Value, _markdownPipeline);
            //    _chatHistory[Sender.Bot][lastAnswerIndex] = new(partialAnswer);
            //    codeBlockDelimiterCounter = 0;
            //}

            await UpdateStateAsync();
        }

        //private async Task AddMessageToHistory(Sender sender, BotAnswerReceivedEventArgs e)
        private async Task AddMessageToHistory(Sender sender, string answer)
        {
            if (_chatHistory.TryGetValue(sender, out var messages))
            {
                _chatHistory[sender].Add(new(answer));
            }
            else
            {
                _chatHistory.Add(sender, [new(answer)]);
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

        private IEnumerable<dynamic> BuildChatSequence()
        {
            var userMessages = _chatHistory.ContainsKey(Sender.User) ? _chatHistory[Sender.User] : [];
            var botMessages = _chatHistory.ContainsKey(Sender.Bot) ? _chatHistory[Sender.Bot] : [];

            var chatSequence = userMessages.Zip(botMessages.DefaultIfEmpty(), (user, bot) => new { UserMessage = user, BotMessage = bot });
            return chatSequence;
        }

        private async Task ScrollToElementAsync(string elementId) => await JSRuntime.InvokeVoidAsync("scrollToElement", elementId);
    }
}