﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Pixsper.CitpSharp.DummyMediaServer;

public class RunCommand : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        var loggerFactory = (ILoggerFactory)new LoggerFactory();
        loggerFactory.AddSerilog(new LoggerConfiguration().WriteTo.Console().CreateLogger());

        var device = new DummyMediaServerDevice(Guid.NewGuid(), Environment.MachineName, "Online", "Dummy Media Server", 1, 0, 0);

        AnsiConsole.MarkupLine("[bold yellow]Server started on all network adapters[/]");

        AnsiConsole.MarkupLine("[bold yellow]Press ESC key to stop...[/]");
        AnsiConsole.WriteLine();

        var service = new CitpMediaServerService(loggerFactory.CreateLogger<CitpMediaServerService>(), device, 
            CitpServiceFlags.UseLegacyMulticastIp);
            
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

        app.Configure(config =>
        {
            config.AddCommand<RunCommand>("run");
        });

        app.SetDefaultCommand<RunCommand>();

        AnsiConsole.Render(new FigletText("CITPSharp"));
        AnsiConsole.Render(new FigletText("Dummy Media Server"));

        return app.Run(args);
    }
}