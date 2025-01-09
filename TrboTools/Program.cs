namespace TrboTools;

using Spectre.Console.Cli;
using TrboTools.Commands;

class Program
{
    static int Main(string[] args)
    {
        var app = new CommandApp();

        app.Configure(config =>
        {
            config.SetApplicationName("trbotools");
            config.AddCommand<Enable>("enable")
                .WithDescription("Enable a feature on a radio")
                .WithExample(["enable", "NA_AESPRIVACY_SUB", "GLOBAL_SMA_Q4_2025_SUB"]);
        });

        return app.Run(args);
    }
}
