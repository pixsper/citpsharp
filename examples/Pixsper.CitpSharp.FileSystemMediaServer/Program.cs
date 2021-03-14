using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Pixsper.CitpSharp.FileSystemMediaServer
{
    public class RunCommandSettings : CommandSettings
    {
        [CommandArgument(0, "[LIBRARY_ROOT_PATH]")]
        [Description("Path of directory to use as media library")]
		public string? LibraryRootPath { get; set; }

        [CommandArgument(1, "[LOCAL_IP]")]
        [Description("Path of directory to use as media library")]
		[DefaultValue("0.0.0.0")]
		public string? LocalIp { get; set; }
    }

    public class RunCommand : AsyncCommand<RunCommandSettings>
    {
        static Guid stringToGuid(string value)
        {
            var md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));
            return new Guid(data);
        }

        public override async Task<int> ExecuteAsync(CommandContext context, RunCommandSettings settings)
        {
            var libraryRootAbsolutePath = settings.LibraryRootPath != null
                ? Path.GetFullPath(settings.LibraryRootPath)
                : Directory.GetCurrentDirectory();

            var localIp = IPAddress.Parse(settings.LocalIp ?? "0.0.0.0");

            var loggerFactory = (ILoggerFactory)new LoggerFactory();
            loggerFactory.AddSerilog(new LoggerConfiguration().WriteTo.Console().CreateLogger());

            var device = new FileSystemMediaServerDevice(stringToGuid(Environment.MachineName), Environment.MachineName, 
                "Online", "Filesystem Media Server", 
                1, 0, 0,
                libraryRootAbsolutePath);

            AnsiConsole.MarkupLine(localIp.Equals(IPAddress.Any)
                ? "[bold yellow]Server started on all network adapters[/]"
                : $"[bold yellow]Server started on local IP {localIp}[/]");

            AnsiConsole.MarkupLine("[bold yellow]Press ESC key to stop...[/]");
            AnsiConsole.WriteLine();

            var service = new CitpMediaServerService(loggerFactory.CreateLogger<CitpMediaServerService>(), device,
                CitpServiceFlags.UseLegacyMulticastIp, preferredTcpListenPort: 56676, localIp: localIp);

            var consoleKeyTask = Task.Run(() =>
            {
                ConsoleKeyInfo consoleKeyInfo;
                do
                {
                    consoleKeyInfo = Console.ReadKey(true);
                }
                while (consoleKeyInfo.Key != ConsoleKey.Escape);
            });

            await consoleKeyTask;

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold yellow]Server stopping...[/]");
            AnsiConsole.WriteLine();

            service.Dispose();

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold yellow]Server stopped. Press any key to exit...[/]");
            Console.ReadKey();

            return 0;
        }
    }



    public class Program
    {
        public static int Main(string[] args)
        {
            var app = new CommandApp();

            app.Configure(config => { config.AddCommand<RunCommand>("run"); });

            app.SetDefaultCommand<RunCommand>();

            AnsiConsole.Render(new FigletText("CITPSharp"));
            AnsiConsole.Render(new FigletText("File System Media Server"));

            return app.Run(args);
        }
    }
}
