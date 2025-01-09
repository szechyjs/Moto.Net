using System.ComponentModel;
using Moto.Net;
using Moto.Net.Mototrbo.FXP;
using Spectre.Console;
using Spectre.Console.Cli;

namespace TrboTools.Commands;

internal sealed class FeatureSettings : CommandSettings
{
  [CommandArgument(0, "<FEATURE_NAME>")]
  [Description("The feature to enable, must match name in features file")]
  public string FeatureName { get; init; }

  [CommandOption("-f|--features-file <FILE>")]
  [Description("Features database file")]
  [DefaultValue("features.json")]
  public string FeaturesFile { get; init; }
}

internal sealed class Feature : Command<FeatureSettings>
{
  public override int Execute(CommandContext context, FeatureSettings settings)
  {
    var features = FeatureList.LoadFile(settings.FeaturesFile);
    var feature = features.GetFeature(settings.FeatureName);
    if (feature == null)
    {
      Console.WriteLine("Feature not found");
      return 1;
    }

    var sys = new RadioSystem(0, 0);
    var radio = new LocalRadio(sys, System.Net.IPAddress.Parse("192.168.10.1"));
    if (!radio.InitXNL())
    {
      Console.WriteLine("Failed to initialize XNL");
      return 2;
    }

    var data = radio.Device.ValidationData();

    var hash = new byte[128];
    Array.Fill<byte>(hash, 165);
    Array.Copy(data, 0, hash, 0, data.Length);

    FXPClient client = new FXPClient();
    client.Enable("1", hash, feature.BinaryCode);
    radio.Reset();

    AnsiConsole.MarkupLine(":check_mark_button: Feature enabled!");
    AnsiConsole.MarkupLine("[dim]Rebooting radio...[/]");

    return 0;
  }
}
