using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace zuse_ws_service
{
    public partial class Service1 : ServiceBase
    {
        private CancellationTokenSource cancellationTokenSource;
        private Task webSocketTask;
        private NotifyIcon notifyIcon;
        private ContextMenu contextMenu;
        public Service1()
        {
            InitializeComponent();

            // Create an instance of NotifyIcon
            notifyIcon = new NotifyIcon();

            // Set the properties of NotifyIcon
            notifyIcon.Icon = SystemIcons.Application;
            notifyIcon.Text = "Zuse WS Service";
            // Create the context menu
            contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add("Open", OpenMenuItem_Click);
            contextMenu.MenuItems.Add("Exit", ExitMenuItem_Click);

            // Assign the context menu to NotifyIcon
            notifyIcon.ContextMenu = contextMenu;
        }

        protected override void OnStart(string[] args)

        {
            notifyIcon.Visible = true;

            try
            {
                cancellationTokenSource = new CancellationTokenSource();
                webSocketTask = Task.Run(async () =>
                {
                    while (!cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        try
                        {
                            using (var webSocket = new ClientWebSocket())
                            {
                                await webSocket.ConnectAsync(new Uri("ws://127.0.0.1:8000/ws/ping"), cancellationTokenSource.Token);

                                while (webSocket.State == WebSocketState.Open)
                                {
                                    var buffer = new byte[1024];
                                    var pingMessage = Encoding.UTF8.GetBytes("ping");

                                    await webSocket.SendAsync(new ArraySegment<byte>(pingMessage), WebSocketMessageType.Text, true, cancellationTokenSource.Token);
                                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);
                                    var response = Encoding.UTF8.GetString(buffer, 0, result.Count);

                                    if (response != "pong")
                                    {
                                        throw new Exception("Invalid ping response received.");
                                    }

                                    await Task.Delay(10000, cancellationTokenSource.Token); // Ping interval
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Handle WebSocket connection errors
                        }
                    }
                }, cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                // Handle any startup errors
            }

        }

        protected override void OnStop()
        {
            cancellationTokenSource?.Cancel();

            cancellationTokenSource?.Cancel();

            if (webSocketTask != null && !webSocketTask.Wait(TimeSpan.FromSeconds(5)))
            {
                // Handle timeout or any other cleanup logic
            }

            cancellationTokenSource?.Dispose();
            webSocketTask?.Dispose();

            cancellationTokenSource?.Dispose();
            webSocketTask?.Dispose();

            notifyIcon.Visible = false;
        }

        private void OpenMenuItem_Click(object sender, EventArgs e)
        {
            // Implement the logic to open your application window or perform any desired action
            // ...
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            // Implement the logic to gracefully exit your service application
            // This could include stopping any running processes, releasing resources, etc.
            // Then call the Stop method to stop the service
            Stop();
        }
    }
}
