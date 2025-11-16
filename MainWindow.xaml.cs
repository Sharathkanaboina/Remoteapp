using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;
using System.Windows;

namespace RemoteAdminClientUI
{
    public partial class MainWindow : Window
    {
        private HubConnection? connection;
        private string? activeSessionId;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectButton.IsEnabled = false;

            connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/control", o =>
                {
                    o.AccessTokenProvider = () => Task.FromResult("PLACEHOLDER_TOKEN");
                })
                .WithAutomaticReconnect()
                .Build();

            connection.On<object>("ControlRequested", async payload =>
            {
                Dispatcher.Invoke(() => Log("Incoming control request."));

                string json = JsonSerializer.Serialize(payload);
                var doc = JsonDocument.Parse(json);
                string sessionId = doc.RootElement.GetProperty("SessionId").GetString()!;
                string operatorName = doc.RootElement.GetProperty("Operator").GetString()!;

                bool consent = await AskForConsent(operatorName, sessionId);

                if (consent)
                {
                    activeSessionId = sessionId;
                    Dispatcher.Invoke(() => SessionText.Text = sessionId);

                    await connection.SendAsync("ClientMessageToOperator", null, new
                    {
                        SessionAccepted = true,
                        SessionId = sessionId
                    });

                    Dispatcher.Invoke(() => Log($"Session {sessionId} accepted."));
                }
                else
                {
                    await connection.SendAsync("ClientMessageToOperator", null, new
                    {
                        SessionAccepted = false,
                        SessionId = sessionId
                    });
                    Dispatcher.Invoke(() => Log("Session declined."));
                }
            });

            connection.On<object>("InputEvent", payload =>
            {
                var json = JsonSerializer.Serialize(payload);
                var doc = JsonDocument.Parse(json);

                string sid = doc.RootElement.GetProperty("sessionId").GetString()!;
                if (sid != activeSessionId)
                    return;

                var inputEvent = doc.RootElement.GetProperty("inputEvent");
                InputHandler.Handle(inputEvent);

                Dispatcher.Invoke(() => Log("Input processed: " + inputEvent.ToString()));
            });

            await connection.StartAsync();
            StatusText.Text = "Connected";
            Log("Connected to server.");

            ConnectButton.IsEnabled = true;
        }

        private Task<bool> AskForConsent(string op, string sid)
        {
            return Dispatcher.InvokeAsync(() =>
            {
                var dlg = new ConsentDialog(op, sid);
                return dlg.ShowDialog() == true;
            }).Task;
        }

        private void Log(string msg)
        {
            LogBox.AppendText(msg + "\n");
            LogBox.ScrollToEnd();
        }
    }
}
