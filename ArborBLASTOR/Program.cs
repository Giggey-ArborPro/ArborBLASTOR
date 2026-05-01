// See https://aka.ms/new-console-template for more information

using dotAPNS;

// ── Push type helpers ───────────────────────────────────────────────────────
var validPushTypes = Enum.GetValues<ApplePushType>()
    .Where(t => t != ApplePushType.Unknown)
    .ToArray();

static void PrintPushTypes(ApplePushType[] types)
{
    Console.WriteLine("Available push types:");
    for (int i = 0; i < types.Length; i++)
        Console.WriteLine($"  {i + 1}. {types[i]}");
}

// ── --list-push-types ───────────────────────────────────────────────────────
if (args.Contains("--list-push-types"))
{
    PrintPushTypes(validPushTypes);
    return;
}

// ── Help ────────────────────────────────────────────────────────────────────
if (args.Contains("--help") || args.Contains("-h"))
{
    Console.WriteLine("""
        ArborBLASTOR – Send APNs push notifications from the command line

        Usage:
          ArborBLASTOR [options]

        Options:
          --token      <device_token>   APNs device token to send to
          --title      <title>          Notification title
          --body       <body>           Notification body
          --push-type  <type>           Push type (e.g. Alert, Background, Voip)
          --cert       <path>           Path to .p8 auth key (defaults to AuthKey_D8T6LX3N8V.p8)
          --list-push-types             List all available push types and exit
          --help, -h                    Show this help message

        Examples:
          ArborBLASTOR --token abc123 --title "Hello" --body "World"
          ArborBLASTOR --token abc123 --push-type Background
          ArborBLASTOR --cert ~/keys/AuthKey_ABC123.p8 --token abc123 --title "Hi" --body "Hey"
          ArborBLASTOR --list-push-types
          ArborBLASTOR                         (prompts for all values)
        """);
    return;
}

// ── Argument parsing ────────────────────────────────────────────────────────
static string? GetArg(string[] args, string flag)
{
    var idx = Array.IndexOf(args, flag);
    return idx >= 0 && idx + 1 < args.Length ? args[idx + 1] : null;
}

var token = GetArg(args, "--token");
var title = GetArg(args, "--title");
var body  = GetArg(args, "--body");
var pushTypeArg = GetArg(args, "--push-type");
var certArg = GetArg(args, "--cert");

var certPath = !string.IsNullOrWhiteSpace(certArg)
    ? certArg
    : Path.Combine(AppContext.BaseDirectory, "AuthKey_D8T6LX3N8V.p8");

if (!File.Exists(certPath))
{
    Console.WriteLine($"Error: .p8 cert file not found at \"{certPath}\".");
    Console.WriteLine("Use --cert <path> to specify its location, or place AuthKey_D8T6LX3N8V.p8 next to the binary.");
    return;
}

// ── Interactive prompts for missing values ──────────────────────────────────
if (string.IsNullOrWhiteSpace(token))
{
    Console.Write("Enter Device Token: ");
    token = Console.ReadLine() ?? string.Empty;
}

if (string.IsNullOrWhiteSpace(title))
{
    Console.Write("Enter notification title: ");
    title = Console.ReadLine() ?? string.Empty;
}

if (string.IsNullOrWhiteSpace(body))
{
    Console.Write("Enter notification body: ");
    body = Console.ReadLine() ?? string.Empty;
}

ApplePushType pushType;
if (!string.IsNullOrWhiteSpace(pushTypeArg))
{
    if (!Enum.TryParse(pushTypeArg, ignoreCase: true, out pushType) || pushType == ApplePushType.Unknown)
    {
        Console.WriteLine($"Unknown push type \"{pushTypeArg}\". Run with --list-push-types to see valid options.");
        return;
    }
}
else
{
    PrintPushTypes(validPushTypes);
    Console.Write($"Select push type [1-{validPushTypes.Length}] (default: Alert): ");
    var input = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(input))
    {
        pushType = ApplePushType.Alert;
    }
    else if (int.TryParse(input, out var idx) && idx >= 1 && idx <= validPushTypes.Length)
    {
        pushType = validPushTypes[idx - 1];
    }
    else if (!Enum.TryParse(input, ignoreCase: true, out pushType) || pushType == ApplePushType.Unknown)
    {
        Console.WriteLine($"Invalid selection. Run with --list-push-types to see valid options.");
        return;
    }
}

// ── Send ────────────────────────────────────────────────────────────────────
var apns = ApnsClient.CreateUsingJwt(new HttpClient(), new ApnsJwtOptions
{
    CertFilePath = certPath,
    KeyId = "D8T6LX3N8V",
    TeamId = "BP42WMJ84Q",
    BundleId = "app.arborprousa.experimental",
});

var push = new ApplePush(pushType)
    .AddAlert(title, body)
    .AddToken(token);
push.SendToDevelopmentServer();

Console.WriteLine($"Sending → token={token}  title=\"{title}\"  body=\"{body}\"  type={pushType}");

try
{
    var response = await apns.Send(push);
    if (response.IsSuccessful)
    {
        Console.WriteLine("An alert push has been successfully sent!");
    }
    else
    {
        switch (response.Reason)
        {
            case ApnsResponseReason.BadCertificateEnvironment:
                // The client certificate is for the wrong environment.
                // TODO: retry on another environment
                break;
            // TODO: process other reasons we might be interested in
            default:
                throw new ArgumentOutOfRangeException(nameof(response.Reason), response.Reason, null);
        }
        Console.WriteLine("Failed to send a push, APNs reported an error: " + response.ReasonString);
    }
}
catch (TaskCanceledException)
{
    Console.WriteLine("Failed to send a push: HTTP request timed out.");
}
catch (HttpRequestException ex)
{
    Console.WriteLine("Failed to send a push. HTTP request failed: " + ex);
}
catch (ApnsCertificateExpiredException)
{
    Console.WriteLine("APNs certificate has expired. No more push notifications can be sent using it until it is replaced with a new one.");
}