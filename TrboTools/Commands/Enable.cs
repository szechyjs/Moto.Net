using System.ComponentModel;
using System.Configuration;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Moto.Net;
using Moto.Net.Mototrbo.FXP;
using Spectre.Console;
using Spectre.Console.Cli;

namespace TrboTools.Commands;

internal sealed class EnableSettings : CommandSettings
{
  [CommandArgument(0, "<FEATURE_NAME>")]
  [Description("The features to enable, must match names in features file")]
  public string[] FeatureNames { get; init; }

  [CommandOption("-f|--features-file <FILE>")]
  [Description("Features database file")]
  [DefaultValue("features.json")]
  public string FeaturesFile { get; init; }
}

internal sealed class Enable : Command<EnableSettings>
{
  public override int Execute(CommandContext context, EnableSettings settings)
  {
    var featureDB = FeatureList.LoadFile(settings.FeaturesFile);
    List<Feature> features = [];
    foreach (var name in settings.FeatureNames)
    {
      var feature = featureDB.GetFeature(name);
      if (feature == null)
      {
        AnsiConsole.MarkupLine(":magnifying_glass_tilted_left: [red]Feature not found:[/] {0}", name);
        return 1;
      }
      features.Add(feature);
    }

    var sys = new RadioSystem(0, 0);
    var radio = new LocalRadio(sys, System.Net.IPAddress.Parse("192.168.10.1"));
    if (!radio.InitXNL())
    {
      Console.WriteLine("Failed to initialize XNL");
      return 2;
    }

    var security = radio.QuerySecurityCapability();
    if (security == 2)
    {
        // Get secure IP and Port
        var ipPort = radio.QuerySecureIPandPort();

        // Establish TLS connection
        TcpClient tlsClient = new TcpClient(ipPort.RadioIP.ToString(), ipPort.RadioPort);
        SslStream tlsStream = new SslStream(tlsClient.GetStream(), false, CertCheck, UserCert, EncryptionPolicy.RequireEncryption);

        // Get client certificate
        var cpsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Motorola", "MOTOTRBO CPS 2.0");
        var certPath = Path.Combine(cpsPath, "Metadata", "PCR", "resources", "msi_pcr.pfx");
        string password = ConfigurationManager.AppSettings.Get("pfxPassword");
        var cert = new X509Certificate2(certPath, password);
        var certs = new X509Certificate2Collection(cert);

        try
        {
            tlsStream.AuthenticateAsClient("", certs, SslProtocols.Tls12, false);
        } catch (AuthenticationException e)
        {
            AnsiConsole.WriteException(e);
            if (e.InnerException != null)
            {
                AnsiConsole.WriteException(e.InnerException);
            }
            tlsClient.Close();
            return 3;
        }
    }

    var data = radio.Device.ValidationData();

    var hash = new byte[128];
    Array.Fill<byte>(hash, 165);
    Array.Copy(data, 0, hash, 0, data.Length);

    FXPClient client = new FXPClient();
    foreach (var feature in features)
    {
      try
      {
        client.Enable("1", hash, feature.BinaryCode);
        AnsiConsole.MarkupLine(":check_mark_button: {0} enabled!", feature.Name);
      }
      catch (Exception e)
      {
        AnsiConsole.MarkupLine(":cross_mark: [red]Failed to enable feature:[/] {0}", feature.Name);
        AnsiConsole.MarkupLine("[red]{0}[/]", e.Message);
        return 3;
      }

    }
    radio.Reset();

    AnsiConsole.MarkupLine("[dim]Rebooting radio...[/]");

    return 0;
  }

  bool CertCheck(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
  {
        //if (sslPolicyErrors == SslPolicyErrors.None) { return true; }
        //if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors) { return true; } //we don't have a proper certificate tree
        return true;
    }

    X509Certificate UserCert(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate? remoteCertificate, string[] acceptableIssuers)
    {
        if (localCertificates != null && localCertificates.Count > 0)
            return localCertificates[0];

        return null;
    }
}
