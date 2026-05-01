# ArborBLASTOR

A simple command-line tool for sending Apple Push Notification service (APNs) alerts using JWT (`.p8` key) authentication.

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- An Apple `.p8` Auth Key file (`AuthKey_<KeyId>.p8`)
- A valid APNs device token from a registered device

---

## Configuration

The following APNs credentials are baked into the binary. To change them, edit `Program.cs`:

| Field      | Description                               |
|------------|-------------------------------------------|
| `CertFilePath` | Path to your `.p8` Auth Key file      |
| `KeyId`    | The 10-character Key ID from Apple        |
| `TeamId`   | Your Apple Developer Team ID              |
| `BundleId` | Your app's bundle identifier              |

The `.p8` file must be placed next to the compiled binary (it is copied automatically on build).

---

## Usage

```
ArborBLASTOR [options]
```

### Options

| Flag              | Description                              |
|-------------------|------------------------------------------|
| `--token <value>` | APNs device token to send to             |
| `--title <value>` | Notification title                       |
| `--body  <value>` | Notification body                        |
| `--help`, `-h`    | Show help message and exit               |

If any of `--token`, `--title`, or `--body` are omitted, you will be prompted to enter them interactively.

---

## Examples

**Fully non-interactive:**
```powershell
.\ArborBLASTOR --token "abc123def456" --title "Hello" --body "World"
```

**Partially interactive** (prompted for title and body):
```powershell
.\ArborBLASTOR --token "abc123def456"
```

**Fully interactive** (prompted for all values):
```powershell
.\ArborBLASTOR
```

**Show help:**
```powershell
.\ArborBLASTOR --help
```

---

## Building & Running

```powershell
# Build
dotnet build ArborBLASTOR.sln

# Run directly via dotnet
dotnet run --project ArborBLASTOR/ArborBLASTOR.csproj -- --token "abc123" --title "Hello" --body "World"

# Or run the compiled binary
.\ArborBLASTOR\bin\Debug\net10.0\ArborBLASTOR.exe --token "abc123" --title "Hello" --body "World"
```

---

## Notes

- Push notifications are sent to the **development** APNs server. Remove or change `push.SendToDevelopmentServer()` in `Program.cs` for production.
- The device token must come from a device registered with the same bundle ID and environment (development/production).

