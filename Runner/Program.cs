﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace Runner;

public class Program
{
    // This method is needed for IDE previewer infrastructure
    static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .UseReactiveUI();

    // The entry point. Things aren't ready yet, so at this point you shouldn't use any Avalonia types or anything
    // that expects a SynchronizationContext to be ready
    public static void Main(string[] args) => BuildAvaloniaApp()
        .Start(AppMain, args);

    // Application entry point. Avalonia is completely initialized.
    static void AppMain(Application app, string[] args)
    {
        // A cancellation token source that will be used to stop the main loop
        CancellationTokenSource cts = new();

        // Do you startup code here
        new MainWindow().Show();

        // Start the main loop
        app.Run(cts.Token);
    }
}