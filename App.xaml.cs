using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BetterNotepad
{
    public partial class App : Application
    {
        private const string MutexName = "BetterNotepad_SingleInstance_Mutex";
        private const string PipeName = "BetterNotepad_SingleInstance_Pipe";

        private Mutex? appMutex;
        private MainWindow? mainWindow;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            appMutex = new Mutex(true, MutexName, out bool isFirstInstance);

            if (!isFirstInstance)
            {
                SendArgsToExistingInstance(e.Args);
                Shutdown();
                return;
            }

            mainWindow = new MainWindow(e.Args);
            mainWindow.Show();

            _ = ListenForFilesAsync();
        }

        private void SendArgsToExistingInstance(string[] args)
        {
            try
            {
                string message = string.Join("|", args);

                using NamedPipeClientStream client =
                    new NamedPipeClientStream(".", PipeName, PipeDirection.Out);

                client.Connect(1000);

                using StreamWriter writer = new StreamWriter(client);
                writer.WriteLine(message);
                writer.Flush();
            }
            catch { }
        }

        private async Task ListenForFilesAsync()
        {
            while (true)
            {
                try
                {
                    using NamedPipeServerStream server =
                        new NamedPipeServerStream(PipeName, PipeDirection.In);

                    await server.WaitForConnectionAsync();

                    using StreamReader reader = new StreamReader(server);
                    string? message = await reader.ReadLineAsync();

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            string[] args = message.Split('|', StringSplitOptions.RemoveEmptyEntries);
                            mainWindow?.HandleExternalArgs(args);
                        });
                    }
                }
                catch { }
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            appMutex?.ReleaseMutex();
            appMutex?.Dispose();
        }
    }
}