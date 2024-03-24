# Installing SignalR Client-Side Library

```batch
cd SimpleChatUi\SimpleChatUi
```

- Install LibMan globally

```batch
dotnet tool install -g Microsoft.Web.LibraryManager.Cli
```

 - Install the SignalR client package and copy the minified JavaScript file (`signalr.js`) to the specified location (`wwwroot/js/signalr`)

 ```batch
libman install @microsoft/signalr@latest -p unpkg -d wwwroot/js/signalr --files dist/browser/signalr.js
```

- Reference the SignalR JavaScript in the `head` HTML

```html
<script src="~/js/signalr/signalr.js"></script>
```